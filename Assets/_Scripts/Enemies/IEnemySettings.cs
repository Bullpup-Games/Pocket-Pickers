using System;

namespace _Scripts.Enemies
{
    public interface IEnemySettings
    {
        event Action<float> OnViewModifierChanged;
        bool IsFacingRight();
        void HandleGroundDetection();

        void HandleGravity();

        void FlipLocalScale();

        void changeFov();
    }
}