///
/// Centralized place to house all ENUM and Interface definitions
///
namespace Assets._Scripts
{
    #region ENUMS

    /// <summary>
    /// Possible movement states the player can be in
    /// </summary>
    public enum PlayerMovementStates
    {
        Walking,
        Jumping
    }

    #endregion

    #region Interfaces

    /// <summary>
    /// State interface.
    /// Currently used for Player Movement.
    /// Will be used for enemy movement, potentially other states as well
    /// </summary>
    public interface IState
    {
        void EnterState();
        void UpdateState();
        void FixedUpdateState();
        void ExitState();
    }

    #endregion
}