using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private float health;
    
    [Header("Sound params")]
    public AudioSource audioSource;
    public AudioClip audioClip;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            collision.GetComponent<PlayerStats>().Health(health);
            audioSource.PlayOneShot(audioClip);
            Destroy(gameObject);
        }
    }
}
