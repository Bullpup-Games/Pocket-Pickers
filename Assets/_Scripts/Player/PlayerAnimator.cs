using System;
using System.Runtime.CompilerServices;
using _Scripts.Card;
using _Scripts.Player.State;
using UnityEngine;

namespace _Scripts.Player
{
    /// <summary>
    /// VERY primitive animator example.
    /// </summary>
    public class PlayerAnimator : MonoBehaviour
     {
        private SpriteRenderer _spriteRenderer;
        private Animator _animator;
        private static readonly int Speed = Animator.StringToHash("Speed");
        private static readonly int Jumping = Animator.StringToHash("Jumping");
        
        [HideInInspector] public LayerMask environmentLayer;
        
        #region Singleton
        public static PlayerAnimator Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType(typeof(PlayerAnimator)) as PlayerAnimator;

                return _instance;
            }
            set
            {
                _instance = value;
            }
        }
        private static PlayerAnimator _instance;

        #endregion

        private void OnEnable()
        {
            if (InputHandler.Instance == null)
            {
                return;
            }

            setListeners();
            // Subscribe to input events
            // InputHandler.Instance.OnJumpDown += OnJumpDown;
            // PlayerMovement.Instance.GroundedChanged += OnLanded;
            // InputHandler.Instance.OnMove += OnMove;
        }

        private void OnDisable()
        {
            if (InputHandler.Instance == null)
            {
                return;
            }
            deleteListeners();
            // Unsubscribe from input events
            // InputHandler.Instance.OnJumpDown -= OnJumpDown;
            // PlayerMovement.Instance.GroundedChanged -= OnLanded;
            // InputHandler.Instance.OnMove -= OnMove;
        }

        private void Update()
        {
            bool touchingGround = Physics2D.Raycast(gameObject.transform.position, Vector2.down, 1.1f, environmentLayer);
            Debug.DrawRay(gameObject.transform.position, Vector2.down * 1.0f, Color.red);

            if (touchingGround)
            {
                OnLanded();
            }
            else
            {
                OnJumpDown();
            }
           
            
            if (PlayerMovement.Instance.FrameInput == Vector2.zero && touchingGround)
            {
                //be idle
                _animator.SetFloat(Speed, 0);

            }
            else if (PlayerMovement.Instance.FrameInput != Vector2.zero && touchingGround)
            {
                //be moving
                _animator.SetFloat(Speed,Mathf.Abs(PlayerMovement.Instance.FrameInput.x) );

            }


            

        }

        public void setListeners()
        {
           // PlayerMovementController.Instance.Jumped += OnJumpDown;
           CardManager.Instance.cardCreated += doThrowAnimation;
           CardManager.Instance.Teleport += tuck;
        }

        public void deleteListeners()
        {
            //PlayerMovementController.Instance.Jumped -= OnJumpDown;
        }
        private void Awake()
        {
            _spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
            _animator = gameObject.GetComponent<Animator>();
            environmentLayer = LayerMask.GetMask("Environment");
        }

        private void FixedUpdate()
        {
        }
        
        private void OnJumpDown()
        {
           //  Debug.Log("Attempting to jump");
            _animator.SetBool(Jumping, true);
        }

        private void OnLanded()
        {
            // Debug.Log("Landed");
            
            _animator.SetBool(Jumping, false);
        }
        
        private void OnMove(Vector2 input)
        {
            _animator.SetFloat(Speed, Mathf.Abs(input.x));
        }

        private void doThrowAnimation()
        {
            _animator.SetTrigger("Throw");
        }
        
        private void tuck(Vector2 tuck){
            _animator.SetTrigger("teleported");
        }

        public void wallSlide()
        {
            _animator.SetBool("wallSlide", true);
        }

        public void endSlide()
        {
            _animator.SetBool("wallSlide", false);
        }

        public void ledgeHang()
        {
            Debug.Log("Start Ledge Hang");
            _animator.SetBool("ledgeHang", true);
        }

        public void endHang()
        {
            Debug.Log("End Ledge Hang");
            _animator.SetBool("ledgeHang", false);
        }
    }
}
    
   