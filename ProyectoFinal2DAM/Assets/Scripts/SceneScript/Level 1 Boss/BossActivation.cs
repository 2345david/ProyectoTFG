using System.Collections; // Nos deja usar corrutinas (tareas que esperan un rato antes de seguir)
using UnityEngine;

/// <summary>
/// Hace que el jefe aparezca cuando el jugador entra en su zona (una zona invisible que detecta al jugador).
/// Si el jefe ya estaba derrotado, deja todo apagado. Al aparecer, muestra la pantalla del jefe,
/// cierra los muros y deja al jugador quieto unos segundos mientras llega el jefe.
/// </summary>
public class BossActivation : MonoBehaviour
{
    public GameObject bossGO;                  // El jefe que vamos a encender
    public GameObject muros;                   // Los muros que encierran la zona del jefe
    public string bossID = "Level1Boss";       // Nombre del jefe para saber si ya fue derrotado (checkpoints)

    private bool isTriggered = false;          // Evita que el jefe aparezca más de una vez

    // Al empezar: si el jefe ya estaba vencido, deja todo apagado. Si no, esconde al jefe y deja los muros abiertos esperando.
    private void Start()
    {
        // Si el jefe ya fue derrotado antes, apaga jefe, muros y este detector
        if (CheckpointManager.instance != null && CheckpointManager.instance.IsBossDefeated(bossID))
        {
            if (bossGO != null) bossGO.SetActive(false);
            if (muros != null) muros.SetActive(false);
            gameObject.SetActive(false);
            return;
        }

        // Estado de inicio: jefe escondido y muros abiertos
        bossGO.SetActive(false);
        if (muros != null)
        {
            muros.SetActive(false);
        }
    }

    // Vuelve a dejar este detector listo para activarse otra vez (por ejemplo, después de reiniciar al jefe).
    public void ResetTrigger()
    {
        isTriggered = false;
        gameObject.SetActive(true);
    }

    // Cuando algo entra en la zona: si es el jugador, muestra la pantalla del jefe, cierra los muros y empieza a hacer aparecer al jefe.
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Si ya se activó antes, no hace nada más
        if (isTriggered) return;

        // Solo reacciona si quien entra es el jugador
        if (collision.gameObject.tag == "Player")
        {
            isTriggered = true;
            BossUI.instance.BossActivator(); // Muestra la pantalla y la barra de vida del jefe

            // Cierra los muros para dejar al jugador encerrado con el jefe
            if (muros != null)
            {
                muros.SetActive(true);
            }

            // Empieza la tarea que hace aparecer al jefe
            StartCoroutine(WaitForBoss());
        }
    }

    // Tarea de aparición del jefe: deja al jugador quieto, enciende al jefe, le pasa los muros y la barra de vida,
    // espera 2 segundos y luego le devuelve el movimiento al jugador.
    IEnumerator WaitForBoss()
    {
        // Apunta la velocidad normal del jugador para devolvérsela luego
        var currentSpeed = PlayerController.instance.speed;

        // Pone la velocidad del jugador a 0 (no se puede mover) mientras llega el jefe
        PlayerController.instance.speed = 0;
        bossGO.SetActive(true);

        // Le da al jefe los muros y la barra de vida que tiene que usar
        BossBehaviour bb = bossGO.GetComponent<BossBehaviour>();
        if (bb != null)
        {
            bb.muros = muros;

            if (BossUI.instance != null)
            {
                BossUI.instance.BossActivator();
                bb.SetHealthBar(BossUI.instance.healthBar);
            }
        }

        // Espera 2 segundos de presentación y luego devuelve la velocidad al jugador
        yield return new WaitForSeconds(2f);
        PlayerController.instance.speed = currentSpeed;
        gameObject.SetActive(false);
    }
}
