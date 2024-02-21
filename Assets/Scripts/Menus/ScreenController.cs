using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenController : MonoBehaviour
{
    [Header("Main params")]
    [SerializeField] private float playerMovementSpeed;
    [SerializeField] private float playerJumpSpeed;
    [SerializeField] private float gravityOnWall;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private Rigidbody2D playerBody;
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private BoxCollider2D playerBoxCollider;

    private float wallJumpCoolDown;
    private float horizontalInput;
    private bool running;
    private bool moveLeft;  //determinate if we move left or right
    private bool dontMove; //determinate if we are moving

    [Header("Sound params")]
    public AudioSource audioSource;
    public AudioClip jumpClip;

    private void Start()
    {
        running = true;
        dontMove = true;
    }

    private void Update()
    {
        HandleMoving();
        horizontalInput = Input.GetAxis("Horizontal");
    }

    private void HandleMoving()
    {
        if (dontMove)
        {
            StopMoving();
        }
        else
        {
            if (moveLeft)
            {
                MoveLeft();
            }else if (!moveLeft)
            {
                MoveRight();
            }
        }
    }

    public void AllowMovement(bool movement)
    {
        dontMove = false;
        moveLeft = movement;
        playerAnimator.SetBool("isRunning", running);
    }

    public void DontAllowMovement()
    {
        dontMove = true;
    }

    public void MoveLeft()
    {
        playerBody.velocity = new Vector2(-playerMovementSpeed, playerBody.velocity.y);
        playerAnimator.SetBool("isRunning", running = true);
        transform.localScale = new Vector3(-1, 1, 1);
    }
    
    public void MoveRight()
    {
        playerBody.velocity = new Vector2(playerMovementSpeed, playerBody.velocity.y);
        playerAnimator.SetBool("isRunning", running = true);
        transform.localScale = Vector3.one;
    }

    public void StopMoving()
    {
        playerBody.velocity = new Vector2(0f, playerBody.velocity.y);
        playerAnimator.SetBool("grounded", IsGrounded());
        playerAnimator.SetBool("isRunning", running = false);
    }

    void DetectInput()
    {
        float x = Input.GetAxisRaw("Horizontal");

        if (x > 0)
        {
            MoveRight();
        }else if (x < 0)
        {
            MoveLeft();
        }
        else
        {
            StopMoving();
        }
    }
    
    private void Awake()
    {
        wallJumpCoolDown = 1;
        playerAnimator = GetComponent<Animator>();
    }
    public void Jump()
    {
        //cooldown del salto
        if (wallJumpCoolDown > 0.2f)
        {
            playerBody.velocity = new Vector2(horizontalInput * playerMovementSpeed, playerBody.velocity.y);
            
            //agarrarse a la pared
            if (OnWall() && !IsGrounded())
            {
                playerBody.gravityScale = gravityOnWall;
                playerBody.velocity = Vector2.zero;
            }
            else
            {
                playerBody.gravityScale = 3;
            }

            if (IsGrounded())
            {
                playerBody.velocity = new Vector2(playerBody.velocity.x, playerJumpSpeed);
                playerAnimator.SetTrigger("jump");
            }
            else if (OnWall() && !IsGrounded())
            {
                audioSource.PlayOneShot(jumpClip);
                if (horizontalInput == 0)
                {
                    audioSource.PlayOneShot(jumpClip);
                    playerBody.velocity = new Vector2(-Math.Sign(transform.localScale.x) * 10, 0);
                    transform.localScale = new Vector3(-Math.Sign(transform.localScale.x), transform.localScale.y, transform.localScale.z);
                }
                else
                {
                    playerBody.velocity = new Vector2(-Math.Sign(transform.localScale.x) * 3, 6);
                
                }

                wallJumpCoolDown = 0;
            }
        }
        else
        {
            wallJumpCoolDown += Time.deltaTime;
        }
    }
    private bool IsGrounded()
    {
        RaycastHit2D raycastHit = Physics2D.BoxCast(playerBoxCollider.bounds.center, playerBoxCollider.bounds.size, 0, Vector2.down, 0.1f, groundLayer);
        return raycastHit.collider != null;
    }
    private bool OnWall()
    {
        RaycastHit2D raycastHit = Physics2D.BoxCast(playerBoxCollider.bounds.center, playerBoxCollider.bounds.size, 0, new Vector2(transform.localScale.x, 0), 0.1f, wallLayer);
        return raycastHit.collider != null;
    }
}