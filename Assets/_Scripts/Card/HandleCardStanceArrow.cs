using System;
using _Scripts.Player;
using UnityEngine;

namespace _Scripts.Card
{
    /**
     * HandleCardStanceArrow is responsible for displaying and updating the directional indicator
     * whenever the player is in 'Card Stance'
     */
    public class HandleCardStanceArrow : MonoBehaviour
    {
        // public Transform player;
        public GameObject directionalArrowPrefab;  // Arrow prefab to instantiate
        private GameObject _directionalArrowInstance;  // The instantiated arrow in the scene
        public GameObject DirectionalArrowInstance() => _directionalArrowInstance;
    
        public Vector2 currentDirection;  // Stores the current direction of the arrow

        #region Singleton
        public static HandleCardStanceArrow Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType(typeof(HandleCardStanceArrow)) as HandleCardStanceArrow;

                return _instance;
            }
            set
            {
                _instance = value;
            }
        }
        private static HandleCardStanceArrow _instance;
        #endregion

        public void InstantiateDirectionalArrow()
        {
            // Instantiate the arrow as a child of the player
            _directionalArrowInstance = Instantiate(
                directionalArrowPrefab,
                PlayerVariables.Instance.transform.position,
                Quaternion.identity,
                PlayerVariables.Instance.transform
            );
        }

        private void Update()
        {
            if (CardManager.Instance.IsCardInScene() && _directionalArrowInstance != null)
                DestroyDirectionalArrow();
        }

        private void OnEnable()
        {
            InputHandler.Instance.CardStanceDirectionalInput += UpdateArrow;
        }

        private void UpdateArrow(Vector2 directions)
        {
            // For arrow display, use the provided directions (might be zero to hide arrow)
            Vector2 arrowDirection = directions;
            
            // For card throwing direction:
            // - Controller: use the stick input (directions parameter)
            // - Mouse: use actual mouse direction even when arrow is hidden
            Vector2 mouseActualDirection = InputHandler.Instance.GetActualAimDirection();
            
            if (mouseActualDirection.magnitude > 0.1f)
            {
                // Mouse is being used - use actual mouse direction for throwing
                currentDirection = mouseActualDirection;
            }
            else
            {
                // Controller is being used - use stick input for throwing
                currentDirection = arrowDirection;
            }
            
            // Debug.Log($"Arrow: {arrowDirection.magnitude:F2}, Mouse: {mouseActualDirection.magnitude:F2}, Current: {currentDirection.magnitude:F2}");

            if (arrowDirection == Vector2.zero)
            {
                // Hide the arrow indicator
                DestroyDirectionalArrow();
                return;
            }
            
            if (_directionalArrowInstance == null) 
                InstantiateDirectionalArrow();

            var angleRad = Mathf.Atan2(currentDirection.y, currentDirection.x);

            // Calculate the arrow's position relative to the player
            var offset = new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad) + CardManager.Instance.verticalOffset, 0) * CardManager.Instance.horizontalOffset;
            var arrowPosition = PlayerVariables.Instance.transform.position + offset;
            _directionalArrowInstance.transform.position = arrowPosition;

            // Rotate the arrow to point in the direction of the joystick / mouse
            var angleDeg = angleRad * Mathf.Rad2Deg;
            _directionalArrowInstance.transform.rotation = Quaternion.Euler(0, 0, angleDeg - 90f);
        }

        public void DestroyDirectionalArrow()
        {
            if (_directionalArrowInstance == null) return;
            Destroy(_directionalArrowInstance);
            currentDirection = new Vector2();
            _directionalArrowInstance = null;
        }
    }
}