using System;
using Project06.Authoring;
using Project06.Commands;
using Project06.Definitions;
using Project06.Entities;
using Project06.Presentation;
using Project06.Presentation.Player.Views;
using Project06.Presentation.Player.ViewSystems;
using Project06.Presentation.Projectile.ViewSystems;
using Project06.Systems;
using UnityEngine;

namespace Project06.Core
{
    public class Runner : MonoBehaviour
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

            PlayerAuthoring playerAuthoring = FindFirstObjectByType<PlayerAuthoring>();
            if (playerAuthoring != null && _definitionCatalog != null)
            {
                Vector3 startPosition = playerAuthoring.transform.position;
                Vector3 startFacingDirection = playerAuthoring.transform.forward;
                startFacingDirection.y = 0f;
                _world.player = new PlayerEntity
                {
                    isAlive = true,
                    previousPosition = startPosition,
                    currentPosition = startPosition,
                    facingDirection = startFacingDirection.sqrMagnitude > 0f ? startFacingDirection.normalized : Vector3.forward,
                    movement = _definitionCatalog.player.movement,
                    weaponIndex = -1,
                };
                EquipStartWeapon(ref _world.player);
            }

            _viewContext.playerView = FindFirstObjectByType<PlayerView>();
            ProjectileViewSystem.Initialize(GetProjectilePrefabs(), _world.projectiles.entities.Length, _viewContext);
        }

        private void FixedUpdate()
        {
            CommandBuffer commandBuffer = _world.commandBuffer;
            CommandSystem.Tick(in commandBuffer.moveCommand, in commandBuffer.lookCommand, in commandBuffer.aimCommand, in commandBuffer.fireCommand, ref _world.player);
            PlayerMoveSystem.Tick(ref _world.player);
            ProjectileMoveSystem.Tick(_world.projectiles);
            PlayerFireSystem.Tick(ref _world.player, _world.projectileSpawnRequests, _world.playerFiredRecords);
            SpawnSystem.Tick(_world.projectileSpawnRequests, _world.projectiles);
        }

        private void Update()
        {
            Vector2 moveInput = _playerActions.General.Move.ReadValue<Vector2>();
            _world.commandBuffer.moveCommand.direction = new Vector3(moveInput.x, 0f, moveInput.y);
            _world.commandBuffer.moveCommand.run = _playerActions.General.Run.IsPressed();
            Vector2 lookInput = _playerActions.General.Look.ReadValue<Vector2>();
            _world.commandBuffer.lookCommand.direction = GetCursorDirection(lookInput, _world.player.currentPosition);
            _world.commandBuffer.aimCommand.aim = _playerActions.General.Aim.IsPressed();
            Vector3 muzzlePosition = GetMuzzlePosition();
            _world.commandBuffer.fireCommand.fire = _playerActions.General.Fire.IsPressed();
            _world.commandBuffer.fireCommand.origin = muzzlePosition;
            _world.commandBuffer.fireCommand.direction = GetCursorDirection(lookInput, muzzlePosition);
        }

        private void LateUpdate()
        {
            float interpolation = Mathf.Clamp01((Time.time - Time.fixedTime) / Time.fixedDeltaTime);
            PlayerViewSystem.Present(in _world.player, _world.playerFiredRecords, interpolation, _viewContext);
            ProjectileViewSystem.Present(_world.projectiles, interpolation, _viewContext);

            _world.playerFiredRecords.Clear();
        }

        // PlayerDefinition의 시작 무기를 Catalog에서 찾아 플레이어에 장착한다. 찾지 못하면 무기 없이 둔다.
        private void EquipStartWeapon(ref PlayerEntity player)
        {
            WeaponDefinition startWeapon = _definitionCatalog.player.startWeapon;
            WeaponDefinition[] weapons = _definitionCatalog.weapons;
            int weaponIndex = weapons != null && startWeapon != null ? Array.IndexOf(weapons, startWeapon) : -1;
            if (weaponIndex < 0)
            {
                Debug.LogError("PlayerDefinition의 시작 무기가 DefinitionCatalog.weapons에 없다.");
                return;
            }

            player.weaponIndex = weaponIndex;
            player.weapon = startWeapon.weapon;
        }

        // DefinitionCatalog.weapons 순서대로 투사체 프리팹을 모은다.
        private GameObject[] GetProjectilePrefabs()
        {
            WeaponDefinition[] weapons = _definitionCatalog != null ? _definitionCatalog.weapons : null;
            if (weapons == null)
            {
                return new GameObject[0];
            }

            GameObject[] prefabs = new GameObject[weapons.Length];
            for (int weaponIndex = 0; weaponIndex < weapons.Length; weaponIndex++)
            {
                prefabs[weaponIndex] = weapons[weaponIndex] != null ? weapons[weaponIndex].projectilePrefab : null;
            }

            return prefabs;
        }

        // 총구의 월드 위치를 구한다. 총구가 연결되어 있지 않으면 플레이어 위치를 쓴다.
        private Vector3 GetMuzzlePosition()
        {
            PlayerView playerView = _viewContext.playerView;
            if (playerView == null || playerView.weaponView == null || playerView.weaponView.muzzle == null)
            {
                return _world.player.currentPosition;
            }

            return playerView.weaponView.muzzle.position;
        }

        // 화면 좌표의 커서를 origin 높이의 수평면에 투영해, origin에서 그 지점으로 향하는 XZ 방향을 구한다.
        // 투영할 수 없으면 길이 0을 돌려줘, 받는 쪽이 기존 방향을 쓰게 한다.
        private static Vector3 GetCursorDirection(Vector2 screenPosition, Vector3 origin)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                return Vector3.zero;
            }

            Ray ray = mainCamera.ScreenPointToRay(screenPosition);
            Plane groundPlane = new Plane(Vector3.up, origin);
            if (!groundPlane.Raycast(ray, out float distance))
            {
                return Vector3.zero;
            }

            Vector3 direction = ray.GetPoint(distance) - origin;
            direction.y = 0f;
            return direction;
        }
    }
}
