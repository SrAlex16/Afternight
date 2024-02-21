using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmallEnemy : MonoBehaviour
{
    [Header ("Main params")]
    [SerializeField] private float attackCooldown;
    [SerializeField] private float colliderDistance;
    [SerializeField] private int damage;
    [SerializeField] private int range;
    [SerializeField] private BoxCollider2D boxCollider;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private int health;
    public float currentTime;

    [Header ("Sound params")]
    public AudioSource audioSource;
    public AudioClip DeadClip;
    public AudioClip HurtClip;

    private float cooldownTimer = Mathf.Infinity;
    private Animator animator;
    private PlayerStats playerHealth;
    private DayNightSystem2D time;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        time = GetComponent<DayNightSystem2D>();
    }
    private void Update()
    {
        cooldownTimer += Time.deltaTime;

        if (PlayerInSight())
        {
            if (cooldownTimer >= attackCooldown)
            {
                cooldownTimer = 0;
            }
        }
    }

    private bool PlayerInSight()
    {
        RaycastHit2D hit = Physics2D.BoxCast(boxCollider.bounds.center + transform.right * range * transform.localScale.x * colliderDistance, 
                new Vector3(boxCollider.bounds.size.x * range, boxCollider.bounds.size.y, boxCollider.bounds.size.z),
                0, Vector2.left, 0, playerLayer);

        if (hit.collider != null)
        {
            playerHealth = hit.transform.GetComponent<PlayerStats>();
        }
        
        return hit.collider != null;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(boxCollider.bounds.center + transform.right * range * transform.localScale.x * colliderDistance,
            new Vector3(boxCollider.bounds.size.x * range, boxCollider.bounds.size.y, boxCollider.bounds.size.z));
    }

    private void DamagePlayer()
    {
        if (PlayerInSight())
        {
            if (Time.realtimeSinceStartup>currentTime) //aumento el ataque de los enemigos cuando pasa un minuto
            {
                playerHealth.TakeDamage(damage+1);
            }
            
            else
            {
                playerHealth.TakeDamage(damage);   
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "fireball"||collision.tag == "Sword")
        {
            print("sword");
            audioSource.PlayOneShot(HurtClip);
            health--;
            if (health <= 0)
            {
                //enemy is dead
                audioSource.PlayOneShot(DeadClip);
                Destroy(gameObject);
            }
        }

        if (collision.tag == "Player")
        {
            DamagePlayer();            
        }
    }
}
