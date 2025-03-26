using UnityEngine;

namespace Assets._Scripts.Input
{
    /// <summary>
    /// FrameInput structs are used to package information about the
    /// current frames input information.
    /// This allows for more precise tracking and eliminates 'ghost inputs'.
    /// </summary>
    public struct FrameInput
    {
        public bool JumpDown;
        public bool JumpHeld;
        public Vector2 Move;

        // TODO: Maybe add input frame data for dashing, card throws, etc
    }

    // Concept could be extended to an additional struct 'FrameInteractions'
    // This way we could keep track of exactly when things like puzzle interactions or enemy hits occur
}