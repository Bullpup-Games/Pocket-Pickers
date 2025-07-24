namespace _Scripts.Player.State
{
    public interface IPlayerState
    {
        void EnterState();
        void UpdateState();
        void FixedUpdateState();
        void ExitState();
    }
}