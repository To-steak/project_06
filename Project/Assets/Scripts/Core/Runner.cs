using Project06.Authoring;
using Project06.Definitions;
using Project06.Entities;
using Project06.Presentation;
using Project06.Presentation.Player.Views;
using Project06.Presentation.Player.ViewSystems;
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
                };
            }

            _viewContext.playerView = FindFirstObjectByType<PlayerView>();
        }

        private void FixedUpdate()
        {
            CommandSystem.Tick(in _world.commandBuffer.moveCommand, in _world.commandBuffer.lookCommand, ref _world.player);
            PlayerMoveSystem.Tick(ref _world.player);
        }

        private void Update()
        {
            Vector2 moveInput = _playerActions.General.Move.ReadValue<Vector2>();
            _world.commandBuffer.moveCommand.direction = new Vector3(moveInput.x, 0f, moveInput.y);
            _world.commandBuffer.moveCommand.run = _playerActions.General.Run.IsPressed();
            Vector2 lookInput = _playerActions.General.Look.ReadValue<Vector2>();
            _world.commandBuffer.lookCommand.direction = GetLookDirection(lookInput, _world.player.currentPosition);
        }

        private void LateUpdate()
        {
            float interpolation = Mathf.Clamp01((Time.time - Time.fixedTime) / Time.fixedDeltaTime);
            PlayerViewSystem.Present(in _world.player, interpolation, _viewContext);
        }

        // 화면 좌표의 커서를 플레이어 높이의 수평면에 투영해, 플레이어에서 그 지점으로 향하는 XZ 방향을 구한다.
        // 투영할 수 없으면 길이 0을 돌려줘 바라보는 방향을 유지한다.
        private static Vector3 GetLookDirection(Vector2 screenPosition, Vector3 playerPosition)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                return Vector3.zero;
            }

            Ray ray = mainCamera.ScreenPointToRay(screenPosition);
            Plane groundPlane = new Plane(Vector3.up, playerPosition);
            if (!groundPlane.Raycast(ray, out float distance))
            {
                return Vector3.zero;
            }

            Vector3 direction = ray.GetPoint(distance) - playerPosition;
            direction.y = 0f;
            return direction;
        }
    }
}
