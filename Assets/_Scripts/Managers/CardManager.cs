using UnityEngine;

public class CardManager : MonoBehaviour, ICardManager
{
    // Type of particle effect played during the card destruction
    public enum CardDestructionTypes
    {
        Normal,
        Teleport,
        Cancel,
        FalseTrigger,
        HitEnemy
    }

    public void CreateCard(ICardOwner owner, Vector2 startPos, Vector2 direction)
    {
        throw new System.NotImplementedException();
    }

    public void DestroyCard(ICardOwner cardOwner, ICardManager.CardDestructionTypes particleEffect)
    {
        throw new System.NotImplementedException();
    }
}
