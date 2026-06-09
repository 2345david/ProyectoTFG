using System.Collections;
using UnityEngine;

namespace EnemyScript
{
    public class EnemyHealth : MonoBehaviour
    {
        private global::EnemyScript.Enemy _enemyScript;
        public bool isDamaged;
        public bool isKnockedBack;
        public GameObject deathEffect;
        SpriteRenderer spriteRenderer;
        Blink material;
        private Rigidbody2D rb;

        private void Start()
        {
            _enemyScript = GetComponent<Enemy>();
            rb = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            material = GetComponent<Blink>();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Weapon") && !isDamaged)
            {
                _enemyScript.healthPoints -= PlayerAttack.instance.damage;

                if (collision.transform.position.x < transform.position.x)
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
                    Instantiate(deathEffect, transform.position, Quaternion.identity);
                    ExperienceScript.instance.expModifer(GetComponent<Enemy>().ExperienceToGive);

                    // Justo antes de Destroy(gameObject) en EnemyHealth.cs
                    var uid = GetComponent<UniqueID>();
                    if (uid != null)
                        WorldStateTracker.Instance.RegisterDeadEnemy(uid.ID);

                    Destroy(gameObject);
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