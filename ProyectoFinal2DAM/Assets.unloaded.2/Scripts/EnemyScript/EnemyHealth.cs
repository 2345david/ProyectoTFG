using System;
using System.Collections;
using UnityEngine;
using Metroidvania.Gameplay;

namespace EnemyScript
{
    public class EnemyHealth : MonoBehaviour, IDamageable
    {
        public event Action<GameObject> onDeath;

        private global::EnemyScript.Enemy _enemyScript;
        public bool isDamaged;
        public bool isKnockedBack;
        private bool isDead;
        public GameObject deathEffect;
        SpriteRenderer spriteRenderer;
        Blink material;
        private Rigidbody2D rb;

        private void Start()
        {
            _enemyScript = GetComponent<Enemy>();
            rb = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            material = GetComponent<Blink>();
        }

        public void TakeDamage(float amount, Vector2 hitPosition)
        {
            if (isDamaged) return;

            _enemyScript.healthPoints -= amount;

            // Knockback logic
            if (hitPosition.x < transform.position.x)
                {
                    rb.AddForce(new Vector2(_enemyScript.knockbackForceX, _enemyScript.knockbackForceY), ForceMode2D.Impulse);
                }
                else
                {
                    rb.AddForce(new Vector2(-_enemyScript.knockbackForceX, _enemyScript.knockbackForceY), ForceMode2D.Impulse);
                }

                isKnockedBack = true;
                StartCoroutine(ResetKnockback());
                StartCoroutine(Damager());

                if (_enemyScript.healthPoints <= 0)
                {
                    Die();
                }
            }

        private void Die()
        {
            if (isDead) return;
            isDead = true;

            // Check if it's a boss
            BossBehaviour boss = GetComponent<BossBehaviour>();
            if (boss != null)
            {
                // Find the trigger to get the bossID
                BossActivation trigger = FindObjectOfType<BossActivation>(true);
                // Note: This might be unreliable if multiple bosses exist, but usually there's only one active.
                // Better to have the ID on the boss itself or passed down.
                // Let's assume we can find the relevant ID.
                
                if (CheckpointManager.instance != null)
                {
                    // Since BossActivation is deactivated, we need to find it inactive.
                    // Or we could have added the ID to BossBehaviour directly.
                    // Let's try to find it on the boss behavior if I added it.
                    // I didn't add it to BossBehaviour yet.
                    
                    // Let's find any BossActivation that references this bossGO.
                    BossActivation[] activations = FindObjectsOfType<BossActivation>(true);
                    foreach(var act in activations)
                    {
                        if (act.bossGO == gameObject)
                        {
                            CheckpointManager.instance.MarkBossDefeated(act.bossID);
                            break;
                        }
                    }
                }
            }

            if (deathEffect != null)
                Instantiate(deathEffect, transform.position, Quaternion.identity);
            
            if (ExperienceScript.instance != null)
                ExperienceScript.instance.expModifer(_enemyScript.ExperienceToGive);

            if (LootManager.instance != null)
            {
                LootManager.instance.DropLoot(transform.position);
            }

            if (AudioManager.instance != null)
            {
                AudioManager.instance.PlayAudio(AudioManager.instance.enemyDead);
            }
            
            onDeath?.Invoke(gameObject);
            Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            // We keep this for backward compatibility or direct weapon tag checks, 
            // but the preferred way is now calling TakeDamage from the attacker.
            if (collision.CompareTag("Weapon") && !isDamaged)
            {
                if (PlayerAttack.instance != null)
                {
                    TakeDamage(PlayerAttack.instance.damage, collision.transform.position);
                }
                else
                {
                    Debug.LogWarning("Enemy tried to take damage but PlayerAttack.instance is null!");
                }
            }
        }

        IEnumerator ResetKnockback()
        {
            yield return new WaitForSeconds(0.5f);
            isKnockedBack = false;
        }

        IEnumerator Damager()
        {
            isDamaged = true;
            spriteRenderer.material = material.blink;
            yield return new WaitForSeconds(0.5f);
            spriteRenderer.material = material.original;
            isDamaged = false;
        }
    }
}