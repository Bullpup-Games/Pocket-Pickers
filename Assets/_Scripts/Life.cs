using System;
//using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;
using UnityEngine.UI;

public class Life : MonoBehaviour
{


    public Image heartImage;
    public Sprite fullHeart;
    public Sprite emptyHeart;


    public LifeState state;

    public enum LifeState
    {
        Active,
        Injured,
        Locked
    }

    public void setActive()
    {
        if (state == LifeState.Active) return;

        //heartImage.enabled = true;
        //TODO make sure heartImage references the actual gameobject's image
        gameObject.GetComponent<Image>().enabled = true;
        //Animation que here

        state = LifeState.Active;
        heartImage.sprite = fullHeart;
        //game
    }

    public void setInjured()
    {
        if (state == LifeState.Injured) return;

        heartImage.enabled = true;
        //Animation que here
        state = LifeState.Injured;
        heartImage.sprite = emptyHeart;
    }

    public void setLocked()
    {
        if (state == LifeState.Locked) return;

        //Animation que here
        state = LifeState.Locked;
        heartImage.enabled = false;
    }


    void Start()
    {
        heartImage = GetComponent<Image>(); 
    }
}