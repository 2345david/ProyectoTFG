using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
// Nota: se eliminó "using TMPro;" porque este script no usa ningún tipo de TextMeshPro.

/// <summary>
/// Maneja la pantalla que sale cuando el jugador muere. La enseña, pausa el juego
/// y se encarga de los botones de "reaparecer" y "volver al menú principal".
/// Solo existe una (singleton).
/// </summary>
public class DeathScreenManager : MonoBehaviour
{
    // El único DeathScreenManager que existe; otros scripts lo usan para mostrar la pantalla de muerte.
    public static DeathScreenManager instance;

    public GameObject deathScreenPanel; // La pantalla de muerte completa.
    public Button respawnButton;        // Botón para volver a la vida en el último punto guardado.
    public Button mainMenuButton;       // Botón para volver al menú principal.
    public string mainMenuSceneName = "MainMenu"; // Nombre de la escena del menú (cámbialo si se llama de otra forma).

    // Se ejecuta al nacer: deja a este como el único DeathScreenManager.
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    // Se ejecuta al empezar: busca la pantalla, la esconde y conecta los botones con sus acciones.
    private void Start()
    {
        // Si no nos dieron la pantalla a mano, la buscamos por su nombre.
        if (deathScreenPanel == null)
        {
            deathScreenPanel = GameObject.Find("DeathScreenPanel");
        }

        // Nos aseguramos de que la pantalla de muerte empiece escondida.
        if (deathScreenPanel != null)
        {
            deathScreenPanel.SetActive(false);
        }

        // Decimos a cada botón qué tiene que hacer al pulsarlo.
        if (respawnButton != null)
        {
            respawnButton.onClick.AddListener(Respawn);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(LoadMainMenu);
        }
    }

    /// <summary>
    /// Enseña la pantalla de muerte, congela el juego (el tiempo se para)
    /// y muestra el ratón para poder pulsar los botones.
    /// </summary>
    public void ShowDeathScreen()
    {
        if (deathScreenPanel != null)
        {
            deathScreenPanel.SetActive(true);
            Time.timeScale = 0f; // Para el tiempo del juego (pausa).
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    /// <summary>
    /// Devuelve la vida al jugador: vuelve a poner el tiempo en marcha, carga la
    /// última partida guardada, reinicia los jefes y esconde la pantalla de muerte.
    /// </summary>
    public void Respawn()
    {
        // Volvemos a poner el tiempo en marcha (quitamos la pausa).
        Time.timeScale = 1f;
        
        if (PlayerHealth.instance != null)
        {
            // Volvemos a encender al jugador (al morir se pudo haber apagado).
            PlayerHealth.instance.gameObject.SetActive(true);
            
            // Cargamos la última partida guardada (posición, vida, objetos, etc.).
            if (CheckpointManager.instance != null)
            {
                CheckpointManager.instance.LoadGame();
            }
            
            // Dejamos a los jefes como al principio para poder pelear otra vez.
            ResetActiveBosses();
            // Volvemos a dejar al jugador en su estado normal.
            PlayerHealth.instance.ResetStatus();
        }

        // Escondemos la pantalla de muerte.
        if (deathScreenPanel != null)
        {
            deathScreenPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Deja a los jefes y a sus zonas de activación como al principio, para que
    /// la pelea pueda empezar de nuevo cuando el jugador vuelve a la vida.
    /// </summary>
    private void ResetActiveBosses()
    {
        // Si hay barra de vida del jefe en pantalla, la escondemos/reiniciamos.
        if (BossUI.instance != null)
        {
            BossUI.instance.ResetBossUI();
        }

        // Reiniciamos el comportamiento de todos los jefes de la escena (también los apagados).
        BossBehaviour[] bosses = FindObjectsOfType<BossBehaviour>(true);
        foreach (var boss in bosses)
        {
            boss.ResetBoss();
        }

        // Reiniciamos las zonas que despiertan a los jefes.
        BossActivation[] triggers = FindObjectsOfType<BossActivation>(true);
        foreach (var trigger in triggers)
        {
            // Solo reiniciamos la zona si ese jefe todavía no ha sido derrotado.
            if (CheckpointManager.instance != null && !CheckpointManager.instance.IsBossDefeated(trigger.bossID))
            {
                trigger.ResetTrigger();
            }
        }
    }

    // Quita la pausa y lleva al jugador a la pantalla del menú principal.
    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
