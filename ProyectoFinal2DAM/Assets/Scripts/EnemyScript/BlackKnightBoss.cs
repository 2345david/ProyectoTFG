using UnityEngine;
using System.Collections;

/// <summary>
/// El jefe "Black Knight" (Caballero Negro) de las cuevas. Aprovecha lo que ya hace el jefe
/// genérico (BossBehaviour: barra de vida, mirar al jugador, daño...) y le añade su forma de
/// actuar: persigue al jugador, le pega con golpes y espera un poco entre ataque y ataque.
/// </summary>
public class BlackKnightBoss : BossBehaviour
{
    [Header("Black Knight Specific")]
    // Lo rápido que corre detrás del jugador.
    public float chaseSpeed = 3f;
    // A qué distancia está lo bastante cerca como para atacar.
    public float attackDistance = 2.5f;
    // Hasta qué distancia se molesta en perseguir al jugador.
    public float chaseDistance = 15f;
    // Cuánto espera entre una acción y la siguiente.
    public float actionCooldown = 1.5f;

    [Header("Hitboxes")]
    // La zona de golpe que se enciende cuando ataca (se asigna en el Inspector).
    public GameObject attackHitbox;
    // La zona de golpe del propio cuerpo del jefe (se asigna en el Inspector).
    public GameObject bodyHitbox;

    // Componentes que guardamos al despertar para no buscarlos cada vez.
    private Rigidbody2D rb;
    private Animator anim;
    // El jugador al que persigue.
    private Transform player;
    // Verdadero mientras está haciendo un ataque; mientras tanto no empieza otra cosa.
    private bool isActing = false;
    // Momento a partir del cual ya puede hacer la siguiente acción.
    private float nextActionTime = 0f;

    // Al despertar, guardamos los componentes y apagamos la zona de golpe hasta que haga falta.
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        if (attackHitbox != null) attackHitbox.SetActive(false);
    }

    // Al empezar: prepara la barra de vida, busca al jugador y coloca al jefe en su sitio.
    private void Start()
    {
        // Conecta la barra de vida del jefe con la pantalla.
        if (BossUI.instance != null)
        {
            SetHealthBar(BossUI.instance.healthBar);
        }

        // Buscamos al jugador.
        if (PlayerController.instance != null)
        {
            player = PlayerController.instance.transform;
        }

        // Lo ponemos en su primer punto de aparición (esa lista viene del jefe genérico).
        if (transforms != null && transforms.Length > 0)
        {
            transform.position = transforms[0].position;
        }
    }

    // Cada frame el jefe piensa qué hacer: atacar, perseguir o esperar.
    private void Update()
    {
        // Si todavía no encontramos al jugador, intentamos de nuevo y salimos por este frame.
        if (player == null)
        {
            if (PlayerController.instance != null) player = PlayerController.instance.transform;
            return;
        }

        DamageBoss(); // Actualiza la barra de vida (viene del jefe genérico).
        BossScale(); // Hace que el jefe mire hacia el jugador (viene del jefe genérico).

        // Si ya está en mitad de una acción, no decide nada nuevo.
        if (isActing) return;

        // Miramos a qué distancia está el jugador para decidir qué hacer.
        float distToPlayer = Vector2.Distance(transform.position, player.position);

        // Solo decide algo nuevo cuando ha pasado el tiempo de espera.
        if (Time.time > nextActionTime)
        {
            DecideAction(distToPlayer);
        }
    }

    /// <summary>Decide la acción según la distancia al jugador: atacar, perseguir o quedarse quieto.</summary>
    private void DecideAction(float dist)
    {
        if (dist < attackDistance)
        {
            // En rango de ataque: lanza un ataque.
            StartCoroutine(PerformAttack());
        }
        else if (dist < chaseDistance)
        {
            // Dentro del rango de persecución: persigue.
            ChasePlayer();
        }
        else
        {
            // Demasiado lejos: se detiene y deja de correr.
            anim.SetBool("isRunning", false);
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    /// <summary>Mueve al jefe horizontalmente hacia el jugador a chaseSpeed.</summary>
    private void ChasePlayer()
    {
        anim.SetBool("isRunning", true);
        // Dirección 1 (derecha) o -1 (izquierda) según dónde esté el jugador.
        float direction = player.position.x > transform.position.x ? 1 : -1;
        rb.linearVelocity = new Vector2(direction * chaseSpeed, rb.linearVelocity.y);
    }

    /// <summary>
    /// El ataque del jefe paso a paso: elige al azar uno de tres golpes, enciende la zona de golpe
    /// justo en el momento de pegar, la apaga después y luego espera antes de poder hacer otra cosa.
    /// </summary>
    private IEnumerator PerformAttack()
    {
        // Marcamos que está atacando y lo paramos en el sitio.
        isActing = true;
        anim.SetBool("isRunning", false);
        rb.linearVelocity = Vector2.zero;

        // Elegimos un golpe al azar (0, 1 o 2) y ponemos cuánto dura por defecto.
        int r = Random.Range(0, 3);
        float attackDuration = 1.5f;

        if (r == 0) anim.SetTrigger("attack1");
        else if (r == 1) anim.SetTrigger("attack2");
        else 
        {
            anim.SetTrigger("weaponArt");
            attackDuration = 2.5f; // El golpe especial "weapon art" dura más.
        }

        // Esperamos a que la animación llegue al momento justo del golpe.
        yield return new WaitForSeconds(0.4f);
        
        // Encendemos la zona de golpe (durante este rato sí hace daño).
        if (attackHitbox != null) attackHitbox.SetActive(true);
        
        yield return new WaitForSeconds(0.6f); // Cuánto tiempo está encendida la zona de golpe.
        
        if (attackHitbox != null) attackHitbox.SetActive(false);

        // Esperamos el resto de la animación antes de poder hacer otra acción.
        yield return new WaitForSeconds(attackDuration - 1.0f); 
        
        isActing = false;
        nextActionTime = Time.time + actionCooldown;
    }

    /// <summary>
    /// Permite terminar la acción desde la propia animación (un "Animation Event"), para más precisión.
    /// </summary>
    public void EndAction()
    {
        isActing = false;
        nextActionTime = Time.time + actionCooldown;
    }
}
