using System.Collections;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEditor.PackageManager;
using UnityEngine;

public class LifeIcon : MonoBehaviour
{
    public Sprite activeSprite;
    public Sprite injuredSprite;

    [HideInInspector]
    public lifeState state;



    public void becomeActive()
    {
        SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        if (state == lifeState.Active)
        {
            return;
        }

        //we are unlocking a life
        if (state == lifeState.Locked)
        {
            //todo set an animation to unlock a life
            Color color = spriteRenderer.color;
            color.a = 1.0f;
            spriteRenderer.color = color;

        }

        //we are healing a life
        if (state == lifeState.Injured)
        {
            //todo set an animation to heal a life
        }

        state = lifeState.Active;
        spriteRenderer.sprite = activeSprite;
    }

    public void becomeInjured()
    {
        SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();

        if (state == lifeState.Injured)
        {
            return;
        }

        if (state == lifeState.Locked)
        {
            return; //TODO maybe throw an error here
        }

        //TODO impliment animation
        state = lifeState.Injured;
        spriteRenderer.sprite = injuredSprite;
    }

    public void lockLife()
    {
        if (state == lifeState.Locked)
        {
            return;
        }

        state = lifeState.Locked;
        SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();

        //TODO impliment animation for locking life
        Color color = spriteRenderer.color;
        color.a = 0f;
        spriteRenderer.color = color;


    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}

public enum lifeState
{
    Active,
    Injured,
    Locked
}
