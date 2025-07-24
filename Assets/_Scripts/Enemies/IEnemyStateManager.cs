namespace _Scripts.Enemies
{
    public interface IEnemyStateManager<T> : IEnemyStateManagerBase where T : class
    {
        void TransitionToState(IEnemyState<T> newState);
    }
}