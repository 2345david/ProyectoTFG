using System;
using System.Collections;
using EnemyScript;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public float health;
    public float maxHealth;
    public Image healthBar;
    private bool isInmune;
    public float inmunityTime;
    private Blink material;
    private SpriteRenderer sprite;
    public float knockbackForceX;
    public float knockbackForceY;
    public float knockbackTime;
    Rigidbody2D rb;
    private PlayerController playerController;

    public static PlayerHealth instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerController = GetComponent<PlayerController>();
        sprite = GetComponent<SpriteRenderer>();
        material = GetComponent<Blink>();
        
        // Solo inicializamos salud si no ha sido cargada o es 0
        if (health <= 0) health = maxHealth;

        if (material != null)
        {
            material.original = sprite.material;
        }
    }

    void Update()
    {
        healthBar.fillAmount = health / maxHealth;
        
        if (health > maxHealth)
        {
            health = maxHealth;
        }
    }

    public void ResetStatus()
    {
        if (sprite != null && material != null && material.original != null)
        {
            sprite.material = material.original;
        }
        
        if (playerController != null)
        {
            playerController.ResetController();
        }
        
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
        
        isInmune = false;
        health = maxHealth;
    }

    public void TakeDamage(float damage, Transform damageSource = null)
    {
        if (isInmune || health <= 0) return;

        health -= damage;
        StartCoroutine(Inmunity());
        
        if (damageSource != null)
        {
            StartCoroutine(Knockback(damageSource));
        }

        if (health <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        if (health > 0) health = 0;
        Debug.Log("Player Dead!");

        if (AudioManager.instance != null)
            AudioManager.instance.PlayAudio(AudioManager.instance.playerDead);

        if (DeathScreenManager.instance != null)
        {
            DeathScreenManager.instance.ShowDeathScreen();
        }
        else
        {
            Debug.LogError("DeathScreenManager instance not found!");
        }

        gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy") && !isInmune)
        {
            EnemyScript.Enemy enemy = collision.GetComponent<EnemyScript.Enemy>();
            float damage = enemy != null ? enemy.damageToGive : 10f;
            TakeDamage(damage, collision.transform);
        }
    }

    IEnumerator Inmunity()
    {
        isInmune = true;

        if (material != null && material.blink != null)
        {
            sprite.material = material.blink;
        }

        yield return new WaitForSeconds(inmunityTime);

        if (material != null)
        {
            sprite.material = material.original;
        }

        isInmune = false;
    }

    IEnumerator Knockback(Transform enemyTransform)
    {
        playerController.canMove = false;
        Vector2 knockbackDirection = (transform.position - enemyTransform.position).normalized;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(new Vector2(knockbackDirection.x * knockbackForceX, knockbackForceY), ForceMode2D.Impulse);

        yield return new WaitForSeconds(knockbackTime);

        playerController.canMove = true;
    }
}
