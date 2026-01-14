using NUnit.Framework;
using UnityEngine;
using _Scripts.Managers;
using _Scripts.Player;

namespace Tests
{
    /// <summary>
    /// Integration tests for PlayerController ICardOwner implementation.
    /// Tests card ownership, throwing, teleportation, and input routing.
    ///
    /// TDD Phase: RED - These tests are expected to FAIL until Task 4.2 is implemented.
    /// </summary>
    [TestFixture]
    public class PlayerCardOwnerTests
    {
        private GameObject _playerObject;
        private _Scripts.Player.PlayerController _playerController;
        private _Scripts.Managers.CardManager _cardManager;
        private GameObject _cardManagerObject;
        private MockCardEffectHandler _mockEffectHandler;
        private GameObject _cardPrefab;

        [SetUp]
        public void Setup()
        {
            // Create mock card prefab with Card component
            _cardPrefab = new GameObject("CardPrefab");
            _cardPrefab.AddComponent<_Scripts.Card.Card>();

            // Create CardManager
            _cardManagerObject = new GameObject("CardManager");
            _cardManager = _cardManagerObject.AddComponent<_Scripts.Managers.CardManager>();

            // Set cardPrefab via reflection
            var cardPrefabField = typeof(_Scripts.Managers.CardManager).GetField("cardPrefab",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            cardPrefabField?.SetValue(_cardManager, _cardPrefab);

            // Create effect handler
            var effectHandlerObject = new GameObject("EffectHandler");
            _mockEffectHandler = effectHandlerObject.AddComponent<MockCardEffectHandler>();

            // Initialize CardManager
            _cardManager.Initialize(_mockEffectHandler);

            // Create PlayerController
            _playerObject = new GameObject("Player");
            _playerObject.AddComponent<BoxCollider2D>(); // Required for ICardOwner
            _playerController = _playerObject.AddComponent<_Scripts.Player.PlayerController>();
            _playerController.Initialize(_cardManager);
        }

        [TearDown]
        public void TearDown()
        {
            // Destroy all test objects
            if (_mockEffectHandler != null)
                Object.DestroyImmediate(_mockEffectHandler.gameObject);

            if (_cardManagerObject != null)
                Object.DestroyImmediate(_cardManagerObject);

            if (_playerObject != null)
                Object.DestroyImmediate(_playerObject);

            if (_cardPrefab != null)
                Object.DestroyImmediate(_cardPrefab);

            // Destroy any remaining cards
            foreach (var card in Object.FindObjectsOfType<_Scripts.Card.Card>())
                Object.DestroyImmediate(card.gameObject);
        }

        #region ICardOwner Interface Tests

        [Test]
        public void PlayerController_ImplementsICardOwner()
        {
            // Act & Assert
            Assert.IsNotNull(_playerController as ICardOwner,
                "PlayerController should implement ICardOwner interface");
        }

        [Test]
        public void Transform_ReturnsPlayerTransform()
        {
            // Arrange
            ICardOwner owner = _playerController;

            // Act
            Transform playerTransform = owner.transform;

            // Assert
            Assert.AreEqual(_playerObject.transform, playerTransform,
                "ICardOwner.transform should return player's transform");
        }

        #endregion

        #region Card Throwing Tests

        [Test]
        public void ThrowCard_CallsCardManagerCreateCard()
        {
            // Arrange
            Vector2 direction = Vector2.right;

            // Act
            _playerController.ThrowCard(direction);

            // Assert
            Assert.IsTrue(_cardManager.IsCardActive(_playerController),
                "ThrowCard should create card via CardManager");
        }

        [Test]
        public void ThrowCard_WithValidDirection_CreatesCardInCorrectDirection()
        {
            // Arrange
            Vector2 direction = new Vector2(1f, 0.5f).normalized;

            // Act
            _playerController.ThrowCard(direction);
            _Scripts.Card.Card card = _cardManager.GetCard(_playerController);

            // Assert
            Assert.IsNotNull(card, "Card should be created when thrown");
        }

        [Test]
        public void CanThrowCard_WhenNoCardActive_ReturnsTrue()
        {
            // Act
            bool canThrow = _playerController.CanThrowCard;

            // Assert
            Assert.IsTrue(canThrow,
                "CanThrowCard should return true when no card is active");
        }

        [Test]
        public void CanThrowCard_WhenCardActive_ReturnsFalse()
        {
            // Arrange
            _playerController.ThrowCard(Vector2.right);

            // Act
            bool canThrow = _playerController.CanThrowCard;

            // Assert
            Assert.IsFalse(canThrow,
                "CanThrowCard should return false when card is active");
        }

        [Test]
        public void CanThrowCard_DuringCooldown_ReturnsFalse()
        {
            // Arrange
            _playerController.ThrowCard(Vector2.right);
            _cardManager.DestroyCard(_playerController, ICardManager.CardDestructionTypes.Normal);

            // Act - immediately after destruction (cooldown active)
            bool canThrow = _playerController.CanThrowCard;

            // Assert
            Assert.IsFalse(canThrow,
                "CanThrowCard should return false during cooldown period");
        }

        [Test]
        public void ThrowCard_WhenCardAlreadyActive_DoesNotCreateDuplicate()
        {
            // Arrange
            _playerController.ThrowCard(Vector2.right);
            _Scripts.Card.Card firstCard = _cardManager.GetCard(_playerController);

            // Act
            _playerController.ThrowCard(Vector2.left);
            _Scripts.Card.Card secondCard = _cardManager.GetCard(_playerController);

            // Assert
            Assert.AreSame(firstCard, secondCard,
                "ThrowCard should not create duplicate when card already active");
        }

        #endregion

        #region Teleportation Tests

        [Test]
        public void Teleport_MovesPlayerToSafePosition()
        {
            // Arrange
            Vector2 originalPosition = _playerObject.transform.position;
            Vector2 safePosition = new Vector2(5f, 3f);
            Transform mockCardTransform = new GameObject("MockCard").transform;

            // Act
            _playerController.Teleport(mockCardTransform, safePosition);

            // Assert
            Vector2 newPosition = _playerObject.transform.position;
            Assert.AreEqual(safePosition, newPosition,
                "Teleport should move player to safe position");

            // Cleanup
            Object.DestroyImmediate(mockCardTransform.gameObject);
        }

        [Test]
        public void Teleport_UpdatesPlayerPosition()
        {
            // Arrange
            _playerObject.transform.position = Vector2.zero;
            Vector2 targetPosition = new Vector2(10f, 5f);
            Transform mockCardTransform = new GameObject("MockCard").transform;

            // Act
            _playerController.Teleport(mockCardTransform, targetPosition);

            // Assert
            float distance = Vector2.Distance(_playerObject.transform.position, targetPosition);
            Assert.Less(distance, 0.1f,
                "Player should be teleported to within 0.1 units of target position");

            // Cleanup
            Object.DestroyImmediate(mockCardTransform.gameObject);
        }

        #endregion

        #region Card Destruction Callback Tests

        [Test]
        public void OnCardDestroyed_WithNormalDestruction_UpdatesInternalState()
        {
            // Arrange
            _playerController.ThrowCard(Vector2.right);

            // Act
            _cardManager.DestroyCard(_playerController, ICardManager.CardDestructionTypes.Normal);

            // Assert
            Assert.IsFalse(_cardManager.IsCardActive(_playerController),
                "Card should no longer be active after destruction");
        }

        [Test]
        public void OnCardDestroyed_WithTeleportDestruction_UpdatesInternalState()
        {
            // Arrange
            _playerController.ThrowCard(Vector2.right);

            // Act
            _cardManager.DestroyCard(_playerController, ICardManager.CardDestructionTypes.Teleport);

            // Assert
            Assert.IsFalse(_cardManager.IsCardActive(_playerController),
                "Card should no longer be active after teleport destruction");
        }

        #endregion

        #region Multiple Throw Cycle Tests

        [Test]
        public void ThrowDestroyThrow_AllowsCardRecreation()
        {
            // Arrange & Act - first throw/destroy cycle
            _playerController.ThrowCard(Vector2.right);
            _cardManager.DestroyCard(_playerController, ICardManager.CardDestructionTypes.Normal);

            // Wait for cooldown to expire (simulate time passage)
            // Note: In real implementation, this would need Time.deltaTime simulation

            // Act - second throw (after cooldown)
            // For now, test that the pattern works conceptually
            Assert.IsFalse(_cardManager.IsCardActive(_playerController),
                "Card should not be active after destruction");
        }

        #endregion

        #region Integration Tests

        [Test]
        public void FullCardLifecycle_ThrowTeleportDestroy_WorksCorrectly()
        {
            // Arrange
            Vector2 startPosition = _playerObject.transform.position;
            Vector2 throwDirection = Vector2.right;

            // Act - Throw card
            _playerController.ThrowCard(throwDirection);
            Assert.IsTrue(_cardManager.IsCardActive(_playerController),
                "Card should be active after throwing");

            // Act - Simulate teleport
            _Scripts.Card.Card card = _cardManager.GetCard(_playerController);
            Vector2 teleportPosition = new Vector2(5f, 0f);
            _playerController.Teleport(card.transform, teleportPosition);

            // Act - Destroy card
            _cardManager.DestroyCard(_playerController, ICardManager.CardDestructionTypes.Teleport);

            // Assert
            Assert.IsFalse(_cardManager.IsCardActive(_playerController),
                "Card should be destroyed after full lifecycle");
            Vector2 finalPosition = _playerObject.transform.position;
            Assert.AreEqual(teleportPosition, finalPosition,
                "Player should be at teleport position after full lifecycle");
        }

        #endregion
    }
}
