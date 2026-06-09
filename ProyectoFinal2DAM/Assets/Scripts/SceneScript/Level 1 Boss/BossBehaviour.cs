using EnemyScript;                       // Nos deja usar el componente Enemy (vida y velocidad del jefe)
using UnityEngine;
using UnityEngine.UI;                     // Nos deja usar imágenes (la barra de vida)
using Random = UnityEngine.Random;        // Aclara que "Random" es el de Unity (para números al azar)

/// <summary>
/// Controla cómo se porta el jefe: dispara cada cierto tiempo, se teletransporta de un sitio a otro,
/// actualiza su barra de vida, se gira para mirar al jugador y se puede reiniciar.
/// Otros jefes parten de esta clase para reaprovechar todo esto.
/// </summary>
public class BossBehaviour : MonoBehaviour
{
    public Transform[] transforms;     // Sitios a los que el jefe puede aparecer o teletransportarse
    public GameObject flame;           // La llama (disparo) que el jefe va a crear
    public GameObject muros;           // Los muros que encierran la zona del jefe
    public GameObject medalPrefab;     // La medalla (premio) que se da al derrotar al jefe

    public float timeToShoot, countdown;     // Cada cuánto dispara y cuánto falta para el próximo disparo
    public float timeToTP, countdownToTP;    // Cada cuánto se teletransporta y cuánto falta para el próximo salto

    public float bossHealth, currentHealth;  // Vida máxima (al empezar) y vida que le queda ahora
    public Image healthImg;                  // La imagen que se vacía para mostrar la vida del jefe

    // Guarda qué imagen es la barra de vida y apunta cuánta vida tiene el jefe al máximo. Se llama al aparecer el jefe.
    public void SetHealthBar(UnityEngine.UI.Image bar)
    {
        healthImg = bar;
        Enemy enemy = GetComponent<Enemy>();
        bossHealth = enemy.healthPoints;

        // Muestra el nombre del jefe en la pantalla, leyéndolo de la ficha del enemigo (enemyNam).
        if (BossUI.instance != null && enemy != null)
        {
            BossUI.instance.SetBossName(enemy.enemyNam);
        }
    }

    // Al empezar: coloca al jefe, pone a cero los relojes de disparo y salto, y apunta su barra y su vida máxima.
    private void Start()
    {
        // Pone al jefe en su sitio de inicio (el de la posición número 1)
        transform.position = transforms[1].position;
        // Reinicia los relojes de disparo y de teletransporte
        countdown = timeToShoot;
        countdownToTP = timeToTP;

        // Coge la barra de vida de la pantalla del jefe, si existe
        if (BossUI.instance != null)
        {
            healthImg = BossUI.instance.healthBar;
        }

        // Apunta la vida máxima del jefe
        bossHealth = GetComponent<Enemy>().healthPoints;
    }

    // Deja al jefe como al principio (sitio, relojes, vida, muros y puertas) y lo apaga. Se usa al reaparecer en un checkpoint.
    public void ResetBoss()
    {
        // Vuelve a su sitio de inicio y reinicia los relojes
        transform.position = transforms[1].position;
        countdown = timeToShoot;
        countdownToTP = timeToTP;

        // Le devuelve toda su vida (la que guardamos en bossHealth)
        Enemy enemy = GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.healthPoints = bossHealth;
        }

        // Abre los muros y vuelve a subir las puertas que cuelgan de ellos
        if (muros != null)
        {
            muros.SetActive(false);
            // Si los muros tienen puertas (FallingDoor), las reinicia también
            FallingDoor[] doors = muros.GetComponentsInChildren<FallingDoor>(true);
            foreach(var door in doors)
            {
                door.ResetDoor();
            }
        }

        // Apaga al jefe hasta la próxima vez que se active
        gameObject.SetActive(false);
    }

    // En cada cuadro: revisa los relojes, actualiza la barra de vida y gira al jefe hacia el jugador.
    private void Update()
    {
        CountDowns();   // Lleva la cuenta del tiempo para disparar y teletransportarse
        DamageBoss();   // Actualiza la barra de vida según la vida que le queda
        BossScale();    // Gira al jefe para que mire al jugador
    }

    // Resta tiempo a los relojes; cuando uno llega a cero, dispara o se teletransporta y vuelve a empezar la cuenta.
    public void CountDowns()
    {
        countdown -= Time.deltaTime;
        countdownToTP -= Time.deltaTime;

        // Cuando vence el temporizador de disparo, dispara y reinícialo
        if (countdown <= 0f)
        {
            ShootPlayer();
            countdown = timeToShoot;
        }

        // Cuando vence el temporizador de teletransporte, reinícialo y teletransporta
        if (countdownToTP <= 0f)
        {
            countdownToTP = timeToTP;
            Teleport();
        }
    }

    // Crea una llama (disparo) en el sitio donde está el jefe.
    public void ShootPlayer()
    {
        Instantiate(flame, transform.position, Quaternion.identity);
    }

    // Mueve al jefe de golpe a uno de sus sitios, elegido al azar.
    public void Teleport()
    {
        var initialPosition = Random.Range(0, transforms.Length);
        transform.position = transforms[initialPosition].position;
    }

    // Llena o vacía la barra de vida según la vida que le queda al jefe.
    public void DamageBoss()
    {
        // Si no hay barra de vida, no hay nada que hacer
        if (healthImg == null) return;

        // Mira la vida actual y calcula qué parte de la barra debe verse llena
        currentHealth = GetComponent<Enemy>().healthPoints;
        healthImg.fillAmount = currentHealth / bossHealth;
    }

    // Cuando el jefe es destruido (derrotado): esconde su pantalla y abre los muros para liberar al jugador.
    private void OnDestroy()
    {
        // Esconde la pantalla del jefe
        if (BossUI.instance != null)
        {
            BossUI.instance.BossDeactivator();
        }

        // Abre los muros para que el jugador pueda salir
        if (muros != null)
        {
            muros.SetActive(false);
        }
    }

    // Da la vuelta al jefe (lo voltea de lado) para que siempre mire hacia el jugador.
    public void BossScale()
    {
        // Si el jugador está a la izquierda, voltea al jefe poniendo su escala en X a -1
        if (transform.position.x > PlayerController.instance.transform.position.x)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
    }
}
