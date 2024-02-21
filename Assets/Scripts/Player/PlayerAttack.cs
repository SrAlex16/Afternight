using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack params")]
    [SerializeField] private float attackCooldown;
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject[] fireballs;

    [Header("Sound params")]
    public AudioSource audioSource;
    public AudioClip swordAttackClip;
    public AudioClip castAttackClip;
    public AudioClip gameMusicClip;
    
    private Animator animator;
    private PlayerMovement playerMovement;
    private float cooldownTimer = Mathf.Infinity;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
        //audioSource.PlayOneShot(gameMusicClip);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && cooldownTimer > attackCooldown && playerMovement.CanAttack() && !PauseMenu.GameIsPaused)
        {
            CastAttack();
        }
        if (Input.GetKeyDown(KeyCode.Mouse0) && cooldownTimer > attackCooldown && playerMovement.CanAttack() && !PauseMenu.GameIsPaused)
        {
            Attack();
        }

        cooldownTimer += Time.deltaTime;
    }
    
    private void Attack()
    {
        audioSource.PlayOneShot(swordAttackClip);
        animator.SetTrigger("attack");
    }

    private void CastAttack()
    {
        audioSource.PlayOneShot(castAttackClip);
        
        animator.SetTrigger("cast");
        cooldownTimer = 0;
        
        fireballs[FindFireball()].transform.position = firePoint.position;
        fireballs[FindFireball()].GetComponent<Projectile>().SetDirection(Mathf.Sign(transform.localScale.x));
    }

    private int FindFireball()
    {
        for (int i = 0; i < fireballs.Length; i++)
        {
            if (!fireballs[i].activeInHierarchy)
            {
                return i;
            }
        }

        return 0;
    }
}
