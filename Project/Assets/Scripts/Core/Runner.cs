using System;
using System.Collections.Generic;
using Project06.Authoring;
using Project06.Commands;
using Project06.Components;
using Project06.Definitions;
using Project06.Entities;
using Project06.Presentation;
using Project06.Presentation.Camera.Views;
using Project06.Presentation.Camera.ViewSystems;
using Project06.Presentation.Enemy.ViewSystems;
using Project06.Presentation.EnemyProjectile.ViewSystems;
using Project06.Presentation.Player.Views;
using Project06.Presentation.Player.ViewSystems;
using Project06.Presentation.Projectile.ViewSystems;
using Project06.Presentation.Sky.Views;
using Project06.Presentation.Sky.ViewSystems;
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
            PlayerAuthoring playerAuthoring = FindFirstObjectByType<PlayerAuthoring>();
            PlayerDefinition playerDefinition = playerAuthoring != null ? playerAuthoring.definition : null;
            EnqueuePlayerSpawn(playerAuthoring);
            EnqueueShelterSpawn();
            EnqueueDayNightSpawn();
            List<EnemyDefinition> enemyDefinitions = new List<EnemyDefinition>();
            EnqueueEnemySpawnerSpawns(enemyDefinitions);
            EnqueueBossSpawnerSpawns(enemyDefinitions);

            CollectWeaponViewData(out GameObject[] weaponPrefabs, out GameObject[] projectilePrefabs, out string[] meleeAttackTriggerNames);
            InitializePlayerView(playerAuthoring, meleeAttackTriggerNames);
            _viewContext.cameraView = FindFirstObjectByType<CameraView>();
            InitializeCamera(playerDefinition);
            InitializeSky();
            WeaponViewSystem.Initialize(weaponPrefabs, _viewContext);
            ProjectileViewSystem.Initialize(projectilePrefabs, _world.projectiles.entities.Length, _viewContext);
            CollectEnemyViewData(enemyDefinitions, out GameObject[] enemyPrefabs, out string[][] enemyAttackTriggerNames, out GameObject[] enemyProjectilePrefabs);
            EnemyViewSystem.Initialize(enemyPrefabs, enemyAttackTriggerNames, _world.enemies.entities.Length, _viewContext);
            EnemyProjectileViewSystem.Initialize(enemyProjectilePrefabs, _world.enemyProjectiles.entities.Length, _viewContext);
            InitializeUserInterface();
        }

        private void FixedUpdate()
        {
            // 승패가 나면 시뮬레이션을 멈춘다.
            if (_world.gameResult != GameResult.Playing)
            {
                return;
            }

            DayNightSystem.Tick(_world.dayNights, _world.enemySpawners, _world.enemies);
            CommandBuffer commandBuffer = _world.commandBuffer;
            PlayerReloadSystem.Tick(in commandBuffer.reloadCommand, in commandBuffer.fireCommand, _world.players);
            commandBuffer.reloadCommand = default;
            CommandSystem.Tick(in commandBuffer.moveCommand, in commandBuffer.lookCommand, in commandBuffer.aimCommand, in commandBuffer.fireCommand, _world.players);
            PlayerMeleeSystem.Tick(_world.players, _world.enemies, _world.shelters, _world.random, _world.playerMeleeStartedRecords, _world.enemyHitStunnedRecords);
            PlayerMoveSystem.Tick(_world.players);
            EnemyBehaviorSystem.Tick(_world.enemies, _world.players, _world.shelters);
            EnemyAttackSystem.Tick(_world.enemies, _world.players, _world.shelters, _world.enemyProjectileSpawnRequests, _world.enemyAttackStartedRecords);
            EnemySeparationSystem.Tick(_world.enemies, _world.players, _world.shelters);
            ProjectileHitSystem.Tick(_world.projectiles, _world.enemies, _world.random, _world.enemyHitStunnedRecords);
            ProjectileMoveSystem.Tick(_world.projectiles);
            EnemyProjectileHitSystem.Tick(_world.enemyProjectiles, _world.players, _world.shelters);
            EnemyProjectileMoveSystem.Tick(_world.enemyProjectiles);
            PlayerSanitySystem.Tick(_world.players, _world.shelters);
            PlayerDeathSystem.Tick(_world.players);
            ShelterDestroySystem.Tick(_world.shelters);
            GameResultSystem.Tick(_world.enemies, _world.shelters, ref _world.gameResult);
            PlayerFireSystem.Tick(_world.players, _world.projectileSpawnRequests, _world.playerFiredRecords);
            EnemySpawnerSystem.Tick(_world.dayNights, _world.enemySpawners, _world.enemies, _world.enemySpawnRequests, _world.random);
            BossSpawnerSystem.Tick(_world.dayNights, _world.bossSpawners, _world.enemySpawnRequests);
            SpawnSystem.Tick(
                _world.playerSpawnRequests, _world.players,
                _world.shelterSpawnRequests, _world.shelters,
                _world.dayNightSpawnRequests, _world.dayNights,
                _world.enemySpawnerSpawnRequests, _world.enemySpawners,
                _world.bossSpawnerSpawnRequests, _world.bossSpawners,
                _world.enemySpawnRequests, _world.enemies,
                _world.projectileSpawnRequests, _world.projectiles,
                _world.enemyProjectileSpawnRequests, _world.enemyProjectiles);
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
                PlayerViewSystem.Present(in player, _world.playerFiredRecords, _world.playerMeleeStartedRecords, interpolation, _viewContext);
                WeaponViewSystem.Present(in player, _world.playerFiredRecords, _viewContext);
                CameraViewSystem.Present(in player, _playerActions.General.Look.ReadValue<Vector2>(), _viewContext);
            }

            int dayNightIndex = FindAliveDayNightIndex();
            if (dayNightIndex >= 0)
            {
                SkyViewSystem.Present(in _world.dayNights.entities[dayNightIndex], interpolation, _viewContext);
            }

            ProjectileViewSystem.Present(_world.projectiles, interpolation, _viewContext);
            EnemyViewSystem.Present(_world.enemies, _world.enemyAttackStartedRecords, _world.enemyHitStunnedRecords, interpolation, _viewContext);
            EnemyProjectileViewSystem.Present(_world.enemyProjectiles, interpolation, _viewContext);
            PresentUserInterface(playerIndex, dayNightIndex);

            _world.playerFiredRecords.Clear();
            _world.playerMeleeStartedRecords.Clear();
            _world.enemyAttackStartedRecords.Clear();
            _world.enemyHitStunnedRecords.Clear();
        }

        // 씬의 PlayerAuthoring 위치에 그 Authoring이 가리키는 PlayerDefinition 수치로 플레이어 생성을 요청한다.
        // PlayerAuthoring이나 PlayerDefinition이 없거나, 무기와 구경을 찾을 Catalog가 없으면 요청하지 않는다.
        private void EnqueuePlayerSpawn(PlayerAuthoring playerAuthoring)
        {
            if (playerAuthoring == null || _definitionCatalog == null)
            {
                return;
            }

            PlayerDefinition playerDefinition = playerAuthoring.definition;
            if (playerDefinition == null)
            {
                Debug.LogError("PlayerAuthoring의 definition이 비어 있다.", playerAuthoring);
                return;
            }

            PlayerSpawnRequest request = new PlayerSpawnRequest
            {
                position = playerAuthoring.transform.position,
                facingDirection = GetStartFacingDirection(playerAuthoring),
                player = playerDefinition.player,
                movement = playerDefinition.movement,
                weaponIndex = -1,
                caliberIndex = -1,
                reserveAmmo = CreateStartReserveAmmo(),
            };
            EquipStartWeapon(ref request, playerDefinition.startWeapon);
            _world.playerSpawnRequests.Enqueue(request);
        }

        // PlayerAuthoring 위치와 방향에 PlayerDefinition의 프리팹으로 플레이어 View를 만든다. PlayerAuthoring이나 프리팹이 없으면 만들지 않는다.
        private void InitializePlayerView(PlayerAuthoring playerAuthoring, string[] meleeAttackTriggerNames)
        {
            if (playerAuthoring == null || playerAuthoring.definition == null)
            {
                return;
            }

            GameObject playerPrefab = playerAuthoring.definition.playerPrefab;
            if (playerPrefab == null)
            {
                Debug.LogError($"{playerAuthoring.definition.name}의 playerPrefab이 비어 있다.", playerAuthoring.definition);
                return;
            }

            Quaternion rotation = Quaternion.LookRotation(GetStartFacingDirection(playerAuthoring), Vector3.up);
            PlayerViewSystem.Initialize(playerPrefab, playerAuthoring.transform.position, rotation, meleeAttackTriggerNames, _viewContext);
        }

        // PlayerAuthoring이 바라보는 방향을 XZ 평면에 눕혀 길이 1로 만든다. 바로 위나 아래를 보면 Z+를 쓴다.
        private static Vector3 GetStartFacingDirection(PlayerAuthoring playerAuthoring)
        {
            Vector3 startFacingDirection = playerAuthoring.transform.forward;
            startFacingDirection.y = 0f;
            return startFacingDirection.sqrMagnitude > 0f ? startFacingDirection.normalized : Vector3.forward;
        }

        // 씬의 ShelterAuthoring 위치에 그 Authoring이 가리키는 ShelterDefinition 수치로 쉘터 생성을 요청한다. ShelterAuthoring이나 ShelterDefinition이 없으면 요청하지 않는다.
        private void EnqueueShelterSpawn()
        {
            ShelterAuthoring shelterAuthoring = FindFirstObjectByType<ShelterAuthoring>();
            if (shelterAuthoring == null)
            {
                return;
            }

            if (shelterAuthoring.definition == null)
            {
                Debug.LogError("ShelterAuthoring의 definition이 비어 있다.", shelterAuthoring);
                return;
            }

            _world.shelterSpawnRequests.Enqueue(new ShelterSpawnRequest
            {
                position = shelterAuthoring.transform.position,
                shelter = shelterAuthoring.definition.shelter,
            });
        }

        // DayNightDefinition 수치로 낮밤 생성을 요청한다. DayNightDefinition이 없으면 요청하지 않으며, 그러면 적 스포너가 항상 적을 낸다.
        private void EnqueueDayNightSpawn()
        {
            if (_definitionCatalog == null)
            {
                return;
            }

            if (_definitionCatalog.dayNight == null)
            {
                Debug.LogError("DefinitionCatalog의 dayNight가 비어 있다.", _definitionCatalog);
                return;
            }

            _world.dayNightSpawnRequests.Enqueue(new DayNightSpawnRequest
            {
                dayNight = _definitionCatalog.dayNight.dayNight,
            });
        }

        // PlayerDefinition의 시작 무기를 Catalog에서 찾아 탄창을 가득 채운 채 생성 요청에 담는다. 찾지 못하면 무기 없이 둔다. 근접 무기는 구경 없이 둔다.
        private void EquipStartWeapon(ref PlayerSpawnRequest request, WeaponDefinition startWeapon)
        {
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

            // 근접 무기는 탄약을 쓰지 않으므로 구경을 찾지 않는다.
            if (startWeapon.weapon.kind != WeaponKind.Ranged)
            {
                return;
            }

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
        // 가리킨 EnemyDefinition을 enemyDefinitions에 중복 없이 더한다. 적 종류 인덱스는 이 목록의 인덱스다.
        private void EnqueueEnemySpawnerSpawns(List<EnemyDefinition> enemyDefinitions)
        {
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

                int enemyTypeIndex = GetEnemyTypeIndex(enemyDefinitions, definition);
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
        }

        // 씬의 BossSpawnerAuthoring마다 그 Authoring이 가리키는 EnemyDefinition의 보스를 내는 스포너 생성을 요청한다. definition이 비어 있는 스포너는 건너뛴다.
        // 가리킨 EnemyDefinition을 enemyDefinitions에 중복 없이 더한다. 적 종류 인덱스는 이 목록의 인덱스다.
        private void EnqueueBossSpawnerSpawns(List<EnemyDefinition> enemyDefinitions)
        {
            BossSpawnerAuthoring[] bossSpawnerAuthorings = FindObjectsByType<BossSpawnerAuthoring>(FindObjectsSortMode.None);
            for (int authoringIndex = 0; authoringIndex < bossSpawnerAuthorings.Length; authoringIndex++)
            {
                BossSpawnerAuthoring bossSpawnerAuthoring = bossSpawnerAuthorings[authoringIndex];
                EnemyDefinition definition = bossSpawnerAuthoring.definition;
                if (definition == null)
                {
                    Debug.LogError($"BossSpawnerAuthoring '{bossSpawnerAuthoring.name}'의 definition이 비어 있다.", bossSpawnerAuthoring);
                    continue;
                }

                int enemyTypeIndex = GetEnemyTypeIndex(enemyDefinitions, definition);
                Transform spawnerTransform = bossSpawnerAuthoring.transform;
                Vector3 facingDirection = spawnerTransform.forward;
                facingDirection.y = 0f;
                _world.bossSpawnerSpawnRequests.Enqueue(new BossSpawnerSpawnRequest
                {
                    position = spawnerTransform.position,
                    facingDirection = facingDirection.sqrMagnitude > 0f ? facingDirection.normalized : Vector3.forward,
                    spawner = bossSpawnerAuthoring.spawner,
                    enemyTypeIndex = enemyTypeIndex,
                    enemy = definition.enemy,
                });
            }
        }

        // enemyDefinitions에서 definition의 인덱스를 찾는다. 없으면 공격 목록을 검사한 뒤 끝에 더하고 그 인덱스를 돌려준다.
        private static int GetEnemyTypeIndex(List<EnemyDefinition> enemyDefinitions, EnemyDefinition definition)
        {
            int enemyTypeIndex = enemyDefinitions.IndexOf(definition);
            if (enemyTypeIndex < 0)
            {
                enemyTypeIndex = enemyDefinitions.Count;
                enemyDefinitions.Add(definition);
                ValidateEnemyAttacks(definition);
            }

            return enemyTypeIndex;
        }

        // 적 종류 순서대로 적 프리팹, 공격 Trigger 이름 목록, 적 투사체 프리팹을 모은다.
        private static void CollectEnemyViewData(List<EnemyDefinition> enemyDefinitions, out GameObject[] enemyPrefabs, out string[][] attackTriggerNames, out GameObject[] enemyProjectilePrefabs)
        {
            enemyPrefabs = new GameObject[enemyDefinitions.Count];
            attackTriggerNames = new string[enemyDefinitions.Count][];
            enemyProjectilePrefabs = new GameObject[enemyDefinitions.Count];
            for (int enemyTypeIndex = 0; enemyTypeIndex < enemyDefinitions.Count; enemyTypeIndex++)
            {
                enemyPrefabs[enemyTypeIndex] = enemyDefinitions[enemyTypeIndex].enemyPrefab;
                attackTriggerNames[enemyTypeIndex] = enemyDefinitions[enemyTypeIndex].attackTriggerNames;
                enemyProjectilePrefabs[enemyTypeIndex] = enemyDefinitions[enemyTypeIndex].projectilePrefab;
            }
        }

        // 적 Definition의 공격 목록과 Trigger 이름 목록의 길이가 다르면 에러 로그를 남긴다.
        private static void ValidateEnemyAttacks(EnemyDefinition definition)
        {
            int attackCount = definition.enemy.attacks != null ? definition.enemy.attacks.Length : 0;
            int triggerCount = definition.attackTriggerNames != null ? definition.attackTriggerNames.Length : 0;
            if (triggerCount != attackCount)
            {
                Debug.LogError($"{definition.name}의 공격 수({attackCount})와 attackTriggerNames 수({triggerCount})가 다르다.", definition);
            }
        }

        // PlayerDefinition의 조준 카메라 수치로 카메라 View를 준비한다. PlayerDefinition이 없으면 카메라를 옮기지 않는다. InitializePlayerView 다음에 호출한다.
        private void InitializeCamera(PlayerDefinition playerDefinition)
        {
            if (playerDefinition == null)
            {
                CameraViewSystem.Initialize(Camera.main, 0f, 0f, 0f, 0f, _viewContext);
                return;
            }

            CameraViewSystem.Initialize(Camera.main, playerDefinition.aimCameraDeadZone, playerDefinition.aimCameraMaxDistance, playerDefinition.aimCameraDistance, playerDefinition.aimCameraSmoothTime, _viewContext);
        }

        // SkyDefinition의 조명 수치로 하늘 View를 준비한다. SkyDefinition이 없으면 해와 달만 돌리고 조명은 바꾸지 않는다.
        private void InitializeSky()
        {
            SkyView skyView = FindFirstObjectByType<SkyView>();
            SkyDefinition skyDefinition = _definitionCatalog != null ? _definitionCatalog.sky : null;
            if (skyDefinition == null)
            {
                if (_definitionCatalog != null)
                {
                    Debug.LogError("DefinitionCatalog의 sky가 비어 있다.", _definitionCatalog);
                }

                SkyViewSystem.Initialize(skyView, 0, null, null, default, null, null, null, null, null, _viewContext);
                return;
            }

            SkyViewSystem.Initialize(
                skyView, skyDefinition.twilightTicks,
                skyDefinition.sunColor, skyDefinition.sunIntensity, skyDefinition.moonColor, skyDefinition.moonIntensity,
                skyDefinition.ambientSkyColor, skyDefinition.ambientEquatorColor, skyDefinition.ambientGroundColor, skyDefinition.fogColor,
                _viewContext);
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

        // 살아 있는 첫 낮밤의 슬롯 인덱스를 구한다. 없으면 -1을 돌려준다.
        private int FindAliveDayNightIndex()
        {
            DayNightEntity[] dayNights = _world.dayNights.entities;
            for (int index = 0; index < dayNights.Length; index++)
            {
                if (dayNights[index].isAlive)
                {
                    return index;
                }
            }

            return -1;
        }

        // DefinitionCatalog.weapons 순서대로 무기 프리팹, 투사체 프리팹, 근접 공격 Trigger 이름을 모은다.
        private void CollectWeaponViewData(out GameObject[] weaponPrefabs, out GameObject[] projectilePrefabs, out string[] meleeAttackTriggerNames)
        {
            WeaponDefinition[] weapons = _definitionCatalog != null && _definitionCatalog.weapons != null
                ? _definitionCatalog.weapons
                : new WeaponDefinition[0];

            weaponPrefabs = new GameObject[weapons.Length];
            projectilePrefabs = new GameObject[weapons.Length];
            meleeAttackTriggerNames = new string[weapons.Length];
            for (int weaponIndex = 0; weaponIndex < weapons.Length; weaponIndex++)
            {
                WeaponDefinition weapon = weapons[weaponIndex];
                weaponPrefabs[weaponIndex] = weapon != null ? weapon.weaponPrefab : null;
                projectilePrefabs[weaponIndex] = weapon != null ? weapon.projectilePrefab : null;
                meleeAttackTriggerNames[weaponIndex] = weapon != null ? weapon.meleeAttackTriggerName : null;
            }
        }

        // 총구의 월드 위치를 구한다. 총이 아니거나 총구가 연결되어 있지 않으면 플레이어 위치를 쓴다.
        private Vector3 GetMuzzlePosition()
        {
            GunView gunView = _viewContext.equippedGunView;
            if (gunView == null || gunView.muzzle == null)
            {
                int playerIndex = FindAlivePlayerIndex();
                return playerIndex >= 0 ? _world.players.entities[playerIndex].currentPosition : Vector3.zero;
            }

            return gunView.muzzle.position;
        }

        // 플레이어 View 기준 총구의 로컬 위치를 구한다. 총이 아니거나 총구가 연결되어 있지 않으면 0을 돌려준다.
        private Vector3 GetMuzzleOffset()
        {
            PlayerView playerView = _viewContext.playerView;
            GunView gunView = _viewContext.equippedGunView;
            if (playerView == null || gunView == null || gunView.muzzle == null)
            {
                return Vector3.zero;
            }

            return playerView.transform.InverseTransformPoint(gunView.muzzle.position);
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
