using System;
using _Scripts.Enemies.ViewTypes;
using UnityEngine;

namespace _Scripts.Enemies.Skreecher
{
    public class SkreecherSettings : MonoBehaviour, IEnemySettings
    {
        [Header("Detection Settings")] public float baseDetectionTime = 2.5f;
        [Tooltip("The amount of time in seconds that the enemy will stay in detection state even if they are no longer detecting a player")]
        public float waitBeforeTransitionTime = 1.5f;
        [Header("Screech Settings")] public float screechTime = 1.0f;
        private IViewType[] _viewTypes;
        
        public float disabledTimeout = 5f; // Amount of time in seconds the skreecher will spend disabled


        private void Awake()
        {
            _viewTypes = GetComponents<IViewType>();
        }

        private void Update()
        {
            foreach (var view in _viewTypes)
            {
                view.SetView();
            }
        }
        // The skreecher never changes direction, it is always facing forward
        public bool IsFacingRight() => true;
        
        public event Action<float> OnViewModifierChanged;

        public void HandleGroundDetection() {}

        public void HandleGravity() {}

        public void FlipLocalScale() {}

        public void changeFov() {/* TODO: Implement this */}
    }
}