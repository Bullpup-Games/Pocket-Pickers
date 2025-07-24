using System;
using System.Collections;
using _Scripts.Enemies.ViewTypes;
using _Scripts.Player;
using UnityEngine;

namespace _Scripts.Enemies.Sniper.State
{
    public class SniperStateManager : MonoBehaviour, IEnemyStateManager<SniperStateManager>
    {
        public IEnemyState<SniperStateManager> PatrollingState { get; private set; }
        public IEnemyState<SniperStateManager> ChargingState { get; private set; }
        public IEnemyState<SniperStateManager> ReloadingState { get; private set; }
        public IEnemyState<SniperStateManager> DisabledState { get; private set; }
        public IEnemyState<SniperStateManager> CurrentState { get; private set; }
        public IEnemyState<SniperStateManager> PreviousState { get; private set; }
        [SerializeField] private SniperState enumState;

        [HideInInspector] public SniperSettings Settings;
        [HideInInspector] public Rigidbody2D Rigidbody2D;
        [HideInInspector] public Collider2D Collider2D;
        [HideInInspector] public RayView RayView;
        public bool originallyFacingRight;
        public bool investigatingFalseTrigger;

        [HideInInspector] public LayerMask environmentLayer;
        [HideInInspector] public LayerMask enemyLayer;
        [HideInInspector] public LayerMask playerLayer;
        
        [Header("Sin Values")]
        public int sinPenalty;

        public bool alertedFromAggroSkreecher;
        public bool alertedFromInvestigatingSkreecher;
        private void Awake()
        {
            Debug.Log("SniperStateManager Awake");
            Settings = GetComponent<SniperSettings>();
            Rigidbody2D = GetComponent<Rigidbody2D>();
            Collider2D = GetComponent<Collider2D>();
            RayView = GetComponent<RayView>();
            
            environmentLayer = LayerMask.NameToLayer("Environment");
            playerLayer = LayerMask.NameToLayer("Enemy");
            playerLayer = LayerMask.NameToLayer("Player");

            PatrollingState = new SniperPatrollingState();
            ChargingState = new SniperChargingState();
            ReloadingState = new SniperReloadingState();
            DisabledState = new SniperDisabledState();

            // Set the initial state
            CurrentState = PatrollingState;
            CurrentState.EnterState(this);
        }

        private void Start() => originallyFacingRight = Settings.isFacingRight;

        private void Update()
        {
            // Update the current state via its UpdateState function
            CurrentState.UpdateState();
            
            if (IsPatrollingState())
            {
                enumState = SniperState.Patrolling;
            }
            else if (IsChargingState())
            {
                enumState = SniperState.Charging;
            }
            else if (IsReloadingState())
            {
                enumState = SniperState.Reloading;
            }
            else if (IsDisabledState())
            {
                enumState = SniperState.Disabled;
            }
        }
        public void TransitionToState(IEnemyState<SniperStateManager> newState)
        {
            // if (CurrentState == newState)
            //     return;
            
            // Disable state trap
            if (CurrentState == DisabledState)
            {
                Debug.Log("Tried to exit from DisabledState");
                // return;
            }

            PreviousState = CurrentState;
            CurrentState.ExitState();
            CurrentState = newState;
            CurrentState.EnterState(this); 
        }
        
        public void KillEnemy()
        {
            if (CurrentState == DisabledState) return;
            PlayerVariables.Instance.CommitSin(sinPenalty);
            TransitionToState(DisabledState);
        }

        public void KillEnemyWithoutGeneratingSin()
        {
            if (CurrentState == DisabledState) return;
            TransitionToState(DisabledState);
            StartCoroutine(DisableObjectWithDelay());
        }
        
        private IEnumerator DisableObjectWithDelay()
        {
            yield return new WaitForSeconds(0.75f);
            gameObject.SetActive(false);
        }

        
        public void AlertFromAggroSkreecher()
        {
            if (CurrentState == DisabledState) return;
            alertedFromAggroSkreecher = true;
            TransitionToState(ChargingState);
        }
        
        public void AlertFromInvestigatingSkreecher()
        {
            if (CurrentState == DisabledState) return;
            alertedFromInvestigatingSkreecher = true;
            TransitionToState(ChargingState);
        }
        
        public bool IsPlayerDetected() => RayView.IsPlayerDetectedThisFrame();
        public bool IsPlayerDetectedWithQuickDetect() => RayView.IsPlayerDetectedThisFrame() && RayView.QuickDetection();
        
        private void OnCollisionEnter2D(Collision2D col)
        {
            CurrentState.OnCollisionEnter2D(col);

            if (CurrentState == DisabledState || CurrentState == ReloadingState) return;

            if (col.gameObject.layer != playerLayer) return;

            Debug.Log("Player Col");
            if ((PlayerVariables.Instance.transform.position.x > transform.position.x && !Settings.isFacingRight) ||
                (PlayerVariables.Instance.transform.position.x < transform.position.x && Settings.isFacingRight))
            {
                Settings.FlipLocalScale();
            }

            RayView.ignoreSweepAngle = true;
        }

        #region State Getters

        public bool IsPatrollingState()
        {
            return CurrentState == PatrollingState;
        }
        public bool IsDetectingState()
        {
            return false;
        }
        public bool IsAlertedState()
        {
            return false;
        }
        public bool IsInvestigatingState()
        {
            return false;
        }
        public bool IsDisabledState()
        {
            return CurrentState == DisabledState;
        }
        
        // For alternate enemy types just return false

        // Guard-Specific
        public bool IsAggroState() // Guard & Bat
        {
            return false;
        }
        public bool IsStunnedState()
        {
            return false;
        }
        
        // Sniper Specific
        public bool IsChargingState()
        {
            return CurrentState == ChargingState;
        }
        public bool IsReloadingState()
        {
            return CurrentState == ReloadingState;
        }
        
        #endregion
    }
}