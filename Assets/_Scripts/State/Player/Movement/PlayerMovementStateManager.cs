using UnityEngine;

namespace Assets._Scripts.State.Player.Movement
{
    public class PlayerMovementStateManager : MonoBehaviour
    {
        public IState WalkingState { get; private set; }
        public IState JumpingState { get; private set; }

        [SerializeField] private PlayerMovementStateManager MovementState;

        private static PlayerMovementStateManager _instance;
        public static PlayerMovementStateManager Instance => _instance;

        private void Awake()
        {
            if (_instance is not null)
            {
                Destroy(_instance);
            }
            _instance = this;

            WalkingState = new WalkingState();
            JumpingState = new JumpingState();
        }

        public void TransitionToState(PlayerMovementStates state)
        {

        }
    }
}
