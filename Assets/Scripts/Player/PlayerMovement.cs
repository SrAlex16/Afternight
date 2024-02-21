using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float playerMovementSpeed;
    [SerializeField] private float playerJumpSpeed;
    [SerializeField] private float gravityOnWall;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask wallLayer;
    private float wallJumpCoolDown;
    private float horizontalInput;
    private Rigidbody2D body;
    private Animator playerAnimator;
    private BoxCollider2D boxCollider;

    [Header("Sound params")]
    public AudioSource audioSource;
    public AudioClip jumpClip;
    public AudioClip walkClip;
    
    private void Awake()
    {
        //objetos
        body = GetComponent<Rigidbody2D>();
        playerAnimator = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded())
        {
            audioSource.PlayOneShot(jumpClip);
        }
        if (Input.GetKeyDown(KeyCode.W) && IsGrounded() || Input.GetKeyDown(KeyCode.A) && IsGrounded() || Input.GetKeyDown(KeyCode.S) && IsGrounded() || Input.GetKeyDown(KeyCode.D) && IsGrounded())
        {
            audioSource.PlayOneShot(walkClip);
        }
        
        horizontalInput = Input.GetAxis("Horizontal");

        //cambiar posición del jugador izquierda - derecha
        if (horizontalInput > 0.01f)
        {
            transform.localScale = Vector3.one;
        } 
        else if (horizontalInput < -0.01f)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }

        //parámetros de playerAnimator
        playerAnimator.SetBool("isRunning", horizontalInput != 0);
        playerAnimator.SetBool("grounded", IsGrounded());

        //cooldown del salto
        if (wallJumpCoolDown > 0.2f)
        {
            body.velocity = new Vector2(horizontalInput * playerMovementSpeed, body.velocity.y);
            
            //agarrarse a la pared
            if (OnWall() && !IsGrounded())
            {
                body.gravityScale = gravityOnWall;
                body.velocity = Vector2.zero;
            }
            else
            {
                body.gravityScale = 3;
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                Jump();
            }
        }
        else
        {
            wallJumpCoolDown += Time.deltaTime;
        }
    }
    private void Jump()
    {
        if (IsGrounded())
        {
            body.velocity = new Vector2(body.velocity.x, playerJumpSpeed);
            playerAnimator.SetTrigger("jump");
        }
        else if (OnWall() && !IsGrounded())
        {
            if (horizontalInput == 0)
            {
                body.velocity = new Vector2(-Math.Sign(transform.localScale.x) * 10, 0);
                transform.localScale = new Vector3(-Math.Sign(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
            else
            {
                body.velocity = new Vector2(-Math.Sign(transform.localScale.x) * 3, 6);
                
            }

            wallJumpCoolDown = 0;
        }
    }
    private bool IsGrounded()
    {
        RaycastHit2D raycastHit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0, Vector2.down, 0.1f, groundLayer);
        return raycastHit.collider != null;
    }
    private bool OnWall()
    {
        RaycastHit2D raycastHit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0, new Vector2(transform.localScale.x, 0), 0.1f, wallLayer);
        return raycastHit.collider != null;
    }

    public bool CanAttack()
    {
        return horizontalInput == 0 && IsGrounded() && !OnWall();
    }
}
