using UnityEditor;
using UnityEngine;

namespace _Scripts.Camera
{
    public class CameraRoomHandler : MonoBehaviour
    {
        public int roomNumber;
        public Vector3 anchorPoint;

        public LayerMask playerLayer;

        private void OnTriggerStay2D(Collider2D col)
        {
            if ((playerLayer.value & (1 << col.gameObject.layer)) == 0) return;
            if (CameraController.Instance.GetCurrentRoom() == roomNumber) return;
            
            CameraController.Instance.SwitchRooms(anchorPoint, roomNumber);
        }
        
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            // Create a GUIStyle to define the text appearance
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.white; // Set the text color
            style.fontSize = 24; // Set the font size (adjust as needed)
            style.fontStyle = FontStyle.Bold; // Make the text bold (optional)
            style.alignment = TextAnchor.MiddleCenter; // Center the text

            // Get the position of the object's center
            Vector3 position = transform.position;

            // Ensure the label is always visible
            Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;

            // Draw the room number label at the object's position
            Handles.Label(position, "Rm# " + roomNumber.ToString(), style);

            // Reset the zTest to default
            Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
        }
#endif
    }
}