using System;
using System.Collections;
using UnityEngine;
using Metroidvania.Gameplay;

namespace EnemyScript
{
    /// <summary>
    /// Se encarga de la vida del enemigo: le quita vida cuando lo golpean, lo empuja hacia
    /// atrás (retroceso), hace que parpadee un momento y se ocupa de su muerte. Usa
    /// IDamageable, que es solo una forma de decir "a este objeto se le puede hacer daño".
    /// </summary>
    public class EnemyHealth : MonoBehaviour, IDamageable
    {
        // Aviso que se manda a otros scripts en el momento en que el enemigo muere.
        public event Action<GameObject> onDeath;

        // La ficha de datos del enemigo (vida, empujones, experiencia...).
        private global::EnemyScript.Enemy _enemyScript;
        // Verdadero durante el ratito en que el enemigo parpadea y no puede recibir más golpes.
        public bool isDamaged;
        // Verdadero mientras el enemigo está siendo empujado hacia atrás.
        public bool isKnockedBack;
        // Sirve para que la muerte se haga solo una vez (y no se repita).
        private bool isDead;
        // El efecto visual (por ejemplo una explosión) que aparece cuando muere.
        public GameObject deathEffect;
        // El dibujo del enemigo. Lo usamos para cambiar su aspecto cuando parpadea.
        SpriteRenderer spriteRenderer;
        // Guarda los dos "looks" del enemigo: el normal y el de parpadeo.
        Blink material;
        // El cuerpo físico del enemigo. Lo usamos para darle el empujón.
        private Rigidbody2D rb;

        // Al empezar, buscamos y guardamos los componentes que vamos a usar.
        private void Start()
        {
            _enemyScript = GetComponent<Enemy>();
            rb = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            material = GetComponent<Blink>();
        }

        /// <summary>
        /// Le hace daño al enemigo: le quita vida, lo empuja hacia el lado contrario del golpe,
        /// lo hace parpadear y, si se queda sin vida, lo manda a morir.
        /// </summary>
        public void TakeDamage(float amount, Vector2 hitPosition)
        {
            // Si está parpadeando, no puede recibir más golpes todavía. Salimos.
            if (isDamaged) return;

            // Le quitamos vida según el daño recibido.
            _enemyScript.healthPoints -= amount;

            // Lo empujamos hacia el lado opuesto de donde vino el golpe.
            if (hitPosition.x < transform.position.x)
                {
                    rb.AddForce(new Vector2(_enemyScript.knockbackForceX, _enemyScript.knockbackForceY), ForceMode2D.Impulse);
                }
                else
                {
                    rb.AddForce(new Vector2(-_enemyScript.knockbackForceX, _enemyScript.knockbackForceY), ForceMode2D.Impulse);
                }

                // Marcamos que está siendo empujado y arrancamos los dos tiempos: el del empujón y el del parpadeo.
                isKnockedBack = true;
                StartCoroutine(ResetKnockback());
                StartCoroutine(Damager());

                // Si su vida llega a cero (o menos), el enemigo muere.
                if (_enemyScript.healthPoints <= 0)
                {
                    Die();
                }
            }

        /// <summary>
        /// Hace todo lo que pasa cuando el enemigo muere: si es un jefe suelta una medalla y lo
        /// marca como vencido; muestra el efecto de muerte, da experiencia y botín, suena un sonido,
        /// avisa a otros scripts y, por último, borra al enemigo.
        /// </summary>
        private void Die()
        {
            // Nos aseguramos de que la muerte ocurra una sola vez.
            if (isDead) return;
            isDead = true;

            // Miramos si este enemigo es un jefe.
            BossBehaviour boss = GetComponent<BossBehaviour>();
            if (boss != null)
            {
                // Si el jefe tiene una medalla configurada, la dejamos caer.
                if (boss.medalPrefab != null)
                {
                    Instantiate(boss.medalPrefab, transform.position, Quaternion.identity);
                }

                // Si existe el sistema de puntos de guardado, apuntamos que este jefe ya fue vencido.
                if (CheckpointManager.instance != null)
                {
                    // Buscamos quién activa a este jefe (aunque esté apagado) y guardamos su número de jefe como vencido.
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

            // Si hay un efecto de muerte preparado, lo mostramos en el sitio donde estaba el enemigo.
            if (deathEffect != null)
                Instantiate(deathEffect, transform.position, Quaternion.identity);
            
            // Le damos experiencia al jugador.
            if (ExperienceScript.instance != null)
                ExperienceScript.instance.expModifer(_enemyScript.ExperienceToGive);

            // Soltamos el botín (objetos que deja el enemigo al morir) donde estaba.
            if (LootManager.instance != null)
            {
                LootManager.instance.DropLoot(transform.position);
            }

            // Reproducimos el sonido de enemigo muerto.
            if (AudioManager.instance != null)
            {
                AudioManager.instance.PlayAudio(AudioManager.instance.enemyDead);
            }
            
            // Avisamos a los scripts que estaban "escuchando" la muerte y borramos al enemigo de la escena.
            onDeath?.Invoke(gameObject);
            Destroy(gameObject);
        }

        /// <summary>
        /// Otra forma de recibir daño: si algo con la etiqueta "Weapon" (un arma) toca al enemigo,
        /// le hace el daño del jugador. Se mantiene por si acaso, pero lo normal es usar TakeDamage.
        /// </summary>
        private void OnTriggerEnter2D(Collider2D collision)
        {
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

        /// <summary>Espera medio segundo y luego quita el estado de "siendo empujado".</summary>
        IEnumerator ResetKnockback()
        {
            yield return new WaitForSeconds(0.5f);
            isKnockedBack = false;
        }

        /// <summary>
        /// Hace que el enemigo parpadee medio segundo (cambia su aspecto y luego lo deja como estaba).
        /// Durante ese rato no puede recibir golpes.
        /// </summary>
        IEnumerator Damager()
        {
            isDamaged = true;
            // Antes de tocar nada, comprobamos que existe (para que el juego no falle si falta algo).
            if (spriteRenderer != null && material != null && material.blink != null)
            {
                spriteRenderer.material = material.blink;
            }

            yield return new WaitForSeconds(0.5f);

            // Volvemos a comprobar que existe antes de devolverle su aspecto normal.
            if (spriteRenderer != null && material != null && material.original != null)
            {
                spriteRenderer.material = material.original;
            }
            isDamaged = false;
        }
    }
}