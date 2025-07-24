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
            currentDirection = directions;
            // Debug.Log(currentDirection);

            if (currentDirection == Vector2.zero)
            {
                // If no input is given remove the arrow indicator
                DestroyDirectionalArrow();
                return;

            }
            
            if (_directionalArrowInstance == null) 
                InstantiateDirectionalArrow();

            // Calculate the angle in radians
            var angleRad = Mathf.Atan2(currentDirection.y, currentDirection.x);

            // Calculate the arrow's position relative to the player
            var offset = new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad) + CardManager.Instance.verticalOffset, 0) * CardManager.Instance.horizontalOffset;
            var arrowPosition = PlayerVariables.Instance.transform.position + offset;
            _directionalArrowInstance.transform.position = arrowPosition;

            // Rotate the arrow to point in the direction of the joystick
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