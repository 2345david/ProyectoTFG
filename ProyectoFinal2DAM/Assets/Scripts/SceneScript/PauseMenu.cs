using UnityEngine;
using UnityEngine.InputSystem; // Nos deja leer el teclado (saber qué teclas pulsa el jugador)
using UnityEngine.SceneManagement; // Nos deja cambiar de pantalla (escena), como ir al menú principal

/// <summary>
/// Maneja el menú de pausa. Cuando pulsas la tecla Escape, el juego se congela
/// y aparece el menú; si la vuelves a pulsar, el juego sigue.
/// También se asegura de que exista el inventario del jugador y su pantalla.
/// </summary>
public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenu;                          // El panel que se ve cuando el juego está en pausa
    [SerializeField] Sprite inventoryPotionSprite;        // Dibujo (sprite) de la poción para el inventario
    [SerializeField] Sprite inventoryArrowSprite;         // Dibujo de la flecha para el inventario
    [SerializeField] Sprite inventoryManaPotionSprite;    // Dibujo de la poción de maná para el inventario
    bool isPaused;                                         // ¿Está el juego en pausa ahora mismo? (sí / no)

    // Al despertar: pone el juego a velocidad normal, esconde el menú y prepara el inventario.
    void Awake()
    {
        Time.timeScale = 1;          // Velocidad normal del juego (1 = normal, 0 = congelado)
        pauseMenu.SetActive(false);  // Esconde el panel de pausa
        isPaused = false;            // Al empezar no estamos en pausa
        EnsurePlayerInventory();     // Comprueba que exista el inventario del jugador
        EnsurePauseInventoryUI();    // Comprueba que el panel tenga su pantalla de inventario
    }

    // Si no hay ningún inventario del jugador en la escena, crea uno nuevo.
    void EnsurePlayerInventory()
    {
        // Si ya existe un inventario (esté visible o escondido), no creamos otro
        if (FindAnyObjectByType<PlayerInventory>(FindObjectsInactive.Include) != null)
            return;
        // Crea un objeto nuevo y le pone el inventario del jugador
        var go = new GameObject("PlayerInventory");
        go.AddComponent<PlayerInventory>();
    }

    // Se asegura de que el panel de pausa tenga la pantalla de inventario y le pasa los dibujos de los objetos.
    void EnsurePauseInventoryUI()
    {
        if (pauseMenu == null)
            return;
        // Busca la pantalla de inventario en el panel; si no la tiene, se la añade
        var ui = pauseMenu.GetComponent<PauseInventoryUI>();
        if (ui == null)
            ui = pauseMenu.AddComponent<PauseInventoryUI>();
        // Le entrega a esa pantalla los dibujos que elegiste en el Inspector
        ui.Configure(inventoryPotionSprite, inventoryArrowSprite, inventoryManaPotionSprite);
    }

    // Cada cuadro del juego: revisa si hay que pausar o continuar.
    public void Update()
    {
        Pause();
    }

    // Si pulsas Escape, cambia entre pausa y juego: congela o reanuda el tiempo y muestra u oculta el menú.
    public void Pause()
    {
        // Lee el teclado. Si no hay teclado conectado, no hace nada.
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        // Solo actuamos en el momento exacto en que se pulsa Escape.
        if (!keyboard.escapeKey.wasPressedThisFrame) return;

        // Cambia de estado: si estaba jugando lo pausa, y si estaba pausado lo reanuda.
        if (!isPaused)
        {
            Time.timeScale = 0;          // Congela el tiempo: el juego se queda quieto
            pauseMenu.SetActive(true);   // Muestra el menú de pausa
            isPaused = true;
        }
        else
        {
            Time.timeScale = 1;          // Vuelve el tiempo a la normalidad
            pauseMenu.SetActive(false);  // Esconde el menú de pausa
            isPaused = false;
        }
    }

    // Vuelve al menú principal SIN guardar. Solo se guarda en los checkpoints (puntos de control),
    // así que se pierde lo jugado desde el último checkpoint. Borra los datos que viajan entre
    // pantallas para empezar limpio en el menú y en la próxima partida.
    public void GoToMainMenu()
    {
        Time.timeScale = 1f;             // Devuelve el tiempo a normal por si estaba en pausa
        isPaused = false;

        if (CheckpointManager.instance != null)
            Destroy(CheckpointManager.instance.gameObject);
        if (Metroidvania.Core.WorldManager.Instance != null)
            Destroy(Metroidvania.Core.WorldManager.Instance.gameObject);

        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }
}