using System;
using _Scripts.Card;
using _Scripts.Player;
using _Scripts.Player.State;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Scripts
{
    public class InputHandler : MonoBehaviour
    {
        public Vector2 MovementInput { get; private set; }
        public Vector2 LookInput { get; private set; }
        public bool JumpPressed { get; private set; }
        public bool JumpHeld { get; private set; }
        
        [SerializeField] private UnityEngine.Camera gameCamera;
        private bool _isUsingMouse = false;
        
        #region Singleton

        public static InputHandler Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType(typeof(InputHandler)) as InputHandler;

                return _instance;
            }
            set { _instance = value; }
        }

        private static InputHandler _instance;

        #endregion

        private PlayerInputActions _inputActions;

        private void OnEnable()
        {
            if (_inputActions == null)
            {
                _inputActions = new PlayerInputActions();
            }

            _inputActions.Player.Enable();
            _inputActions.UI.Enable();

            // Subscribe to input events
            _inputActions.Player.Aim.performed += OnLookPerformed;
            _inputActions.Player.Aim.canceled += OnLookCanceled;

            _inputActions.Player.Throw.performed += OnThrowPerformed;
            _inputActions.Player.CancelCardThrow.performed += OnCancelCardThrow;
            _inputActions.Player.FalseTrigger.performed += OnFalseTriggerPerformed;
            
            _inputActions.Player.Move.performed += OnMovePerformed;
            _inputActions.Player.Move.canceled += OnMoveCanceled;
            _inputActions.Player.Jump.performed += OnJumpPerformed;
            _inputActions.Player.Jump.canceled += OnJumpCanceled;

            _inputActions.Player.Dash.performed += OnDashPerformed;
            _inputActions.Player.Crouch.performed += OnCrouchPerformed;
            
            _inputActions.UI.PauseEvent.performed += OnPausePerformed;
        }

        private void OnDisable()
        {
            _inputActions.Player.Aim.performed -= OnLookPerformed;
            _inputActions.Player.Aim.canceled -= OnLookCanceled;

            _inputActions.Player.Throw.performed -= OnThrowPerformed;
            _inputActions.Player.CancelCardThrow.performed -= OnCancelCardThrow;
            _inputActions.Player.FalseTrigger.performed -= OnFalseTriggerPerformed;
            
            _inputActions.Player.Move.performed -= OnMovePerformed;
            _inputActions.Player.Move.canceled -= OnMoveCanceled;
            _inputActions.Player.Jump.performed -= OnJumpPerformed;
            _inputActions.Player.Jump.canceled -= OnJumpCanceled;

            _inputActions.Player.Dash.performed -= OnDashPerformed;
            _inputActions.Player.Crouch.performed -= OnCrouchPerformed;
            
            _inputActions.UI.PauseEvent.performed -= OnPausePerformed;
            _inputActions.Player.Disable();
            _inputActions.UI.Disable();
        }
        
        private void Start()
        {
            if (gameCamera == null)
                gameCamera = UnityEngine.Camera.main;
        }
        
        private void Update()
        {
            if (GameManager.Instance is null) 
                return;
            if (GameManager.Instance.isDead)
                return;
            
            // Handle input based on input type
            Vector2 rawAimInput = _inputActions.Player.Aim.ReadValue<Vector2>();
            
            // Detect if we're using mouse input (position values > typical joystick range)
            _isUsingMouse = rawAimInput.magnitude > 100f;
            
            if (_isUsingMouse)
            {
                // Convert mouse screen position to direction relative to player
                LookInput = GetMouseDirectionToPlayer(rawAimInput);
            }
            else
            {
                // Use gamepad stick input directly
                LookInput = rawAimInput;
            }

            // Invoke CardStanceDirectionalInput event if necessary
            if (!PlayerStateManager.Instance.IsStunnedState() && !CardManager.Instance.IsCardInScene())
            {
                if (LookInput.magnitude > 0.1f)
                {
                    Vector2 inputDirection = LookInput.normalized;
                    CardStanceDirectionalInput?.Invoke(inputDirection);
                }
                else
                {
                    CardStanceDirectionalInput?.Invoke(Vector2.zero);
                }
            }
            
            // Reset JumpPressed after it has been read
            if (JumpPressed)
            {
                JumpPressed = false;
            }
        }

        // Event for updating direction while in card stance
        public event Action<Vector2> CardStanceDirectionalInput;

        // Event for handling card throw
        public event Action OnCardThrow;

        private Vector2 _lookInput;

        // public Vector2 LookInput() => _lookInput;

        private Vector2 GetMouseDirectionToPlayer(Vector2 mouseScreenPosition)
        {
            if (gameCamera == null || PlayerVariables.Instance == null)
                return Vector2.zero;
                
            // Convert player world position to screen position
            Vector3 playerScreenPos = gameCamera.WorldToScreenPoint(PlayerVariables.Instance.transform.position);
            
            // Calculate direction from player to mouse
            Vector2 direction = (mouseScreenPosition - (Vector2)playerScreenPos).normalized;
            
            return direction;
        }
        
        private void OnLookPerformed(InputAction.CallbackContext context)
        {
            if (GameManager.Instance is null) 
                return;
            if (GameManager.Instance.isDead)
                return;
            
            if (PlayerStateManager.Instance.IsStunnedState()) 
                return;
            if (CardManager.Instance.IsCardInScene()) 
                return;

            Vector2 rawInput = context.ReadValue<Vector2>();
            
            // Check if this is mouse input (large position values)
            if (rawInput.magnitude > 100f)
            {
                _lookInput = GetMouseDirectionToPlayer(rawInput);
            }
            else
            {
                _lookInput = rawInput;
            }

            if (_lookInput.magnitude > 0.1f)
            {
                Vector2 inputDirection = _lookInput.normalized;
                CardStanceDirectionalInput?.Invoke(inputDirection);
            }
            else
            {
                CardStanceDirectionalInput?.Invoke(Vector2.zero);
            }
        }

        private void OnLookCanceled(InputAction.CallbackContext context)
        {
            CardStanceDirectionalInput?.Invoke(Vector2.zero);
        }

        private void OnThrowPerformed(InputAction.CallbackContext context)
        {
            // if (PlayerStateManager.Instance.IsStunnedState()) return;
            Debug.Log($"Throw Input from: {context.control.device}");
            if (GameManager.Instance is null) 
                return;
            if (!GameManager.Instance.isDead)
                OnCardThrow?.Invoke();
        }
        
        public event Action OnFalseTrigger;
        private void OnFalseTriggerPerformed(InputAction.CallbackContext context)
        
        {
                /*
                 * The False trigger input is used to escape stuns. Even if it wasn't, it would be a clever way
                 * of escaping one regardless if the player already has an active card out and near the enemy.
                 * So, allow FalseTrigger input even if the player is stunned
                 */
                // Debug.Log("False Trigger Input");
                OnFalseTrigger?.Invoke();
        }

        public event Action OnCancelActiveCard;
        private void OnCancelCardThrow(InputAction.CallbackContext context)
        {
            // Debug.Log("Cancel throw");
            OnCancelActiveCard?.Invoke();
        }

        public event Action OnDash;

        private void OnDashPerformed(InputAction.CallbackContext context)
        {
            OnDash?.Invoke();
        }

        public event Action OnPausePressed;

        private void OnPausePerformed(InputAction.CallbackContext context)
        {
            Debug.Log("Pause Pressed");
            OnPausePressed?.Invoke();
        }
        
        private void OnMovePerformed(InputAction.CallbackContext context)
        {
            MovementInput = context.ReadValue<Vector2>();
        }

        private void OnMoveCanceled(InputAction.CallbackContext context)
        {
            MovementInput = Vector2.zero;
        }
        public event Action OnJumpPressed;
        
        private void OnJumpPerformed(InputAction.CallbackContext context)
        {
            // JumpPressed = true;
            JumpHeld = true;
            OnJumpPressed?.Invoke();
        }

        private void OnJumpCanceled(InputAction.CallbackContext context)
        {
            JumpHeld = false;
        }

        public Action OnCrouch;

        private void OnCrouchPerformed(InputAction.CallbackContext context)
        {
            OnCrouch?.Invoke();
        }
    }
}