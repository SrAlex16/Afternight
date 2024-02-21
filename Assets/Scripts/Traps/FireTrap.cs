using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireTrap : MonoBehaviour
{
    [SerializeField] private float activationDelay;
    [SerializeField] private float activeTime;
    [SerializeField] private float damage;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private bool triggered; //cuando el trigger de la trampa se activa
    private bool active; //cuando la trampa está activa y puede dañar al player

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            if (!triggered)
            {
                StartCoroutine(ActivateFiretrap());
            }

            if (active)
            {
                collision.GetComponent<PlayerStats>().TakeDamage(damage);
            }
        }
    }

    private IEnumerator ActivateFiretrap()
    {
        //pinta de rojo el sprite para avisar al player y activa la trampa
        triggered = true;
        spriteRenderer.color = Color.red;
        
        //espera al delay, activa la trampa y pone el color normal
        yield return new WaitForSeconds(activationDelay);
        spriteRenderer.color = Color.white;
        active = true;
        animator.SetBool("activated", true);
        
        //espera durante x segundos y resetea las variables
        yield return new WaitForSeconds(activeTime);
        active = false;
        triggered = false;
    }
}
