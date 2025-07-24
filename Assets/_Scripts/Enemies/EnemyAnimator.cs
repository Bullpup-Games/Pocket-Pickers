using System.Collections;
using System.Collections.Generic;
using _Scripts.Enemies;
using UnityEngine;

public class EnemyAnimator : MonoBehaviour
{
    // Start is called before the first frame update
    private Animator _animator;
    private bool isFalling;
    private IEnemySettings _enemySettings;
    
    //used to determine the enemy's current speed
    private Vector3 lastPosition;
    private Vector3 currentPosition;
    public float speed;
    
    private readonly int speedHash = Animator.StringToHash("Speed");
    private readonly int disabledHash = Animator.StringToHash("Disabled");
    

    [HideInInspector] public bool disabled;
    
    [HideInInspector] public LayerMask environmentLayer;
    void Start()
    {
        _animator = gameObject.transform.Find("Sprite").GetComponent<Animator>();
        isFalling = false;
        _enemySettings = gameObject.GetComponent<IEnemySettings>();
        environmentLayer = LayerMask.GetMask("Environment");

        speed = 0;
        lastPosition = gameObject.transform.position;
        currentPosition = gameObject.transform.position;
        _animator.SetFloat(speedHash, speed);
    }

    // Update is called once per frame
    void Update()
    {
        if (disabled) return;//we dont want to change the animations on a dead enemy
        
        bool touchingGround = Physics2D.Raycast(gameObject.transform.position, Vector2.down, 1.1f, environmentLayer);
        Debug.DrawRay(gameObject.transform.position, Vector2.down * 1.1f, Color.red);

        
        //doing checks to make sure we don't set a bool that has already been set
        if (!touchingGround)
        {
            falling();
        } else 
        {
            grounded();
        }
        
        //calculate the speed the patroller is moving
        //should be (absolute value of( the distance between the current position and the last position) * Time.deltatime)
        
        currentPosition = gameObject.transform.position;
        speed = Mathf.Abs(currentPosition.x - lastPosition.x) / Time.deltaTime; //we only care about horizontal speed
        lastPosition = currentPosition;
        
        //set the speed in the animator
        _animator.SetFloat(speedHash, speed);
    }

    private void falling()
    {
        isFalling = true;
        _animator.SetBool("Falling", true);
    }

    private void grounded()
    {
        isFalling = false;
        _animator.SetBool("Falling", false);
    }

    //these next two are called from the state change itself
    public void chase()
    {
        _animator.SetBool("Aggressive", true);
    }

    public void stopChase()
    {
        _animator.SetBool("Aggressive", false);
    }

    public void disable()
    {
        _animator.SetBool(disabledHash, true);
        disabled = true;
    }

    public void endDisable()
    {
     _animator.SetBool(disabledHash, false);   
    }
}
