using _Scripts;
using _Scripts.Player;
using UnityEngine;

public class GameStateManager : MonoBehaviour, IGameStateManager
{
    [SerializeField] private int _deathPenalty = 10;

    private SinManager _sinManager;
    private SaveManager _saveManager;
    private LevelLoader _levelLoader;
    private PlayerVariables _playerVariables;
    private SinEscapedWith _sinEscapedWith;
    private UIManager _uiManager;

    public bool IsDead { get; private set; }

    public void Initialize(
        SinManager sinManager,
        SaveManager saveManager,
        LevelLoader levelLoader,
        PlayerVariables playerVariables,
        SinEscapedWith sinEscapedWith,
        UIManager uiManager)
    {
        _sinManager = sinManager;
        _saveManager = saveManager;
        _levelLoader = levelLoader;
        _playerVariables = playerVariables;
        _sinEscapedWith = sinEscapedWith;
        _uiManager = uiManager;

        IsDead = false;
        Debug.Log("GameStateManager initialized");
    }

    public void EscapeLevel()
    {
        if (_playerVariables.sinHeld == 0) return;

        _sinEscapedWith.sinHeldOnEscape = _playerVariables.sinHeld;
        _sinEscapedWith.sinLeftInLevelOnEscape = _sinManager.RemainingSin;

        _playerVariables.sinHeld = 0;

        if (_sinManager.CheckWinCondition(_playerVariables.sinAccrued))
        {
            _levelLoader.LoadLevel(_levelLoader.credits);
            _saveManager.DeleteSaveFile();
            return;
        }

        _sinManager.RefreshSinLists();
        _saveManager.Cleanup();
        _levelLoader.LoadLevel(_levelLoader.escapeScreen);
    }

    public void Die()
    {
        int sinToDistribute = _playerVariables.sinHeld + _playerVariables.sinAccrued + _deathPenalty;
        _sinManager.RedistributeSin(sinToDistribute);

        _playerVariables.sinHeld = 0;
        _playerVariables.sinAccrued = 0;

        _saveManager.Cleanup();

        IsDead = true;

        _uiManager.ShowDeathPanel();
    }
}