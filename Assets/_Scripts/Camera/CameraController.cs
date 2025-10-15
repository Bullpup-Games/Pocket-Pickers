using System.Collections;
using UnityEngine;

namespace _Scripts.Camera
{
    public enum CameraState
    {
        Idle,
        Transitioning
    }

    [RequireComponent(typeof(UnityEngine.Camera))]
    public class CameraController : MonoBehaviour
    {
        [Header("Movement")]
        public float lerpSpeed;
        public AnimationCurve movementCurve;

        [Header("Screen Shake")]
        public bool enableShake = false;
        [Range(-1f, 1f)]
        public float shakeIntensity = 0.1f;
        public float shakeFrequency = 20f;

        private UnityEngine.Camera _cam;

        private int _currentRoom;

        private CameraState _currentState = CameraState.Idle;
        private Vector3 _transitionStartPosition;
        private Vector3 _transitionTargetPosition;
        private float _transitionProgress;
        private float _transitionDuration;
        private Coroutine _activeTransition;

        private bool _isShaking;
        private Vector3 _shakeOffset;
        private Coroutine _shakeCoroutine;
        private Vector3 _basePosition;

        public int GetCurrentRoom() => _currentRoom;
        public CameraState GetCurrentState() => _currentState;
        
        #region Singleton

        public static CameraController Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType(typeof(CameraController)) as CameraController;

                return _instance;
            }
            set { _instance = value; }
        }

        private static CameraController _instance;

        #endregion

        private void Awake()
        {
            _cam = GetComponent<UnityEngine.Camera>();
            _currentRoom = 0;
        }

        private void Update()
        {
            HandleShakeToggle();
        }

        private void HandleShakeToggle()
        {
            if (enableShake && !_isShaking)
            {
                StartShake();
                return;
            }

            if (!enableShake && _isShaking)
            {
                StopShake();
            }
        }

        public void SwitchRooms(Vector3 anchorPoint, int roomNumber)
        {
            if (roomNumber == _currentRoom) return;

            _currentRoom = roomNumber;

            switch (_currentState)
            {
                case CameraState.Idle:
                    StartTransition(anchorPoint);
                    break;
                case CameraState.Transitioning:
                    UpdateTransitionTarget(anchorPoint);
                    break;
            }
        }

        private void StartTransition(Vector3 targetPosition)
        {
            if (_activeTransition != null)
            {
                StopCoroutine(_activeTransition);
            }

            _currentState = CameraState.Transitioning;
            _transitionStartPosition = _cam.transform.position;
            _transitionTargetPosition = targetPosition;
            _transitionProgress = 0f;
            _transitionDuration = 1f / lerpSpeed;

            _activeTransition = StartCoroutine(SmoothTransition());
        }

        private void UpdateTransitionTarget(Vector3 newTargetPosition)
        {
            _transitionStartPosition = _cam.transform.position - _shakeOffset;
            _transitionTargetPosition = newTargetPosition;
            _transitionProgress = 0f;
            _transitionDuration = 1f / lerpSpeed;
        }

        private void StartShake()
        {
            if (_shakeCoroutine != null)
            {
                StopCoroutine(_shakeCoroutine);
            }

            _isShaking = true;
            _basePosition = _cam.transform.position;
            _shakeCoroutine = StartCoroutine(ShakeCoroutine());
        }

        private void StopShake()
        {
            if (_shakeCoroutine != null)
            {
                StopCoroutine(_shakeCoroutine);
                _shakeCoroutine = null;
            }

            _isShaking = false;
            _shakeOffset = Vector3.zero;
            _cam.transform.position = _basePosition;
        }

        // Returns the current bounds of the camera
        public Bounds OrthographicBounds()
        {
            var screenAspect = (float)Screen.width / Screen.height;
            var cameraHeight = _cam.orthographicSize * 2;
            var bounds = new Bounds(
                _cam.transform.position,
                new Vector3(cameraHeight * screenAspect, cameraHeight, 0));
            return bounds;
        }

        private IEnumerator SmoothTransition()
        {
            while (_currentState == CameraState.Transitioning)
            {
                _transitionProgress += Time.deltaTime;
                var normalizedTime = Mathf.Clamp01(_transitionProgress / _transitionDuration);
                var curveValue = movementCurve.Evaluate(normalizedTime);

                _basePosition = Vector3.Lerp(_transitionStartPosition, _transitionTargetPosition, curveValue);
                _cam.transform.position = _basePosition + _shakeOffset;

                if (normalizedTime >= 1f)
                {
                    _basePosition = _transitionTargetPosition;
                    _cam.transform.position = _basePosition + _shakeOffset;
                    _currentState = CameraState.Idle;
                    _activeTransition = null;
                    yield break;
                }

                yield return null;
            }
        }

        private IEnumerator ShakeCoroutine()
        {
            while (_isShaking)
            {
                _shakeOffset = new Vector3(
                    Random.Range(-shakeIntensity, shakeIntensity),
                    Random.Range(-shakeIntensity, shakeIntensity),
                    0f
                );

                _cam.transform.position = _basePosition + _shakeOffset;

                yield return new WaitForSeconds(1f / shakeFrequency);
            }
        }
    }
}