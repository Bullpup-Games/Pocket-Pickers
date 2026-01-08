using UnityEngine;

namespace Tests
{
    /// <summary>
    /// Mock implementation of ICardOwner for testing card ownership and lifecycle logic
    /// </summary>
    public class MockCardOwner : ICardOwner
    {
        private GameObject _mockGameObject;
        private Transform _transform;

        // Tracking properties for test assertions
        public bool TeleportCalled { get; private set; }
        public Vector2 LastTeleportPosition { get; private set; }
        public Transform LastTeleportTransform { get; private set; }

        public bool ThrowCardCalled { get; private set; }
        public Vector2 LastThrowDirection { get; private set; }

        public ICardManager.CardDestructionTypes? LastDestructionType { get; private set; }
        public bool OnCardDestroyedCalled { get; private set; }

        // ICardOwner implementation properties
        public bool CanThrowCard { get; set; } = true;

        public Transform transform
        {
            get => _transform;
            set => _transform = value;
        }

        /// <summary>
        /// Creates a new MockCardOwner with a temporary GameObject and Transform
        /// </summary>
        public MockCardOwner()
        {
            _mockGameObject = new GameObject("MockCardOwner");
            _transform = _mockGameObject.transform;
            _transform.position = Vector2.zero;
        }

        /// <summary>
        /// Creates a new MockCardOwner with a specific position
        /// </summary>
        public MockCardOwner(Vector2 position) : this()
        {
            _transform.position = position;
        }

        // ICardOwner interface implementation
        public void Teleport(Transform cardTransform, Vector2 safePosition)
        {
            TeleportCalled = true;
            LastTeleportTransform = cardTransform;
            LastTeleportPosition = safePosition;

            // Simulate teleportation by moving transform
            _transform.position = safePosition;
        }

        public void ThrowCard(Vector2 direction)
        {
            ThrowCardCalled = true;
            LastThrowDirection = direction;
        }

        public void OnCardDestroyed(ICardManager.CardDestructionTypes reason)
        {
            OnCardDestroyedCalled = true;
            LastDestructionType = reason;
        }

        /// <summary>
        /// Reset tracking flags for reuse in multiple tests
        /// </summary>
        public void Reset()
        {
            TeleportCalled = false;
            ThrowCardCalled = false;
            OnCardDestroyedCalled = false;
            LastTeleportPosition = Vector2.zero;
            LastTeleportTransform = null;
            LastThrowDirection = Vector2.zero;
            LastDestructionType = null;
            CanThrowCard = true;
        }

        /// <summary>
        /// Cleanup the mock GameObject
        /// </summary>
        public void Cleanup()
        {
            if (_mockGameObject != null)
            {
                Object.DestroyImmediate(_mockGameObject);
            }
        }
    }
}
