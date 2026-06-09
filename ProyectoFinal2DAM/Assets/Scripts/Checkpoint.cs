using UnityEngine;
using UnityEngine.InputSystem; // Necesario para leer el teclado (Keyboard.current)
using TMPro;                    // Necesario para el texto del prompt (TextMeshProUGUI)

/// <summary>
/// Es un punto de guardado. Cuando el jugador se acerca, aparece un cartel que dice
/// "E guardar"; al pulsar la tecla E, se guarda la partida (a través del CheckpointManager).
/// </summary>
public class Checkpoint : MonoBehaviour
{
    public GameObject promptUI;            // El cartel que dice "E guardar" / "guardado".
    private TextMeshProUGUI promptText;    // El texto que hay dentro del cartel.
    private RectTransform promptRect;       // La posición/tamaño del cartel (para moverlo).
    private Canvas parentCanvas;            // El lienzo (Canvas) donde está el cartel.
    private bool playerInRange = false;     // ¿Está el jugador cerca del punto de guardado?
    private bool isSaved = false;           // ¿Ya hemos guardado en esta visita? (para no repetir).

    [Header("Positioning")]
    // Cuánto se sube el cartel respecto al objeto (para que salga por encima).
    public Vector3 worldOffset = new Vector3(0, 1.2f, 0);

    // Se ejecuta al empezar: busca y prepara el cartel.
    private void Start()
    {
        FindPrompt();
    }

    /// <summary>
    /// Busca el cartel "CheckpointPrompt" en la escena (si no se asignó a mano),
    /// guarda sus partes, le pone tamaño y color correctos y lo deja escondido.
    /// </summary>
    private void FindPrompt()
    {
        // Si no nos dieron el cartel, lo buscamos por su nombre entre todos los objetos
        // de la escena (incluso los apagados), quedándonos con uno que esté en la escena cargada.
        if (promptUI == null)
        {
            GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (GameObject go in allObjects)
            {
                if (go.name == "CheckpointPrompt" && go.hideFlags == HideFlags.None && go.scene.isLoaded)
                {
                    promptUI = go;
                    break;
                }
            }
        }

        if (promptUI != null)
        {
            // Guardamos las partes del cartel que vamos a usar para mostrarlo y moverlo.
            promptText = promptUI.GetComponentInChildren<TextMeshProUGUI>(true);
            promptRect = promptUI.GetComponent<RectTransform>();
            parentCanvas = promptUI.GetComponentInParent<Canvas>();
            
            promptUI.transform.localScale = Vector3.one; // Le ponemos tamaño normal por si estaba raro.
            
            if (promptText != null)
            {
                promptText.color = Color.white;
            }
            // Empezamos con el cartel escondido.
            promptUI.SetActive(false);
        }
        else
        {
            Debug.LogWarning("[Checkpoint] Could not find CheckpointPrompt in scene.");
        }
    }

    // Se ejecuta en cada fotograma: si el jugador está cerca, mira si pulsa la tecla E para guardar.
    private void Update()
    {
        // Solo escuchamos la tecla E si el jugador está cerca y aún no ha guardado en esta visita.
        if (playerInRange && !isSaved)
        {
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                SaveGame();
            }
        }
    }

    // Se ejecuta al final de cada fotograma: mantiene el cartel pegado al punto mientras se vea.
    private void LateUpdate()
    {
        if (playerInRange && promptUI != null && promptUI.activeInHierarchy)
        {
            UpdatePromptPosition();
        }
    }

    /// <summary>
    /// Coloca el cartel justo encima del punto de guardado, pasando la posición del
    /// mundo del juego a una posición en la pantalla.
    /// </summary>
    private void UpdatePromptPosition()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        // Calculamos dónde está el punto en el mundo (objeto + un poco hacia arriba) y dónde cae en pantalla.
        Vector3 worldPos = transform.position + worldOffset;
        Vector3 screenPos = cam.WorldToScreenPoint(worldPos);

        // Si el punto queda detrás de la cámara, escondemos el cartel.
        if (screenPos.z < 0) 
        {
            promptUI.SetActive(false);
            return;
        }

        if (promptRect != null && parentCanvas != null)
        {
            // Si el Canvas va "pegado a la cámara", traducimos el punto de pantalla
            // a una posición dentro del Canvas.
            if (parentCanvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                Vector2 localPos;
                RectTransform canvasRect = parentCanvas.GetComponent<RectTransform>();
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, parentCanvas.worldCamera, out localPos))
                {
                    promptRect.anchoredPosition = localPos;
                }
            }
            // Si el Canvas va "encima de toda la pantalla", usamos la posición de pantalla directamente.
            else if (parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                promptRect.position = screenPos;
            }
        }
    }

    // Guarda la partida en este punto y cambia el texto del cartel a "guardado" para avisar.
    private void SaveGame()
    {
        if (CheckpointManager.instance != null)
        {
            // Guardamos usando la posición de este punto como sitio de reaparición.
            CheckpointManager.instance.SaveCheckpoint(transform.position);
            
            isSaved = true; // Marcamos que ya guardamos, para no repetir hasta salir y volver a entrar.
            if (promptText != null)
            {
                promptText.text = "guardado";
            }
            Debug.Log("[Checkpoint] Game Saved at " + transform.position);
        }
    }

    // Se ejecuta cuando algo entra en la zona del punto: si es el jugador, muestra el cartel.
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Solo nos importa si quien entra es el jugador.
        if (other.CompareTag("Player") || (other.attachedRigidbody != null && other.attachedRigidbody.CompareTag("Player")))
        {
            // Nos aseguramos de tener el cartel localizado.
            if (promptUI == null) FindPrompt();

            playerInRange = true;
            isSaved = false; // Permitimos volver a guardar en esta visita.
            if (promptUI != null)
            {
                // Dejamos el texto en "E guardar", bien visible, y colocamos el cartel.
                if (promptText != null)
                {
                    promptText.text = "E guardar";
                    promptText.color = Color.white;
                    promptText.alpha = 1f;
                }
                promptUI.SetActive(true);
                UpdatePromptPosition();
            }
        }
    }

    // Se ejecuta cuando algo sale de la zona del punto: si es el jugador, esconde el cartel.
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") || (other.attachedRigidbody != null && other.attachedRigidbody.CompareTag("Player")))
        {
            playerInRange = false;
            if (promptUI != null)
            {
                promptUI.SetActive(false);
            }
        }
    }
}
