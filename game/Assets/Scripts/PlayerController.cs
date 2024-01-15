using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Start is called before the first frame update
    private Rigidbody2D rb;
    [SerializeField] private float walkSpeed = 10;
    private float xAxis,yAxis;
    [SerializeField] private float jumpForce=30;
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckY = 0.2f;
    [SerializeField] private float groundCheckX = 0.5f;
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] public PlayerStateList pState;
    Animator anim;
    public static PlayerController Instance;

    private int jumpCounter;
    [SerializeField] private int maxJumpCounter;
    private int jumpBufferCounter;
    [SerializeField] private int jumpBufferFrames;

    [SerializeField]  private bool canDash=true;
    [SerializeField] private float dashTime, dashCoolDown, dashSpeed;
    float gravity;
    private bool dashed = false;
    [SerializeField] private GameObject dashEffect;

    [Header("Attack settings")]
    [SerializeField] bool attack = false;
    [SerializeField] private float timeBetweenAttack, timeSinceAttack;
    [SerializeField] private Transform sideAttackTransform, upAttackTransform;
    [SerializeField] private Vector2 sideAttackArea, upAttackArea;
    private LayerMask attackableLayer;
    private void Awake()
    {
        if(Instance !=null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }
    void Start()
    { 
        jumpCounter = 0;
        pState = GetComponent<PlayerStateList>();
        rb = GetComponent<Rigidbody2D>();       
        anim = GetComponent<Animator>();
        gravity = rb.gravityScale;
    }

    // Update is called once per frame
    void Update()
    {
        getInputs();
        UpdateJumpVariables();
        if (pState.dashing) return;
        Flip();
        Move();
        jump();
        startDash();
        Attack();
    }
    void Flip()
    {   
        if(xAxis < 0)
        {
            transform.localScale = new Vector3((float)0.5, transform.localScale.y, transform.localScale.z);
        }
        else
        {
            transform.localScale = new Vector3((float)-0.5, transform.localScale.y, transform.localScale.z);
        }
    }
    void getInputs()
    {
        xAxis = Input.GetAxisRaw("Horizontal");
        yAxis = Input.GetAxisRaw("Vertical");
        attack = Input.GetButtonDown("Attack");
    }
    private void Move()
    {
        rb.velocity = new Vector2 (xAxis * walkSpeed, rb.velocity.y);
        anim.SetBool("Walking", rb.velocity.x!=0 && onGround());
    }
    private bool onGround()
    {
        if (Physics2D.Raycast(groundCheckPoint.position, Vector2.down, groundCheckY, whatIsGround)){
            jumpCounter = 0;
            return true;
        }
        else if(Physics2D.Raycast(groundCheckPoint.position + new Vector3(groundCheckX,0,0), Vector2.down, groundCheckY, whatIsGround)
            && Physics2D.Raycast(groundCheckPoint.position - new Vector3(groundCheckX, 0, 0), Vector2.down, groundCheckY, whatIsGround))
        {
            jumpCounter = 0;
            return true;
        }
        return false;

    }
    void jump()
    {
        if (Input.GetButtonDown("Jump") && rb.velocity.y >0)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0);
            pState.jumping = false;
        }
        if(!pState.jumping)
        {
            if(jumpBufferCounter > 0 && onGround())
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                pState.jumping = true;
            }
            else if(!onGround() && jumpCounter < maxJumpCounter && Input.GetButtonDown("Jump")) {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                pState.jumping = true;
                jumpCounter++;
            }
        }
    }
    void UpdateJumpVariables()
    {
        if (onGround())
        {
            pState.jumping = false;
        }
        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferCounter = jumpBufferFrames;
        }
        else
        {
            jumpBufferCounter--;
        }
    }
    void startDash()
    {
        if(Input.GetButtonDown("Dash") && canDash && !dashed)
        {
            StartCoroutine(Dash());
            dashed = true;
        }
        if (onGround())
        {
            dashed = false;
        }
    }
    IEnumerator Dash()
    {
        canDash = false;
        pState.dashing = true;
        anim.SetBool("Dashing", true);
        rb.gravityScale = 0;
        rb.velocity =new Vector2((rb.velocity.x+1) * dashSpeed, 0);
        Instantiate(dashEffect, transform);
        yield return new WaitForSeconds(dashTime);
        rb.gravityScale = gravity;
        pState.dashing = false;
        yield return new WaitForSeconds(dashCoolDown);
        canDash=true;
    }
    void Attack()
    {
        timeSinceAttack += Time.deltaTime;
        if(attack && timeSinceAttack >=timeBetweenAttack)
        {
            timeSinceAttack = 0;
        }
        if(yAxis ==0 || yAxis < 0 && onGround())
        {
            Hit(sideAttackTransform,sideAttackArea);
        }
        else if(yAxis > 0)
        {
            Hit(upAttackTransform,upAttackArea);
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(sideAttackTransform.position, sideAttackArea);
        Gizmos.DrawWireCube(upAttackTransform.position, upAttackArea);
    }
    private void Hit(Transform attackTransform,Vector2 attackArea)
    {
        Collider2D[] objects = Physics2D.OverlapBoxAll(attackTransform.position, attackArea,0, attackableLayer);
        if(objects.Length > 0)
        {
            print("Hit");
        }
    }
}
