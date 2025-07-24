using UnityEngine;

namespace _Scripts.Camera
{
    public class CameraSizeController : MonoBehaviour
    {
        public float roomWidth = 16f;   // Width of the room in units
        public float roomHeight = 10f;  // Height of the room in units

        private UnityEngine.Camera cam;

        void Start()
        {
            cam = GetComponent<UnityEngine.Camera>();
            AdjustCameraSize();
        }

        void AdjustCameraSize()
        {
            float screenAspect = (float)Screen.width / (float)Screen.height;
            float targetAspect = roomWidth / roomHeight;

            if (screenAspect >= targetAspect)
            {
                // Screen is wider than the room
                cam.orthographicSize = roomHeight / 2;
            }
            else
            {
                // Screen is taller than the room
                float differenceInSize = targetAspect / screenAspect;
                cam.orthographicSize = (roomHeight / 2) * differenceInSize;
            }

            // Handle black bars for wider aspect ratios
            float scaleHeight = screenAspect / targetAspect;

            if (scaleHeight < 1.0f)
            {
                // Add letterbox (black bars on top and bottom)
                Rect rect = cam.rect;

                rect.width = 1.0f;
                rect.height = scaleHeight;
                rect.x = 0;
                rect.y = (1.0f - scaleHeight) / 2.0f;

                cam.rect = rect;
            }
            else
            {
                // Add pillarbox (black bars on sides)
                float scaleWidth = 1.0f / scaleHeight;

                Rect rect = cam.rect;

                rect.width = scaleWidth;
                rect.height = 1.0f;
                rect.x = (1.0f - scaleWidth) / 2.0f;
                rect.y = 0;

                cam.rect = rect;
            }

            cam.orthographicSize += 7.5f;
        }
    }
}
