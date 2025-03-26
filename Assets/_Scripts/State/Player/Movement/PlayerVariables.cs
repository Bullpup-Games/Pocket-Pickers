using Assets._Scripts.Stats;
using UnityEngine;

namespace Assets._Scripts.State.Player.Movement
{
    /// <summary>
    /// OBSOLETE CLASS - JUST HERE TO TAKE STUFF FROM!
    /// </summary>
    [System.Obsolete]
    public class PlayerVariables : MonoBehaviour
    {
        public bool isFacingRight = true;   // Start facing right by default

        // public bool inCardStance;
        [HideInInspector] public PlayerMovementStateManager stateManager;
        [HideInInspector] public BoxCollider2D Collider2D;
        [HideInInspector] public Rigidbody2D RigidBody2D;
        [SerializeField] public PlayerStats Stats;

        [HideInInspector] public float Time;

        #region Singleton

        public static PlayerVariables Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType(typeof(PlayerVariables)) as PlayerVariables;

                return _instance;
            }
            set
            {
                _instance = value;
            }
        }
        private static PlayerVariables _instance;
        #endregion

        public void FlipLocalScale()
        {
            var localScale = transform.localScale;
            localScale.x *= -1;
            transform.localScale = localScale;
        }

        private void Awake()
        {
            stateManager = GetComponent<PlayerMovementStateManager>();
            Collider2D = GetComponent<BoxCollider2D>();
            RigidBody2D = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            Time = UnityEngine.Time.time;
            isFacingRight = transform.localScale.x > 0;
        }
    }
}