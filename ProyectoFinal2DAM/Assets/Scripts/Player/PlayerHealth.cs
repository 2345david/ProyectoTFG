using System.Collections;     // Sirve para usar corrutinas (tareas que esperan un tiempo antes de seguir).
using UnityEngine;
using UnityEngine.UI;         // Sirve para usar la barra de vida (componente Image) en la pantalla.

/// <summary>
/// Este script lleva la vida del jugador.
/// Se encarga de recibir daño, dar unos segundos de invencibilidad, empujar al jugador
/// cuando lo golpean, mostrar la barra de vida y avisar cuando el jugador muere.
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    public float health;                    // Vida que tiene ahora mismo el jugador.
    public float maxHealth;                 // Vida máxima que puede tener el jugador.
    public Image healthBar;                 // La barra de vida que se ve en la pantalla.
    private bool isInmune;                   // Vale "true" mientras el jugador no puede recibir daño (invencible).
    public float inmunityTime;              // Cuántos segundos dura la invencibilidad tras un golpe.
    private Blink material;                  // Componente que hace parpadear al jugador cuando lo golpean.
    private SpriteRenderer sprite;           // El dibujo del jugador (lo usamos para cambiar su aspecto al parpadear).
    public float knockbackForceX;           // Fuerza del empujón hacia los lados al recibir daño.
    public float knockbackForceY;           // Fuerza del empujón hacia arriba al recibir daño.
    public float knockbackTime;             // Cuánto tiempo el jugador no puede moverse durante el empujón.
    Rigidbody2D rb;                          // El cuerpo físico del jugador (lo que hace que las fuerzas le afecten).
    private PlayerController playerController;// El script que controla el movimiento (para poder bloquearlo).

    // "instance" es un atajo para que otros scripts puedan tocar la vida del jugador desde cualquier sitio.
    public static PlayerHealth instance;

    // Esto se ejecuta al crearse el objeto. Guardamos el atajo "instance" para que otros lo encuentren.
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    
    // Esto se ejecuta una vez al empezar. Buscamos los componentes que necesitamos y preparamos la vida.
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerController = GetComponent<PlayerController>();
        sprite = GetComponent<SpriteRenderer>();
        material = GetComponent<Blink>();
        
        // Si la vida llega vacía (no se cargó una partida guardada), la llenamos al máximo.
        if (health <= 0) health = maxHealth;

        // Guardamos el aspecto normal del jugador para poder devolvérselo después de que parpadee.
        if (material != null)
        {
            material.original = sprite.material;
        }
    }

    // Esto se repite muchas veces por segundo. Actualiza la barra de vida y evita pasarse de la vida máxima.
    void Update()
    {
        // Ponemos la barra más o menos llena según la vida que quede.
        healthBar.fillAmount = health / maxHealth;
        
        // Si por algún motivo la vida pasó del máximo, la dejamos en el máximo.
        if (health > maxHealth)
        {
            health = maxHealth;
        }
    }

    // Deja al jugador como nuevo (aspecto, control, velocidad y vida llena). Se usa al revivir.
    public void ResetStatus()
    {
        // Le devolvemos su aspecto normal.
        if (sprite != null && material != null && material.original != null)
        {
            sprite.material = material.original;
        }
        
        // Reiniciamos el control del jugador.
        if (playerController != null)
        {
            playerController.ResetController();
        }
        
        // Frenamos cualquier movimiento que le quedara.
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
        
        isInmune = false;
        health = maxHealth;
    }

    // Hace que el jugador reciba daño. Si está invencible o muerto, no pasa nada.
    // Si conocemos quién le pegó, también lo empuja. Si la vida llega a 0, el jugador muere.
    public void TakeDamage(float damage, Transform damageSource = null)
    {
        // Si el jugador es invencible o ya está muerto, no le hacemos daño.
        if (isInmune || health <= 0) return;

        health -= damage;
        StartCoroutine(Inmunity());     // Le damos unos segundos de invencibilidad.
        
        // Si sabemos de dónde vino el golpe, empujamos al jugador para alejarlo.
        if (damageSource != null)
        {
            StartCoroutine(Knockback(damageSource));
        }

        // Si la vida se acabó, el jugador muere.
        if (health <= 0)
        {
            Die();
        }
    }

    // Hace que el jugador muera: suena el sonido de muerte, aparece la pantalla de muerte y se apaga el jugador.
    public void Die()
    {
        if (health > 0) health = 0;
        Debug.Log("Player Dead!");

        // Reproducimos el sonido de muerte del jugador (si está disponible).
        if (AudioManager.instance != null)
            AudioManager.instance.PlayAudio(AudioManager.instance.playerDead);

        // Mostramos la pantalla de "has muerto" si existe quien la controla.
        if (DeathScreenManager.instance != null)
        {
            DeathScreenManager.instance.ShowDeathScreen();
        }
        else
        {
            Debug.LogError("DeathScreenManager instance not found!");
        }

        gameObject.SetActive(false);    // Apagamos al jugador (deja de existir en la escena).
    }

    // Se ejecuta solo cuando el jugador toca algo. Si toca un enemigo, recibe daño.
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Solo nos importa si lo que tocamos es un enemigo y no estamos invencibles.
        if (collision.CompareTag("Enemy") && !isInmune)
        {
            // Preguntamos al enemigo cuánto daño hace. Si no lo dice, usamos 10 por defecto.
            EnemyScript.Enemy enemy = collision.GetComponent<EnemyScript.Enemy>();
            float damage = enemy != null ? enemy.damageToGive : 10f;
            TakeDamage(damage, collision.transform);
        }
    }

    // Tarea con espera: hace al jugador invencible y parpadeando un rato, y luego lo vuelve a la normalidad.
    IEnumerator Inmunity()
    {
        isInmune = true;

        // Le ponemos el aspecto de parpadeo (si existe) para avisar de que está invencible.
        if (material != null && material.blink != null)
        {
            sprite.material = material.blink;
        }

        yield return new WaitForSeconds(inmunityTime);  // Esperamos los segundos que dura la invencibilidad.

        // Le devolvemos su aspecto normal.
        if (material != null)
        {
            sprite.material = material.original;
        }

        isInmune = false;
    }

    // Tarea con espera: empuja al jugador lejos del enemigo y le quita el control un momento.
    IEnumerator Knockback(Transform enemyTransform)
    {
        playerController.canMove = false;   // Le quitamos el control mientras lo empujamos.
        // Calculamos hacia dónde empujar: en sentido contrario al enemigo.
        Vector2 knockbackDirection = (transform.position - enemyTransform.position).normalized;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(new Vector2(knockbackDirection.x * knockbackForceX, knockbackForceY), ForceMode2D.Impulse);

        yield return new WaitForSeconds(knockbackTime);

        playerController.canMove = true;    // Le devolvemos el control al jugador.
    }
}
