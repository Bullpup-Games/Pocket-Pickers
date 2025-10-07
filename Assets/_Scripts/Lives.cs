using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using _Scripts.Player;
using Microsoft.Unity.VisualStudio.Editor;
using Unity.VisualScripting;
using UnityEngine;

public class Lives : MonoBehaviour
{
    // Start is called before the first frame update
    public int currentHearts;//how much health you have
    public int availableHearts;//the amount of hearts the player has unlocked
    private int maxHearts = 10;//the total amount of hearts the player can possibly have

    public Life[] hearts;
    public Sprite fullHeart;
    public Sprite damagedHeart;




    
    #region Singleton

    public static Lives Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType(typeof(Lives)) as Lives;

            return _instance;
        }
        set
        {
            _instance = value;
        }
    }
    private static Lives _instance;
    #endregion

    void Start()
    {
        updateHearts();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void updateHearts()
    {
        currentHearts = PlayerVariables.Instance.currentHealth;


        if (currentHearts > availableHearts) currentHearts = availableHearts;

        for (int i = 0; i < maxHearts; i++)
        {
            if (i < currentHearts)
            {
                hearts[i].setActive();
            }
            else if (i < availableHearts)
            {
                hearts[i].setInjured();
            }
            else
            {
                hearts[i].setLocked();
            }
        }
        
    }

}
