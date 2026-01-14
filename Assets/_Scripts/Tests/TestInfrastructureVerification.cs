using NUnit.Framework;
using UnityEngine;

namespace Tests
{
    /// <summary>
    /// Verification tests to ensure test infrastructure is set up correctly
    /// These tests validate that the mock classes work as expected
    /// </summary>
    [TestFixture]
    public class TestInfrastructureVerification
    {
        [Test]
        public void MockCardOwner_CanBeCreated()
        {
            // Arrange & Act
            var mockOwner = new MockCardOwner();

            // Assert
            Assert.IsNotNull(mockOwner);
            Assert.IsNotNull(mockOwner.transform);
            Assert.IsTrue(mockOwner.CanThrowCard);

            // Cleanup
            mockOwner.Cleanup();
        }

        [Test]
        public void MockCardOwner_TeleportTracksCorrectly()
        {
            // Arrange
            var mockOwner = new MockCardOwner();
            var testTransform = new GameObject("TestCard").transform;
            var testPosition = new Vector2(5f, 10f);

            // Act
            mockOwner.Teleport(testTransform, testPosition);

            // Assert
            Assert.IsTrue(mockOwner.TeleportCalled);
            Assert.AreEqual(testPosition, mockOwner.LastTeleportPosition);
            Assert.AreEqual(testTransform, mockOwner.LastTeleportTransform);
            Assert.AreEqual(testPosition, (Vector2)mockOwner.transform.position);

            // Cleanup
            Object.DestroyImmediate(testTransform.gameObject);
            mockOwner.Cleanup();
        }

        [Test]
        public void MockCardOwner_ThrowCardTracksCorrectly()
        {
            // Arrange
            var mockOwner = new MockCardOwner();
            var testDirection = new Vector2(1f, 0.5f);

            // Act
            mockOwner.ThrowCard(testDirection);

            // Assert
            Assert.IsTrue(mockOwner.ThrowCardCalled);
            Assert.AreEqual(testDirection, mockOwner.LastThrowDirection);

            // Cleanup
            mockOwner.Cleanup();
        }

        [Test]
        public void MockCardOwner_OnCardDestroyedTracksCorrectly()
        {
            // Arrange
            var mockOwner = new MockCardOwner();
            var destructionType = ICardManager.CardDestructionTypes.Teleport;

            // Act
            mockOwner.OnCardDestroyed(destructionType);

            // Assert
            Assert.IsTrue(mockOwner.OnCardDestroyedCalled);
            Assert.AreEqual(destructionType, mockOwner.LastDestructionType);

            // Cleanup
            mockOwner.Cleanup();
        }

        [Test]
        public void MockCardOwner_ResetClearsAllTracking()
        {
            // Arrange
            var mockOwner = new MockCardOwner();
            mockOwner.ThrowCard(Vector2.right);
            mockOwner.Teleport(new GameObject().transform, Vector2.up);
            mockOwner.OnCardDestroyed(ICardManager.CardDestructionTypes.Cancel);

            // Act
            mockOwner.Reset();

            // Assert
            Assert.IsFalse(mockOwner.TeleportCalled);
            Assert.IsFalse(mockOwner.ThrowCardCalled);
            Assert.IsFalse(mockOwner.OnCardDestroyedCalled);
            Assert.IsNull(mockOwner.LastDestructionType);

            // Cleanup
            mockOwner.Cleanup();
        }

        [Test]
        public void MockCardEffectHandler_CanBeCreated()
        {
            // Arrange & Act (plain C# class, not MonoBehaviour)
            var mockHandler = new MockCardEffectHandler();

            // Assert
            Assert.IsNotNull(mockHandler);
        }

        [Test]
        public void MockCardEffectHandler_TeleportEffectTracksCorrectly()
        {
            // Arrange (plain C# class, not MonoBehaviour)
            var mockHandler = new MockCardEffectHandler();
            var testPosition = new Vector2(3f, 4f);

            // Act
            mockHandler.TeleportEffect(testPosition);

            // Assert
            Assert.IsTrue(mockHandler.TeleportEffectCalled);
            Assert.AreEqual(testPosition, mockHandler.LastTeleportEffectPosition);
        }

        [Test]
        public void MockCardEffectHandler_ResetClearsAllTracking()
        {
            // Arrange (plain C# class, not MonoBehaviour)
            var mockHandler = new MockCardEffectHandler();
            mockHandler.TeleportEffect(Vector2.one);
            mockHandler.FalseTriggerEffect(Vector2.one);
            mockHandler.bounceEffect(Vector2.one);
            mockHandler.DestroyEffect(Vector2.one);

            // Act
            mockHandler.Reset();

            // Assert
            Assert.IsFalse(mockHandler.TeleportEffectCalled);
            Assert.IsFalse(mockHandler.FalseTriggerEffectCalled);
            Assert.IsFalse(mockHandler.BounceEffectCalled);
            Assert.IsFalse(mockHandler.DestroyEffectCalled);
        }
    }
}
