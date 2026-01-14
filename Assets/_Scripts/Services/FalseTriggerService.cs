using UnityEngine;

namespace _Scripts.Services
{
    /// <summary>
    /// Manages false trigger global state for enemy investigation behavior.
    /// False trigger is a gameplay mechanic where the card can create a distraction point
    /// that enemies will investigate.
    /// </summary>
    public static class FalseTriggerService
    {
        /// <summary>
        /// The last position where a false trigger was activated.
        /// Enemies use this to know where to investigate.
        /// </summary>
        public static Vector2 LastFalseTriggerPosition { get; set; }

        /// <summary>
        /// Resets the false trigger state (useful for scene transitions, testing, etc.)
        /// </summary>
        public static void Reset()
        {
            LastFalseTriggerPosition = Vector2.zero;
        }
    }
}
