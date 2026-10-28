using System;
using System.Collections.Generic;
using Project06.Authoring;
using Project06.Commands;
using Project06.Definitions;
using Project06.Entities;
using Project06.Presentation;
using Project06.Presentation.Enemy.ViewSystems;
using Project06.Presentation.Player.Views;
using Project06.Presentation.Player.ViewSystems;
using Project06.Presentation.Projectile.ViewSystems;
using Project06.Presentation.Weapon.Views;
using Project06.Presentation.Weapon.ViewSystems;
using Project06.Requests;
using Project06.Systems;
using UnityEngine;

namespace Project06.Core
{
    public partial class Runner : MonoBehaviour
    {
        private World _world;
        private ViewContext _viewContext;
        private PlayerActions _playerActions;
        private DefinitionCatalog _definitionCatalog;

        private const string DEFINITION_CATALOG_PATH = "Definition Catalog";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Create()
        {
            GameObject runnerObject = new GameObject(nameof(Runner));
            DontDestroyOnLoad(runnerObject);
            runnerObject.AddComponent<Runner>();
        }

        private void Awake()
        {
            _playerActions = new PlayerActions();

            _definitionCatalog = Resources.Load<DefinitionCatalog>(DEFINITION_CATALOG_PATH);
            if (_definitionCatalog == null)
            {
                Debug.LogError($"Resources/{DEFINITION_CATALOG_PATH} 에셋을 찾지 못했다.");
            }
        }

        private void OnEnable()
        {
            _playerActions.General.Enable();
        }

        private void OnDisable()
        {
            _playerActions.General.Disable();
        }

        private void OnDestroy()
        {
            _playerActions.Dispose();
        }

        // BeforeSceneLoad에서 생성되므로 Start 시점에는 첫 씬이 로드되어 있다.
        private void Start()
        {
            _world = new World();
            _viewContext = new ViewContext();

            // 씬에 배치한 엔티티는 생성 요청만 넣고, 첫 틱의 SpawnSystem이 만든다.
            EnqueuePlayerSpawn();
            List<EnemyDefinition> enemyDefinitions = EnqueueEnemySpawnerSpawns();

            _viewContext.playerView = FindFirstObjectByType<PlayerView>();
            CollectWeaponPrefabs(out GameObject[] weaponPrefabs, out GameObject[] projectilePrefabs);
            WeaponViewSystem.Initialize(weaponPrefabs, _viewContext);
            ProjectileViewSystem.Initialize(projectilePrefabs, _world.projectiles.entities.Length, _viewContext);
            EnemyViewSystem.Initialize(CollectEnemyPrefabs(enemyDefinitions), _world.enemies.entities.Length, _viewContext);
        }

        private void FixedUpdate()
        {
            CommandBuffer commandBuffer = _world.commandBuffer;
            PlayerReloadSystem.Tick(in commandBuffer.reloadCommand, in commandBuffer.aimCommand, in commandBuffer.fireCommand, _world.players);
            commandBuffer.reloadCommand = default;
            CommandSystem.Tick(in commandBuffer.moveCommand, in commandBuffer.lookCommand, in commandBuffer.aimCommand, in commandBuffer.fireCommand, _world.players);
            PlayerMoveSystem.Tick(_world.players);
            EnemyBehaviorSystem.Tick(_world.enemies, _world.players, _world.random);
            ProjectileHitSystem.Tick(_world.projectiles, _world.enemies);
            ProjectileMoveSystem.Tick(_world.projectiles);
            PlayerFireSystem.Tick(_world.players, _world.projectileSpawnRequests, _world.playerFiredRecords);
            EnemySpawnerSystem.Tick(_world.enemySpawners, _world.enemies, _world.enemySpawnRequests, _world.random);
            SpawnSystem.Tick(
                _world.playerSpawnRequests, _world.players,
                _world.enemySpawnerSpawnRequests, _world.enemySpawners,
                _world.enemySpawnRequests, _world.enemies,
                _world.projectileSpawnRequests, _world.projectiles);
        }

        private void Update()
        {
            Vector2 moveInput = _playerActions.General.Move.ReadValue<Vector2>();
            _world.commandBuffer.moveCommand.direction = new Vector3(moveInput.x, 0f, moveInput.y);
            _world.commandBuffer.moveCommand.run = _playerActions.General.Run.IsPressed();
            Vector2 lookInput = _playerActions.General.Look.ReadValue<Vector2>();
            Vector3 muzzlePosition = GetMuzzlePosition();
            _world.commandBuffer.lookCommand.hasTarget = TryGetCursorPoint(lookInput, muzzlePosition.y, out Vector3 aimPosition);
            _world.commandBuffer.lookCommand.targetPosition = aimPosition;
            _world.commandBuffer.lookCommand.muzzleOffset = GetMuzzleOffset();
            _world.commandBuffer.aimCommand.aim = _playerActions.General.Aim.IsPressed();
            _world.commandBuffer.fireCommand.fire = _playerActions.General.Fire.IsPressed();
            _world.commandBuffer.fireCommand.origin = muzzlePosition;
            _world.commandBuffer.reloadCommand.reload |= _playerActions.General.Reload.WasPressedThisFrame();
            _world.commandBuffer.reloadCommand.cancel |= _playerActions.General.Cancel.WasPressedThisFrame();
        }

        private void LateUpdate()
        {
            float interpolation = Mathf.Clamp01((Time.time - Time.fixedTime) / Time.fixedDeltaTime);
            // 씬의 플레이어 View는 하나라서 살아 있는 첫 플레이어를 표시한다.
            int playerIndex = FindAlivePlayerIndex();
            if (playerIndex >= 0)
            {
                ref PlayerEntity player = ref _world.players.entities[playerIndex];
                PlayerViewSystem.Present(in player, _world.playerFiredRecords, interpolation, _viewContext);
                WeaponViewSystem.Present(in player, _world.playerFiredRecords, _viewContext);
            }

            ProjectileViewSystem.Present(_world.projectiles, interpolation, _viewContext);
            EnemyViewSystem.Present(_world.enemies, interpolation, _viewContext);

            _world.playerFiredRecords.Clear();
        }

        // 씬의 PlayerAuthoring 위치에 PlayerDefinition 수치로 플레이어 생성을 요청한다. PlayerAuthoring이나 Catalog가 없으면 요청하지 않는다.
        private void EnqueuePlayerSpawn()
        {
            PlayerAuthoring playerAuthoring = FindFirstObjectByType<PlayerAuthoring>();
            if (playerAuthoring == null || _definitionCatalog == null)
            {
                return;
            }

            Vector3 startFacingDirection = playerAuthoring.transform.forward;
            startFacingDirection.y = 0f;
            PlayerSpawnRequest request = new PlayerSpawnRequest
            {
                position = playerAuthoring.transform.position,
                facingDirection = startFacingDirection.sqrMagnitude > 0f ? startFacingDirection.normalized : Vector3.forward,
                movement = _definitionCatalog.player.movement,
                weaponIndex = -1,
                caliberIndex = -1,
                reserveAmmo = CreateStartReserveAmmo(),
            };
            EquipStartWeapon(ref request);
            _world.playerSpawnRequests.Enqueue(request);
        }

        // PlayerDefinition의 시작 무기를 Catalog에서 찾아 탄창을 가득 채운 채 생성 요청에 담는다. 찾지 못하면 무기 없이 둔다.
        private void EquipStartWeapon(ref PlayerSpawnRequest request)
        {
            WeaponDefinition startWeapon = _definitionCatalog.player.startWeapon;
            WeaponDefinition[] weapons = _definitionCatalog.weapons;
            int weaponIndex = weapons != null && startWeapon != null ? Array.IndexOf(weapons, startWeapon) : -1;
            if (weaponIndex < 0)
            {
                Debug.LogError("PlayerDefinition의 시작 무기가 DefinitionCatalog.weapons에 없다.");
                return;
            }

            request.weaponIndex = weaponIndex;
            request.weapon = startWeapon.weapon;
            request.magazineAmmo = startWeapon.weapon.magazineCapacity;

            CaliberDefinition[] calibers = _definitionCatalog.calibers;
            request.caliberIndex = calibers != null && startWeapon.caliber != null ? Array.IndexOf(calibers, startWeapon.caliber) : -1;
            if (request.caliberIndex < 0)
            {
                Debug.LogError($"{startWeapon.name}의 구경이 DefinitionCatalog.calibers에 없다. 이 무기는 재장전할 수 없다.");
            }
        }

        // DefinitionCatalog.calibers 순서대로 구경별 시작 예비 탄약 수를 모은다.
        private int[] CreateStartReserveAmmo()
        {
            CaliberDefinition[] calibers = _definitionCatalog.calibers;
            if (calibers == null)
            {
                return new int[0];
            }

            int[] reserveAmmo = new int[calibers.Length];
            for (int caliberIndex = 0; caliberIndex < calibers.Length; caliberIndex++)
            {
                reserveAmmo[caliberIndex] = calibers[caliberIndex] != null ? calibers[caliberIndex].startReserveAmmo : 0;
            }

            return reserveAmmo;
        }

        // 씬의 EnemySpawnerAuthoring마다 그 Authoring이 가리키는 EnemyDefinition의 적을 내는 스포너 생성을 요청한다. definition이 비어 있는 스포너는 건너뛴다.
        // 가리킨 EnemyDefinition을 중복 없이 모아 돌려준다. 적 종류 인덱스는 이 목록의 인덱스다.
        private List<EnemyDefinition> EnqueueEnemySpawnerSpawns()
        {
            List<EnemyDefinition> enemyDefinitions = new List<EnemyDefinition>();
            EnemySpawnerAuthoring[] spawnerAuthorings = FindObjectsByType<EnemySpawnerAuthoring>(FindObjectsSortMode.None);
            for (int authoringIndex = 0; authoringIndex < spawnerAuthorings.Length; authoringIndex++)
            {
                EnemySpawnerAuthoring spawnerAuthoring = spawnerAuthorings[authoringIndex];
                EnemyDefinition definition = spawnerAuthoring.definition;
                if (definition == null)
                {
                    Debug.LogError($"EnemySpawnerAuthoring '{spawnerAuthoring.name}'의 definition이 비어 있다.", spawnerAuthoring);
                    continue;
                }

                int enemyTypeIndex = enemyDefinitions.IndexOf(definition);
                if (enemyTypeIndex < 0)
                {
                    enemyTypeIndex = enemyDefinitions.Count;
                    enemyDefinitions.Add(definition);
                }

                Transform spawnerTransform = spawnerAuthoring.transform;
                Vector3 facingDirection = spawnerTransform.forward;
                facingDirection.y = 0f;
                _world.enemySpawnerSpawnRequests.Enqueue(new EnemySpawnerSpawnRequest
                {
                    position = spawnerTransform.position,
                    facingDirection = facingDirection.sqrMagnitude > 0f ? facingDirection.normalized : Vector3.forward,
                    spawner = spawnerAuthoring.spawner,
                    enemyTypeIndex = enemyTypeIndex,
                    enemy = definition.enemy,
                });
            }

            return enemyDefinitions;
        }

        // 적 종류 순서대로 적 프리팹을 모은다.
        private static GameObject[] CollectEnemyPrefabs(List<EnemyDefinition> enemyDefinitions)
        {
            GameObject[] enemyPrefabs = new GameObject[enemyDefinitions.Count];
            for (int enemyTypeIndex = 0; enemyTypeIndex < enemyDefinitions.Count; enemyTypeIndex++)
            {
                enemyPrefabs[enemyTypeIndex] = enemyDefinitions[enemyTypeIndex].enemyPrefab;
            }

            return enemyPrefabs;
        }

        // 살아 있는 첫 플레이어의 슬롯 인덱스를 구한다. 없으면 -1을 돌려준다.
        private int FindAlivePlayerIndex()
        {
            PlayerEntity[] players = _world.players.entities;
            for (int index = 0; index < players.Length; index++)
            {
                if (players[index].isAlive)
                {
                    return index;
                }
            }

            return -1;
        }

        // DefinitionCatalog.weapons 순서대로 총 프리팹과 투사체 프리팹을 모은다.
        private void CollectWeaponPrefabs(out GameObject[] weaponPrefabs, out GameObject[] projectilePrefabs)
        {
            WeaponDefinition[] weapons = _definitionCatalog != null && _definitionCatalog.weapons != null
                ? _definitionCatalog.weapons
                : new WeaponDefinition[0];

            weaponPrefabs = new GameObject[weapons.Length];
            projectilePrefabs = new GameObject[weapons.Length];
            for (int weaponIndex = 0; weaponIndex < weapons.Length; weaponIndex++)
            {
                WeaponDefinition weapon = weapons[weaponIndex];
                weaponPrefabs[weaponIndex] = weapon != null ? weapon.weaponPrefab : null;
                projectilePrefabs[weaponIndex] = weapon != null ? weapon.projectilePrefab : null;
            }
        }

        // 총구의 월드 위치를 구한다. 총구가 연결되어 있지 않으면 플레이어 위치를 쓴다.
        private Vector3 GetMuzzlePosition()
        {
            WeaponView weaponView = _viewContext.equippedWeaponView;
            if (weaponView == null || weaponView.muzzle == null)
            {
                int playerIndex = FindAlivePlayerIndex();
                return playerIndex >= 0 ? _world.players.entities[playerIndex].currentPosition : Vector3.zero;
            }

            return weaponView.muzzle.position;
        }

        // 플레이어 View 기준 총구의 로컬 위치를 구한다. 총구가 연결되어 있지 않으면 0을 돌려준다.
        private Vector3 GetMuzzleOffset()
        {
            PlayerView playerView = _viewContext.playerView;
            WeaponView weaponView = _viewContext.equippedWeaponView;
            if (playerView == null || weaponView == null || weaponView.muzzle == null)
            {
                return Vector3.zero;
            }

            return playerView.transform.InverseTransformPoint(weaponView.muzzle.position);
        }

        // 화면 좌표의 커서를 높이 height의 수평면에 투영한 월드 위치를 구한다. 투영할 수 없으면 false를 돌려준다.
        private static bool TryGetCursorPoint(Vector2 screenPosition, float height, out Vector3 point)
        {
            point = Vector3.zero;
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                return false;
            }

            Ray ray = mainCamera.ScreenPointToRay(screenPosition);
            Plane plane = new Plane(Vector3.up, new Vector3(0f, height, 0f));
            if (!plane.Raycast(ray, out float distance))
            {
                return false;
            }

            point = ray.GetPoint(distance);
            return true;
        }
    }
}
