using System;
using System.Collections;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    private Animator anim;
    [SerializeField] private GameObject deathMenu;

    [Header("Health")]
    [SerializeField] private float startingHealth;
    public float currentHealth { get; private set; }
    
    [Header("Invulnerability")]
    [SerializeField] private float invunerabilityDuration;
    [SerializeField] private int flashes;
    private SpriteRenderer spriteRenderer;
    
    [Header("Sound params")]
    public AudioSource audioSource;
    public AudioClip damageClip;
    public AudioClip deadClip;
    
    private void Awake()
    {
        currentHealth = startingHealth;
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    public void TakeDamage(float _damage)
    {
        currentHealth = Mathf.Clamp(currentHealth - _damage, 0, startingHealth);

        if (currentHealth > 0)
        {
            audioSource.PlayOneShot(damageClip);
            anim.SetTrigger("hurt");
            StartCoroutine(Invunerability());
        }
        else
        {
            audioSource.PlayOneShot(deadClip);
            anim.SetTrigger("die");
            StartCoroutine(Waiter());
        }
    }

    IEnumerator Waiter()
    {
        //Wait for 5 seconds
        yield return new WaitForSecondsRealtime(1);
        deathMenu.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Health(float _health)
    {
        currentHealth = Mathf.Clamp(currentHealth + _health, 0, startingHealth);
    }
    private IEnumerator Invunerability()
    {
        Physics2D.IgnoreLayerCollision(10, 11, true);
        for (int i = 0; i < flashes; i++)
        {
            spriteRenderer.color = new Color(1, 0, 0, 0.5f);
            yield return new WaitForSeconds(invunerabilityDuration/(flashes * 2));
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(1);
        }
        Physics2D.IgnoreLayerCollision(10, 11, false);
    }
}
