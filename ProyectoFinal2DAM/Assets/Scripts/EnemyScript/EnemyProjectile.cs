using UnityEngine;

/// <summary>
/// Hace que un enemigo dispare. Tiene dos formas de hacerlo:
/// - freqShooter: dispara una y otra vez cada cierto tiempo mientras ve al jugador.
/// - watcher: dispara una sola vez en cuanto ve al jugador.
/// Quien le avisa de que el jugador está cerca es el script PlayerDetect.
/// </summary>
public class EnemyProjectile : MonoBehaviour
{
    // El "molde" del disparo (la bala/objeto) que se crea al disparar.
    public GameObject projectile;

    // Cuántos segundos pasan entre un disparo y el siguiente (en modo freqShooter).
    public float timeToShoot;
    // Cuenta atrás que va bajando hasta que toca volver a disparar.
    private float shootCooldown;

    // Modo: dispara seguido, cada cierto tiempo.
    public bool freqShooter;
    // Modo: dispara solo una vez al ver al jugador.
    public bool watcher;
    // Evita que el modo "watcher" dispare más de una vez seguida.
    private bool hasShotWatcher = false;
    // Verdadero mientras el jugador está dentro de la zona de detección.
    private bool playerDetected = false;
    // Verdadero mientras el enemigo está siendo empujado; mientras tanto no puede disparar.
    private bool isKnockedBack = false;

    // Al empezar, ponemos la cuenta atrás en su valor inicial.
    void Start()
    {
        shootCooldown = timeToShoot;
    }

    // Cada frame: miramos si el enemigo está siendo empujado y, si toca, disparamos.
    void Update()
    {
        // Preguntamos al script de movimiento si el enemigo está siendo empujado (si no existe, ponemos "no").
        isKnockedBack = GetComponent<EnemyMovements>()?.isKnockedBack ?? false;

        // Disparo continuo: solo si es freqShooter, ve al jugador y no lo están empujando.
        if(freqShooter && playerDetected && !isKnockedBack)
        {
            shootCooldown -= Time.deltaTime;

            // Cuando la cuenta atrás llega a cero, dispara y vuelve a empezar la cuenta.
            if(shootCooldown <= 0f)
            {
                Shoot();
                shootCooldown = timeToShoot;
            }
        }
    }

    /// <summary>PlayerDetect llama a esto cuando el jugador entra en la zona de detección.</summary>
    public void OnPlayerEnter()
    {
        playerDetected = true;
        // El "watcher" dispara una sola vez nada más ver al jugador.
        if (watcher && !isKnockedBack && !hasShotWatcher)
        {
            Shoot();
            hasShotWatcher = true;
        }
        // El "freqShooter" reinicia su cuenta atrás al ver al jugador.
        if (freqShooter)
        {
            shootCooldown = timeToShoot;
        }
    }

    /// <summary>PlayerDetect llama a esto cuando el jugador sale de la zona de detección.</summary>
    public void OnPlayerExit()
    {
        playerDetected = false;
        // Dejamos que el "watcher" pueda volver a disparar la próxima vez que vea al jugador.
        hasShotWatcher = false;
    }

    /// <summary>Crea un disparo y lo lanza hacia el lado al que mira el enemigo.</summary>
    public void Shoot()
    {
        GameObject cross = Instantiate(projectile, transform.position, Quaternion.identity);

        // Según hacia dónde mire el enemigo (su escala en X), lanzamos el disparo a un lado u otro.
        if(transform.localScale.x < 0)
        {
            cross.GetComponent<Rigidbody2D>().AddForce(new Vector2(300f, 0f), ForceMode2D.Force);
        }
        else
        {
            cross.GetComponent<Rigidbody2D>().AddForce(new Vector2(-300f, 0f), ForceMode2D.Force);
        }
    }
}