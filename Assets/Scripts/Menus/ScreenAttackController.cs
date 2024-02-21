using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenAttackController : MonoBehaviour
{
    [Header("Attack params")] 
    [SerializeField] private float attackTimer;
    [SerializeField] private float attackCooldown;
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject[] fireballs;
    [SerializeField] private GameObject sword;
    
    [Header("Sound params")]
    public AudioSource audioSource;
    public AudioSource musicSource;
    public AudioClip swordAttackClip;
    public AudioClip castAttackClip;
    public AudioClip gameMusicClip;
    
    private Animator animator;
    private PlayerMovement playerMovement;
    private float cooldownTimer = Mathf.Infinity;

    private void Awake()
    {
        sword.SetActive(false);
        animator = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
        musicSource.PlayOneShot(gameMusicClip);
        sword.GetComponent<Collider2D>();
    }

    private void Update()
    {
        musicSource.loop = true;
        cooldownTimer += Time.deltaTime;
    }

    private void Attack()
    {
        audioSource.PlayOneShot(swordAttackClip);
        animator.SetTrigger("attack");
        print("Attack()");
        StartCoroutine(Waiter());
    }

    IEnumerator Waiter()
    {
        yield return new WaitForSeconds(attackTimer);
        sword.SetActive(false);
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
    
    public void AtackBtn()
    {
        if (cooldownTimer > attackCooldown && playerMovement.CanAttack() && !PauseMenu.GameIsPaused)
        {
            sword.SetActive(true);
            Attack();
        }
    }

    public void CastBtn()
    {
        if (cooldownTimer > attackCooldown && playerMovement.CanAttack() && !PauseMenu.GameIsPaused)
        {
            CastAttack();
        }
    }
}
