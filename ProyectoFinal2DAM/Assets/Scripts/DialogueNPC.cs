using UnityEngine;
using UnityEngine.InputSystem; // Necesario para leer el teclado (Keyboard.current)
using TMPro;                    // Necesario para los textos de UI (TextMeshProUGUI)

/// <summary>
/// NPC con el que se puede dialogar. Muestra un prompt "E hablar" sobre su cabeza
/// cuando el jugador está cerca; al pulsar E inicia un diálogo línea a línea que
/// pausa el movimiento del jugador. Usa el New Input System.
/// </summary>
public class DialogueNPC : MonoBehaviour
{
    [Header("NPC Data")]
    public string npcName = "NPC"; // Nombre que se muestra durante el diálogo

    [TextArea(2, 4)]
    public string[] dialogueLines; // Líneas de diálogo que se muestran en orden

    [Header("UI References (auto-found if null)")]
    public GameObject dialoguePanel;        // Panel que contiene los textos del diálogo
    public TextMeshProUGUI nameText;        // Muestra el nombre del NPC
    public TextMeshProUGUI dialogueText;    // Muestra la línea actual
    public GameObject promptUI;             // Cartel "E hablar" sobre la cabeza del NPC

    [Header("Prompt Positioning")]
    // Desplazamiento en el mundo del cartel respecto al NPC.
    public Vector3 promptWorldOffset = new Vector3(0f, 1.2f, 0f);

    private TextMeshProUGUI promptText; // Texto del cartel "E hablar"
    private RectTransform promptRect;    // RectTransform del cartel (para posicionarlo)
    private Canvas promptCanvas;         // Canvas que contiene el cartel

    private bool playerInRange = false; // ¿Está el jugador dentro del trigger?
    private bool isTalking = false;     // ¿Hay un diálogo en curso?
    private int currentLine = 0;        // Índice de la línea de diálogo actual

    private void Start()
    {
        // Localizamos referencias y dejamos panel y cartel ocultos al inicio.
        AutoFindReferences();

        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (promptUI != null) promptUI.SetActive(false);
    }

    /// <summary>
    /// Busca automáticamente el panel de diálogo, sus textos y el cartel de prompt
    /// si no se han asignado en el Inspector. Reutiliza el "CheckpointPrompt" como cartel.
    /// </summary>
    private void AutoFindReferences()
    {
        // Buscamos el panel de diálogo por nombre si no está asignado.
        if (dialoguePanel == null)
        {
            GameObject[] all = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (GameObject go in all)
            {
                if (go.name == "DialoguePanel" && go.hideFlags == HideFlags.None && go.scene.isLoaded)
                {
                    dialoguePanel = go;
                    break;
                }
            }
        }

        // Dentro del panel, localizamos los textos de nombre y de línea por nombre de hijo.
        if (dialoguePanel != null)
        {
            if (nameText == null)
            {
                Transform t = dialoguePanel.transform.Find("NameText");
                if (t != null) nameText = t.GetComponent<TextMeshProUGUI>();
            }
            if (dialogueText == null)
            {
                Transform t = dialoguePanel.transform.Find("DialogueText");
                if (t != null) dialogueText = t.GetComponent<TextMeshProUGUI>();
            }
        }

        // Si no hay cartel propio, reutilizamos el "CheckpointPrompt" como cartel "E hablar".
        if (promptUI == null)
        {
            GameObject[] all = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (GameObject go in all)
            {
                if (go.name == "CheckpointPrompt" && go.hideFlags == HideFlags.None && go.scene.isLoaded)
                {
                    promptUI = go;
                    break;
                }
            }
        }

        // Guardamos las piezas del cartel ahora (así no hay que buscarlas otra vez)
        // para poder moverlo y cambiar su texto más tarde.
        if (promptUI != null)
        {
            promptText = promptUI.GetComponentInChildren<TextMeshProUGUI>(true);
            promptRect = promptUI.GetComponent<RectTransform>();
            promptCanvas = promptUI.GetComponentInParent<Canvas>();
        }
    }

    private void Update()
    {
        // Leemos el teclado del New Input System; si no hay, salimos.
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        // La tecla E inicia el diálogo (si el jugador está cerca) o avanza a la siguiente línea.
        if (keyboard.eKey.wasPressedThisFrame)
        {
            if (!isTalking && playerInRange)
            {
                StartDialogue();
            }
            else if (isTalking)
            {
                AdvanceDialogue();
            }
        }
    }

    private void LateUpdate()
    {
        // Mantenemos el cartel sobre el NPC solo si está cerca y no se está dialogando.
        if (playerInRange && !isTalking && promptUI != null && promptUI.activeInHierarchy)
        {
            UpdatePromptPosition();
        }
    }

    /// <summary>
    /// Inicia el diálogo: oculta el cartel, muestra el panel con el nombre y la
    /// primera línea, y bloquea el movimiento del jugador mientras habla.
    /// </summary>
    private void StartDialogue()
    {
        // Sin líneas no hay nada que mostrar.
        if (dialogueLines == null || dialogueLines.Length == 0) return;

        isTalking = true;
        currentLine = 0;

        // Ocultamos el cartel "E hablar" mientras se dialoga.
        if (promptUI != null) promptUI.SetActive(false);

        // Mostramos el panel y el nombre del NPC.
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        if (nameText != null) nameText.text = npcName;

        ShowCurrentLine();

        // Bloqueamos el movimiento del jugador mientras dura el diálogo.
        if (PlayerController.instance != null) PlayerController.instance.canMove = false;
    }

    /// <summary>
    /// Pasa a la siguiente frase; si ya no quedan más frases, termina la conversación.
    /// </summary>
    private void AdvanceDialogue()
    {
        currentLine++;
        if (currentLine >= dialogueLines.Length)
        {
            EndDialogue();
        }
        else
        {
            ShowCurrentLine();
        }
    }

    /// <summary>
    /// Vuelca la línea de diálogo actual en el texto del panel.
    /// </summary>
    private void ShowCurrentLine()
    {
        if (dialogueText != null)
        {
            dialogueText.text = dialogueLines[currentLine];
        }
    }

    /// <summary>
    /// Finaliza el diálogo: oculta el panel, devuelve el control al jugador y
    /// vuelve a mostrar el cartel "E hablar" si el jugador sigue cerca.
    /// </summary>
    private void EndDialogue()
    {
        isTalking = false;
        if (dialoguePanel != null) dialoguePanel.SetActive(false);

        // Devolvemos el movimiento al jugador.
        if (PlayerController.instance != null) PlayerController.instance.canMove = true;

        // Si el jugador sigue cerca, mostramos de nuevo el cartel.
        if (playerInRange && promptUI != null)
        {
            ShowPrompt();
        }
    }

    /// <summary>
    /// Muestra el cartel "E hablar" con su texto y lo posiciona sobre el NPC.
    /// </summary>
    private void ShowPrompt()
    {
        if (promptUI == null) return;

        if (promptText != null)
        {
            promptText.text = "E hablar";
            promptText.color = Color.white;
            promptText.alpha = 1f;
        }
        promptUI.SetActive(true);
        UpdatePromptPosition();
    }

    /// <summary>
    /// Coloca el cartel justo encima del NPC. Para ello pasa la posición del NPC en el
    /// mundo del juego a la posición que le corresponde en la pantalla.
    /// </summary>
    private void UpdatePromptPosition()
    {
        Camera cam = Camera.main;
        if (cam == null || promptRect == null || promptCanvas == null) return;

        // Calculamos en qué punto de la pantalla cae el NPC (un poco más arriba, por el offset).
        Vector3 screenPos = cam.WorldToScreenPoint(transform.position + promptWorldOffset);
        // Si el NPC queda detrás de la cámara, escondemos el cartel.
        if (screenPos.z < 0)
        {
            promptUI.SetActive(false);
            return;
        }

        // Hay dos formas de dibujar la interfaz; aquí tratamos cada una de manera distinta.
        // Forma 1: la interfaz se dibuja con la cámara -> convertimos el punto al sistema de la pizarra.
        if (promptCanvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            RectTransform canvasRect = promptCanvas.GetComponent<RectTransform>();
            Vector2 localPos;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, promptCanvas.worldCamera, out localPos))
            {
                promptRect.anchoredPosition = localPos;
            }
        }
        // Forma 2: la interfaz se dibuja encima de todo -> usamos el punto de pantalla tal cual.
        else if (promptCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            promptRect.position = screenPos;
        }
    }

    // Se ejecuta cuando algo entra en la zona del NPC.
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Si quien entra es el jugador, mostramos el cartel (salvo que ya estemos hablando).
        if (other.CompareTag("Player") || (other.attachedRigidbody != null && other.attachedRigidbody.CompareTag("Player")))
        {
            if (dialoguePanel == null) AutoFindReferences();
            playerInRange = true;
            if (!isTalking) ShowPrompt();
        }
    }

    // Se ejecuta cuando algo sale de la zona del NPC.
    private void OnTriggerExit2D(Collider2D other)
    {
        // Si el jugador se va, escondemos el cartel y cortamos la conversación si la había.
        if (other.CompareTag("Player") || (other.attachedRigidbody != null && other.attachedRigidbody.CompareTag("Player")))
        {
            playerInRange = false;
            if (promptUI != null) promptUI.SetActive(false);
            if (isTalking) EndDialogue();
        }
    }
}
