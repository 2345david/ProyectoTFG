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
        health = maxHealth;

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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy") && !isInmune)
        {
            health -= collision.GetComponent<Enemy>().damageToGive;
            StartCoroutine(Inmunity());
            StartCoroutine(Knockback(collision.transform));
            
            if (health <= 0)
            {
                print("player dead");
            }
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
