using UnityEngine;

namespace Tests
{
    /// <summary>
    /// Mock implementation of ICardEffectHandler for testing without actual particle effects.
    /// This is a plain C# class (not a MonoBehaviour) so it can be instantiated in tests.
    /// Tracks method calls for test assertions without requiring Unity prefabs.
    /// </summary>
    public class MockCardEffectHandler : ICardEffectHandler
    {
        // Tracking properties for test assertions
        public bool TeleportEffectCalled { get; private set; }
        public Vector2 LastTeleportEffectPosition { get; private set; }

        public bool FalseTriggerEffectCalled { get; private set; }
        public Vector2 LastFalseTriggerEffectPosition { get; private set; }

        public bool BounceEffectCalled { get; private set; }
        public Vector2 LastBounceEffectPosition { get; private set; }

        public bool DestroyEffectCalled { get; private set; }
        public Vector2 LastDestroyEffectPosition { get; private set; }

        // ICardEffectHandler implementation
        public void TeleportEffect(Vector2 position)
        {
            TeleportEffectCalled = true;
            LastTeleportEffectPosition = position;
        }

        public void FalseTriggerEffect(Vector2 position)
        {
            FalseTriggerEffectCalled = true;
            LastFalseTriggerEffectPosition = position;
        }

        public void bounceEffect(Vector2 position)
        {
            BounceEffectCalled = true;
            LastBounceEffectPosition = position;
        }

        public void DestroyEffect(Vector2 position)
        {
            DestroyEffectCalled = true;
            LastDestroyEffectPosition = position;
        }

        /// <summary>
        /// Reset tracking flags for reuse in multiple tests
        /// </summary>
        public void Reset()
        {
            TeleportEffectCalled = false;
            FalseTriggerEffectCalled = false;
            BounceEffectCalled = false;
            DestroyEffectCalled = false;

            LastTeleportEffectPosition = Vector2.zero;
            LastFalseTriggerEffectPosition = Vector2.zero;
            LastBounceEffectPosition = Vector2.zero;
            LastDestroyEffectPosition = Vector2.zero;
        }
    }
}
