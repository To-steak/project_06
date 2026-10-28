using System;
using Project06.Authoring;
using Project06.Commands;
using Project06.Definitions;
using Project06.Entities;
using Project06.Presentation;
using Project06.Presentation.Player.Views;
using Project06.Presentation.Player.ViewSystems;
using Project06.Presentation.Projectile.ViewSystems;
using Project06.Presentation.Weapon.Views;
using Project06.Presentation.Weapon.ViewSystems;
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
        // 디버그 표시 영역의 화면 가장자리 여백과 크기(픽셀)
        private const float DEBUG_GUI_MARGIN = 10f;
        private const float DEBUG_GUI_WIDTH = 320f;
        private const float DEBUG_GUI_HEIGHT = 140f;

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
                    caliberIndex = -1,
                    reserveAmmo = CreateStartReserveAmmo(),
                };
                EquipStartWeapon(ref _world.player);
            }

            _viewContext.playerView = FindFirstObjectByType<PlayerView>();
            CollectWeaponPrefabs(out GameObject[] weaponPrefabs, out GameObject[] projectilePrefabs);
            WeaponViewSystem.Initialize(weaponPrefabs, _viewContext);
            ProjectileViewSystem.Initialize(projectilePrefabs, _world.projectiles.entities.Length, _viewContext);
        }

        private void FixedUpdate()
        {
            CommandBuffer commandBuffer = _world.commandBuffer;
            PlayerReloadSystem.Tick(in commandBuffer.reloadCommand, in commandBuffer.aimCommand, in commandBuffer.fireCommand, ref _world.player);
            commandBuffer.reloadCommand = default;
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
            PlayerViewSystem.Present(in _world.player, _world.playerFiredRecords, interpolation, _viewContext);
            WeaponViewSystem.Present(in _world.player, _world.playerFiredRecords, _viewContext);
            ProjectileViewSystem.Present(_world.projectiles, interpolation, _viewContext);

            _world.playerFiredRecords.Clear();
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        // 디버그용으로 플레이어의 무기·탄약·재장전·행동 상태를 화면 왼쪽 위에 표시한다. 에디터와 개발 빌드에서만 컴파일된다.
        private void OnGUI()
        {
            if (_world == null || !_world.player.isAlive)
            {
                return;
            }

            ref PlayerEntity player = ref _world.player;
            WeaponDefinition[] weapons = _definitionCatalog != null ? _definitionCatalog.weapons : null;
            CaliberDefinition[] calibers = _definitionCatalog != null ? _definitionCatalog.calibers : null;

            string weaponName = weapons != null && player.weaponIndex >= 0 && player.weaponIndex < weapons.Length && weapons[player.weaponIndex] != null
                ? weapons[player.weaponIndex].name
                : "없음";
            string caliberName = calibers != null && player.caliberIndex >= 0 && player.caliberIndex < calibers.Length && calibers[player.caliberIndex] != null
                ? calibers[player.caliberIndex].name
                : "없음";
            int reserveAmmo = player.reserveAmmo != null && player.caliberIndex >= 0 && player.caliberIndex < player.reserveAmmo.Length
                ? player.reserveAmmo[player.caliberIndex]
                : 0;
            string reloadText = player.isReloading
                ? $"재장전 중 {player.weapon.reloadTicks - player.reloadRemainingTicks} / {player.weapon.reloadTicks} 틱"
                : "재장전 안 함";

            GUILayout.BeginArea(new Rect(DEBUG_GUI_MARGIN, DEBUG_GUI_MARGIN, DEBUG_GUI_WIDTH, DEBUG_GUI_HEIGHT), GUI.skin.box);
            GUILayout.Label($"무기: {weaponName} ({caliberName})");
            GUILayout.Label($"탄창: {player.magazineAmmo} / {player.weapon.magazineCapacity}");
            GUILayout.Label($"예비 탄약: {reserveAmmo}");
            GUILayout.Label(reloadText);
            GUILayout.Label($"조준 {player.isAiming} · 발사 {player.isFiring} · 달리기 {player.isRunning}");
            GUILayout.EndArea();
        }
#endif

        // PlayerDefinition의 시작 무기를 Catalog에서 찾아 탄창을 가득 채운 채 플레이어에 장착한다. 찾지 못하면 무기 없이 둔다.
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
            player.magazineAmmo = startWeapon.weapon.magazineCapacity;

            CaliberDefinition[] calibers = _definitionCatalog.calibers;
            player.caliberIndex = calibers != null && startWeapon.caliber != null ? Array.IndexOf(calibers, startWeapon.caliber) : -1;
            if (player.caliberIndex < 0)
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
                return _world.player.currentPosition;
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
