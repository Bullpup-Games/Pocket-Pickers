using NUnit.Framework;
using UnityEngine;
using _Scripts.Card;

namespace Tests
{
    /// <summary>
    /// Unit tests for CardManager ownership tracking functionality.
    /// Tests the multi-owner card management system with owner-based lifecycle.
    ///
    /// TDD Phase: RED - These tests are expected to FAIL until Task 2.2 is implemented.
    /// </summary>
    [TestFixture]
    public class CardManagerOwnershipTests
    {
        private CardManager _cardManager;
        private GameObject _cardManagerObject;
        private MockCardOwner _mockOwner1;
        private MockCardOwner _mockOwner2;
        private MockCardEffectHandler _mockEffectHandler;
        private GameObject _cardPrefab;

        [SetUp]
        public void Setup()
        {
            // Create mock card prefab with Card component
            _cardPrefab = new GameObject("CardPrefab");
            _cardPrefab.AddComponent<Card>();

            // Create CardManager GameObject
            _cardManagerObject = new GameObject("CardManager");
            _cardManager = _cardManagerObject.AddComponent<CardManager>();

            // Use reflection to set the cardPrefab field (it's serialized but we can't use inspector in tests)
            var cardPrefabField = typeof(CardManager).GetField("cardPrefab",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            cardPrefabField?.SetValue(_cardManager, _cardPrefab);

            // Create mock dependencies
            var effectHandlerObject = new GameObject("EffectHandler");
            _mockEffectHandler = effectHandlerObject.AddComponent<MockCardEffectHandler>();
            _mockOwner1 = new MockCardOwner();
            _mockOwner2 = new MockCardOwner();

            // Initialize CardManager with effect handler
            _cardManager.Initialize(_mockEffectHandler);
        }

        [TearDown]
        public void TearDown()
        {
            // Cleanup mocks
            _mockOwner1?.Cleanup();
            _mockOwner2?.Cleanup();

            // Destroy all created GameObjects
            if (_mockEffectHandler != null)
            {
                Object.DestroyImmediate(_mockEffectHandler.gameObject);
            }

            if (_cardManagerObject != null)
            {
                Object.DestroyImmediate(_cardManagerObject);
            }

            if (_cardPrefab != null)
            {
                Object.DestroyImmediate(_cardPrefab);
            }

            // Destroy any remaining card instances created during tests
            foreach (var card in Object.FindObjectsOfType<Card>())
            {
                Object.DestroyImmediate(card.gameObject);
            }
        }

        #region Card Creation Tests

        [Test]
        public void CreateCard_WithValidOwner_CreatesCardForOwner()
        {
            // Arrange
            Vector2 startPos = new Vector2(1f, 2f);
            Vector2 direction = Vector2.right;

            // Act
            _cardManager.CreateCard(_mockOwner1, startPos, direction);

            // Assert
            Assert.IsTrue(_cardManager.IsCardActive(_mockOwner1),
                "Card should be active for owner after creation");
        }

        [Test]
        public void CreateCard_WithValidOwner_ReturnsCardInstance()
        {
            // Arrange
            Vector2 startPos = new Vector2(0f, 0f);
            Vector2 direction = Vector2.up;

            // Act
            _cardManager.CreateCard(_mockOwner1, startPos, direction);
            Card card = _cardManager.GetCard(_mockOwner1);

            // Assert
            Assert.IsNotNull(card, "GetCard should return card instance after creation");
        }

        [Test]
        public void CreateCard_WithNullOwner_DoesNotCreateCard()
        {
            // Arrange
            Vector2 startPos = Vector2.zero;
            Vector2 direction = Vector2.right;

            // Act
            _cardManager.CreateCard(null, startPos, direction);

            // Assert
            Assert.IsFalse(_cardManager.IsCardActive(null),
                "Card should not be created for null owner");
        }

        [Test]
        public void CreateCard_WithNullOwner_LogsError()
        {
            // Arrange
            Vector2 startPos = Vector2.zero;
            Vector2 direction = Vector2.right;

            // Act - should log error but not crash
            LogAssert.Expect(LogType.Error, System.Text.RegularExpressions.Regex.Escape("CardManager.CreateCard: owner cannot be null"));
            _cardManager.CreateCard(null, startPos, direction);

            // Assert - test passes if expected log message appears
        }

        [Test]
        public void CreateCard_WhenOwnerAlreadyHasCard_DoesNotCreateDuplicate()
        {
            // Arrange
            _cardManager.CreateCard(_mockOwner1, Vector2.zero, Vector2.right);
            Card firstCard = _cardManager.GetCard(_mockOwner1);

            // Act - attempt to create second card for same owner
            _cardManager.CreateCard(_mockOwner1, Vector2.one, Vector2.left);
            Card secondCard = _cardManager.GetCard(_mockOwner1);

            // Assert
            Assert.AreSame(firstCard, secondCard,
                "Creating card for owner with existing card should not create duplicate");
        }

        [Test]
        public void CreateCard_WhenOwnerAlreadyHasCard_LogsWarning()
        {
            // Arrange
            _cardManager.CreateCard(_mockOwner1, Vector2.zero, Vector2.right);

            // Act & Assert
            LogAssert.Expect(LogType.Warning, System.Text.RegularExpressions.Regex.Escape($"Owner {_mockOwner1} already has active card"));
            _cardManager.CreateCard(_mockOwner1, Vector2.one, Vector2.left);
        }

        #endregion

        #region Multi-Owner Tests

        [Test]
        public void CreateCard_ForMultipleOwners_TracksEachCardSeparately()
        {
            // Arrange
            Vector2 pos1 = new Vector2(1f, 1f);
            Vector2 pos2 = new Vector2(2f, 2f);

            // Act
            _cardManager.CreateCard(_mockOwner1, pos1, Vector2.right);
            _cardManager.CreateCard(_mockOwner2, pos2, Vector2.up);

            // Assert
            Assert.IsTrue(_cardManager.IsCardActive(_mockOwner1),
                "First owner should have active card");
            Assert.IsTrue(_cardManager.IsCardActive(_mockOwner2),
                "Second owner should have active card");
            Assert.AreNotSame(_cardManager.GetCard(_mockOwner1), _cardManager.GetCard(_mockOwner2),
                "Each owner should have distinct card instance");
        }

        [Test]
        public void GetCard_ForOwnerWithoutCard_ReturnsNull()
        {
            // Arrange - owner with no card

            // Act
            Card card = _cardManager.GetCard(_mockOwner1);

            // Assert
            Assert.IsNull(card, "GetCard should return null for owner without active card");
        }

        [Test]
        public void IsCardActive_ForOwnerWithoutCard_ReturnsFalse()
        {
            // Arrange - owner with no card

            // Act
            bool isActive = _cardManager.IsCardActive(_mockOwner1);

            // Assert
            Assert.IsFalse(isActive, "IsCardActive should return false for owner without card");
        }

        #endregion

        #region Card Destruction Tests

        [Test]
        public void DestroyCard_WithValidOwner_RemovesCardFromTracking()
        {
            // Arrange
            _cardManager.CreateCard(_mockOwner1, Vector2.zero, Vector2.right);
            Assert.IsTrue(_cardManager.IsCardActive(_mockOwner1), "Precondition: card should be active");

            // Act
            _cardManager.DestroyCard(_mockOwner1, ICardManager.CardDestructionTypes.Normal);

            // Assert
            Assert.IsFalse(_cardManager.IsCardActive(_mockOwner1),
                "Card should no longer be active after destruction");
            Assert.IsNull(_cardManager.GetCard(_mockOwner1),
                "GetCard should return null after card destruction");
        }

        [Test]
        public void DestroyCard_WithValidOwner_NotifiesOwner()
        {
            // Arrange
            _cardManager.CreateCard(_mockOwner1, Vector2.zero, Vector2.right);
            _mockOwner1.Reset(); // Clear creation tracking

            // Act
            _cardManager.DestroyCard(_mockOwner1, ICardManager.CardDestructionTypes.Teleport);

            // Assert
            Assert.IsTrue(_mockOwner1.OnCardDestroyedCalled,
                "Owner should be notified when card is destroyed");
        }

        [Test]
        public void DestroyCard_ForOwnerWithoutCard_DoesNotCrash()
        {
            // Arrange - owner with no card

            // Act & Assert - should not throw exception
            Assert.DoesNotThrow(() =>
            {
                _cardManager.DestroyCard(_mockOwner1, ICardManager.CardDestructionTypes.Cancel);
            }, "Destroying card for owner without card should not crash");
        }

        [Test]
        public void DestroyCard_ForOwnerWithoutCard_DoesNotNotifyOwner()
        {
            // Arrange - owner with no card
            _mockOwner1.Reset();

            // Act
            _cardManager.DestroyCard(_mockOwner1, ICardManager.CardDestructionTypes.Normal);

            // Assert
            Assert.IsFalse(_mockOwner1.OnCardDestroyedCalled,
                "Owner should not be notified when destroying non-existent card");
        }

        [Test]
        public void DestroyCard_WithNullOwner_DoesNotCrash()
        {
            // Act & Assert - should not throw exception
            Assert.DoesNotThrow(() =>
            {
                _cardManager.DestroyCard(null, ICardManager.CardDestructionTypes.Normal);
            }, "Destroying card for null owner should not crash");
        }

        #endregion

        #region Multiple Create/Destroy Cycle Tests

        [Test]
        public void CreateDestroyCreate_ForSameOwner_WorksCorrectly()
        {
            // Arrange & Act - first create/destroy cycle
            _cardManager.CreateCard(_mockOwner1, Vector2.zero, Vector2.right);
            Card firstCard = _cardManager.GetCard(_mockOwner1);
            _cardManager.DestroyCard(_mockOwner1, ICardManager.CardDestructionTypes.Normal);

            // Act - second create cycle
            _cardManager.CreateCard(_mockOwner1, Vector2.one, Vector2.up);
            Card secondCard = _cardManager.GetCard(_mockOwner1);

            // Assert
            Assert.IsNotNull(secondCard, "Should be able to create card again after destruction");
            Assert.AreNotSame(firstCard, secondCard, "Second card should be new instance");
            Assert.IsTrue(_cardManager.IsCardActive(_mockOwner1),
                "Card should be active after recreation");
        }

        [Test]
        public void RapidCreateDestroyCycles_DoesNotLeakMemory()
        {
            // Arrange & Act - rapid create/destroy cycles
            for (int i = 0; i < 10; i++)
            {
                _cardManager.CreateCard(_mockOwner1, Vector2.zero, Vector2.right);
                Assert.IsTrue(_cardManager.IsCardActive(_mockOwner1),
                    $"Card should be active in iteration {i}");

                _cardManager.DestroyCard(_mockOwner1, ICardManager.CardDestructionTypes.Normal);
                Assert.IsFalse(_cardManager.IsCardActive(_mockOwner1),
                    $"Card should be inactive after destruction in iteration {i}");
            }

            // Assert - final state should be clean
            Assert.IsFalse(_cardManager.IsCardActive(_mockOwner1),
                "No card should be active after all cycles complete");
            Assert.IsNull(_cardManager.GetCard(_mockOwner1),
                "GetCard should return null after all cycles complete");
        }

        [Test]
        public void DestroyCard_WithMultipleOwners_OnlyDestroysSpecifiedOwnerCard()
        {
            // Arrange
            _cardManager.CreateCard(_mockOwner1, Vector2.zero, Vector2.right);
            _cardManager.CreateCard(_mockOwner2, Vector2.one, Vector2.up);

            // Act - destroy only owner1's card
            _cardManager.DestroyCard(_mockOwner1, ICardManager.CardDestructionTypes.Normal);

            // Assert
            Assert.IsFalse(_cardManager.IsCardActive(_mockOwner1),
                "Owner1 should not have active card after destruction");
            Assert.IsTrue(_cardManager.IsCardActive(_mockOwner2),
                "Owner2 should still have active card after owner1 destruction");
        }

        #endregion

        #region Initialization Tests

        [Test]
        public void Initialize_WithValidEffectHandler_DoesNotThrow()
        {
            // Arrange
            var newManager = new GameObject("NewManager").AddComponent<CardManager>();
            var effectHandler = new MockCardEffectHandler();

            // Act & Assert
            Assert.DoesNotThrow(() =>
            {
                newManager.Initialize(effectHandler);
            }, "Initialize should not throw with valid effect handler");

            // Cleanup
            Object.DestroyImmediate(newManager.gameObject);
        }

        [Test]
        public void Initialize_WithNullEffectHandler_ThrowsException()
        {
            // Arrange
            var newManager = new GameObject("NewManager").AddComponent<CardManager>();

            // Act & Assert
            Assert.Throws<System.ArgumentNullException>(() =>
            {
                newManager.Initialize(null);
            }, "Initialize should throw ArgumentNullException for null effect handler");

            // Cleanup
            Object.DestroyImmediate(newManager.gameObject);
        }

        #endregion
    }
}
