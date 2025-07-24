using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardEffectHandler : MonoBehaviour
{

    public GameObject pinkPoof;
    public GameObject bluePoof;
    
    public GameObject cardBounce;
    public GameObject cardDestroy;
    
    
    #region Singleton

    public static CardEffectHandler Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType(typeof(CardEffectHandler)) as CardEffectHandler;

            return _instance;
        }
        set => _instance = value;
    }

    private static CardEffectHandler _instance;

    #endregion
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TeleportEffect(Vector2 position)
    {
        Instantiate(pinkPoof, position, Quaternion.identity);
    }

    public void FalseTriggerEffect(Vector2 position)
    {
        Instantiate(bluePoof, position, Quaternion.identity);
    }
    
    public void bounceEffect(Vector2 position) 
    {
        Instantiate(cardBounce, position, Quaternion.identity);
    }

    public void DestroyEffect(Vector2 position)
    {
        Instantiate(cardDestroy, position, Quaternion.identity);
    }
}
