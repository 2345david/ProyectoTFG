using UnityEngine;
using Metroidvania.Core;
using UnityEngine.InputSystem;
using TMPro;

namespace Metroidvania.Gameplay
{
    /// <summary>
    /// Es un punto donde el jugador puede guardar la partida. Cuando el jugador se acerca,
    /// aparece un cartel que dice "E guardar". Si pulsa la tecla E, se guarda el juego
    /// y el cartel cambia a "guardado". El cartel sigue al punto en la pantalla.
    /// </summary>
    public class SavePoint : MonoBehaviour
    {
        public string savePointName;          // Nombre de este punto de guardado.
        public GameObject promptUI;           // El cartel que aparece en pantalla.
        private TextMeshProUGUI promptText;   // El texto del cartel ("E guardar" / "guardado").
        private RectTransform promptRect;      // Datos de posición del cartel en la pantalla.
        private Canvas parentCanvas;           // El "lienzo" (Canvas) donde se dibuja el cartel.
        private bool playerInRange = false;    // ¿El jugador está cerca? true = sí.
        private bool isSaved = false;          // ¿Ya se guardó aquí? Evita guardar muchas veces seguidas.

        [Header("Positioning")]
        public Vector3 worldOffset = new Vector3(0, 1.2f, 0); // Cuánto subimos el cartel sobre el punto.

        // Se ejecuta al empezar. Busca el cartel en la escena.
        private void Start()
        {
            FindPrompt();
        }

        // Busca el cartel (si no se puso a mano) y guarda sus partes (texto, posición y lienzo)
        // para poder usarlas rápido más tarde.
        private void FindPrompt()
        {
            // Si nadie asignó el cartel, lo buscamos en la escena por su nombre ("CheckpointPrompt").
            if (promptUI == null)
            {
                // Buscamos en TODOS los objetos, incluso los que están apagados/escondidos.
                GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
                foreach (GameObject go in allObjects)
                {
                    // Nos quedamos con el que se llama "CheckpointPrompt" y está dentro de la escena actual.
                    if (go.name == "CheckpointPrompt" && go.hideFlags == HideFlags.None && go.scene.isLoaded)
                    {
                        promptUI = go;
                        break;
                    }
                }
            }

            // Si encontramos el cartel, guardamos sus partes y lo dejamos con buen aspecto.
            if (promptUI != null)
            {
                promptText = promptUI.GetComponentInChildren<TextMeshProUGUI>(true);
                promptRect = promptUI.GetComponent<RectTransform>();
                parentCanvas = promptUI.GetComponentInParent<Canvas>();
                promptUI.transform.localScale = Vector3.one; // Tamaño normal (escala 1).
                if (promptText != null) promptText.color = Color.white;
            }
        }

        // Se ejecuta en cada momento del juego. Si el jugador está cerca y aún no guardó,
        // mira si pulsa la tecla E para guardar.
        private void Update()
        {
            // Solo dejamos guardar si el jugador está cerca y no ha guardado todavía.
            if (playerInRange && !isSaved)
            {
                // Comprobamos si se acaba de pulsar la tecla E.
                if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
                {
                    Save();
                }
            }
        }

        // Se ejecuta justo después de dibujar la pantalla. Mantiene el cartel pegado
        // encima del punto de guardado mientras el jugador está cerca.
        private void LateUpdate()
        {
            // Solo movemos el cartel si está visible y el jugador sigue cerca.
            if (playerInRange && promptUI != null && promptUI.activeInHierarchy)
            {
                UpdatePromptPosition();
            }
        }

        // Calcula en qué punto de la pantalla está el punto de guardado y coloca
        // el cartel ahí. Tiene en cuenta el tipo de lienzo (Canvas) que se usa.
        private void UpdatePromptPosition()
        {
            // Si no hay cámara principal, no podemos calcular dónde poner el cartel.
            Camera cam = Camera.main;
            if (cam == null) return;

            // Pasamos la posición del punto (en el mundo del juego) a un punto de la pantalla.
            Vector3 screenPos = cam.WorldToScreenPoint(transform.position + worldOffset);

            // Si el punto queda detrás de la cámara, escondemos el cartel.
            if (screenPos.z < 0)
            {
                promptUI.SetActive(false);
                return;
            }

            if (promptRect != null && parentCanvas != null)
            {
                // Si el lienzo está en modo "cámara", convertimos el punto de pantalla
                // a una posición que el lienzo entienda.
                if (parentCanvas.renderMode == RenderMode.ScreenSpaceCamera)
                {
                    Vector2 localPos;
                    RectTransform canvasRect = parentCanvas.GetComponent<RectTransform>();
                    if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, parentCanvas.worldCamera, out localPos))
                    {
                        promptRect.anchoredPosition = localPos;
                    }
                }
                // Si el lienzo está pegado a la pantalla (modo "overlay"),
                // usamos el punto de pantalla directamente.
                else if (parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    promptRect.position = screenPos;
                }
            }
        }

        // Se ejecuta cuando el jugador se acerca. Muestra el cartel "E guardar"
        // y lo coloca en su sitio en la pantalla.
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                // Por si acaso aún no tenemos el cartel, lo buscamos ahora.
                if (promptUI == null) FindPrompt();

                playerInRange = true;
                isSaved = false; // Al volver a entrar, permitimos guardar otra vez.
                if (promptUI != null)
                {
                    // Preparamos el texto del cartel para invitar a guardar con la tecla E.
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

        // Se ejecuta cuando el jugador se aleja. Esconde el cartel.
        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                playerInRange = false;
                if (promptUI != null) promptUI.SetActive(false);
            }
        }

        // Guarda la partida de verdad: guarda el progreso del jugador y apunta este punto
        // como sitio donde reaparecer. Luego cambia el cartel a "guardado".
        public void Save()
        {
            Debug.Log($"[SavePoint] Saving at {savePointName}");
            // Guarda el progreso general (habilidades, objetos, etc.).
            ProgressionManager.Instance.SaveGame();

            // Apunta este sitio como punto de reaparición, si existe ese gestor.
            if (CheckpointManager.instance != null)
            {
                CheckpointManager.instance.SaveCheckpoint(transform.position);
            }

            // Marcamos que ya se guardó y cambiamos el cartel a "guardado".
            isSaved = true;
            if (promptText != null) promptText.text = "guardado";
        }
    }
}
