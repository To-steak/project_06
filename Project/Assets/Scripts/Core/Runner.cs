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
                _world.player = new PlayerEntity
                {
                    isAlive = true,
                    previousPosition = startPosition,
                    currentPosition = startPosition,
                    movement = _definitionCatalog.player.movement,
                };
            }

            _viewContext.playerView = FindFirstObjectByType<PlayerView>();
        }

        private void FixedUpdate()
        {
            CommandSystem.Tick(in _world.commandBuffer.moveCommand, ref _world.player);
            PlayerMoveSystem.Tick(ref _world.player);
        }

        private void Update()
        {
            Vector2 moveInput = _playerActions.General.Move.ReadValue<Vector2>();
            _world.commandBuffer.moveCommand.direction = new Vector3(moveInput.x, 0f, moveInput.y);
        }

        private void LateUpdate()
        {
            float interpolation = Mathf.Clamp01((Time.time - Time.fixedTime) / Time.fixedDeltaTime);
            PlayerViewSystem.Present(in _world.player, interpolation, _viewContext);
        }
    }
}
