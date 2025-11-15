using System.IO;
using _Scripts;
using _Scripts.Player;
using UnityEngine;

public class GameBootstrapper : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField] private SinManager _sinManager;
    [SerializeField] private GameStateManager _gameStateManager;
    [SerializeField] private SaveManager _saveManager;
    [SerializeField] private UIManager _uiManager;
    [SerializeField] private InputHandler _inputHandler;
    [SerializeField] private LevelLoader _levelLoader;

    [Header("Player")]
    [SerializeField] private PlayerVariables _playerVariables;

    [Header("Sin Settings")]
    [SerializeField] private int _winThreshold = 50;

    private SinEscapedWith _sinEscapedWith;

    private void Awake()
    {
        InitializeServices();
        InitializeManagers();
    }

    private void InitializeServices()
    {
        Debug.Log("GameBootstrapper: Registering services...");

        ServiceLocator.Instance.Register<SinManager>(_sinManager);
        ServiceLocator.Instance.Register<GameStateManager>(_gameStateManager);
        ServiceLocator.Instance.Register<SaveManager>(_saveManager);
        ServiceLocator.Instance.Register<UIManager>(_uiManager);
        ServiceLocator.Instance.Register<InputHandler>(_inputHandler);
        ServiceLocator.Instance.Register<LevelLoader>(_levelLoader);
        ServiceLocator.Instance.Register<PlayerVariables>(_playerVariables);

        _sinEscapedWith = FindObjectOfType<SinEscapedWith>();
        if (_sinEscapedWith != null)
        {
            ServiceLocator.Instance.Register<SinEscapedWith>(_sinEscapedWith);
        }
    }

    private void InitializeManagers()
    {
        Debug.Log("GameBootstrapper: Initializing managers...");

        _sinManager.WinThreshold = _winThreshold;

        if (File.Exists(Application.persistentDataPath + "/save.txt"))
        {
            _sinManager.PurgeSin();
        }

        _sinManager.Initialize();

        _gameStateManager.Initialize(
            _sinManager,
            _saveManager,
            _levelLoader,
            _playerVariables,
            _sinEscapedWith,
            _uiManager
        );

        Debug.Log("GameBootstrapper: Initialization complete!");
    }
}
