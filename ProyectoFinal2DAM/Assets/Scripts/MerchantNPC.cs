using UnityEngine;
using UnityEngine.InputSystem; // New Input System: lectura de Keyboard

/// <summary>
/// NPC mercader.Detecta cuándo el jugador está cerca, muestra un prompt
/// ("tienda") y permite abrir/cerrar la interfaz de la tienda con la tecla T.
/// Mientras la tienda está abierta, el juego se pausa (timeScale = 0).
/// </summary>
public class MerchantNPC : MonoBehaviour
{
    public GameObject promptUI; // Cartel "tienda" que aparece cuando el jugador está cerca
    public ShopUI shopUI;       // Referencia al panel de la tienda
    private bool playerInRange = false; // ¿Está el jugador dentro del rango del mercader?

    void Start()
    {
        // Localizamos las referencias de UI si no se asignaron en el Inspector.
        FindReferences();

        // Estado inicial: prompt y tienda ocultos.
        if (promptUI != null) promptUI.SetActive(false);
        if (shopUI != null) shopUI.gameObject.SetActive(false);
    }

    /// <summary>
    /// Busca automáticamente las referencias del panel de la tienda y del prompt
    /// recorriendo las escenas cargadas (útil cuando la UI vive en otra escena).
    /// </summary>
    private void FindReferences()
    {
        if (shopUI == null || promptUI == null)
        {
            // Recorremos todas las escenas cargadas.
            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
            {
                var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
                if (!scene.isLoaded) continue;

                // Recorremos los objetos raíz de cada escena.
                foreach (var root in scene.GetRootGameObjects())
                {
                    // Caso normal: la UI cuelga del canvas "UI_Canvas".
                    if (root.name == "UI_Canvas")
                    {
                        if (shopUI == null)
                        {
                            Transform t = root.transform.Find("ShopUI_Panel");
                            if (t != null) shopUI = t.GetComponent<ShopUI>();
                        }

                        if (promptUI == null)
                        {
                            // Se reutiliza el "CheckpointPrompt" como cartel de la tienda.
                            Transform t = root.transform.Find("CheckpointPrompt");
                            if (t != null) promptUI = t.gameObject;
                        }
                    }
                    
                    // Caso alternativo: el panel de tienda es directamente un objeto raíz.
                    if (shopUI == null && root.name == "ShopUI_Panel")
                    {
                        shopUI = root.GetComponent<ShopUI>();
                    }
                }
            }
        }
    }

    void Update()
    {
        // Solo escuchamos la tecla T cuando el jugador está dentro del rango.
        if (playerInRange)
        {
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.tKey.wasPressedThisFrame)
            {
                Debug.Log("[MerchantNPC] 'T' pressed.");
                ToggleShop();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Comprobamos si quien entra es el jugador (por tag directo o por su Rigidbody).
        bool isPlayer = other.CompareTag("Player") || (other.attachedRigidbody != null && other.attachedRigidbody.CompareTag("Player"));
        
        if (isPlayer)
        {
            playerInRange = true;
            if (promptUI != null) 
            {
                // Mostramos el cartel "tienda".
                promptUI.SetActive(true);
                // Forzamos la escala X a positiva para que el texto no salga "espejado"
                // si el sprite del mercader se voltea.
                Vector3 localScale = promptUI.transform.localScale;
                localScale.x = Mathf.Abs(localScale.x);
                promptUI.transform.localScale = localScale;
            }
            Debug.Log("[MerchantNPC] Player entered range via trigger.");
        }
    }

    // Se ejecuta cuando algo sale de la zona del vendedor.
    private void OnTriggerExit2D(Collider2D other)
    {
        bool isPlayer = other.CompareTag("Player") || (other.attachedRigidbody != null && other.attachedRigidbody.CompareTag("Player"));
        
        if (isPlayer)
        {
            playerInRange = false;
            // Ocultamos el cartel al alejarse.
            if (promptUI != null) promptUI.SetActive(false);
            
            // Si el jugador se va con la tienda abierta, la cerramos y reanudamos el tiempo.
            if (shopUI != null && shopUI.gameObject.activeSelf) 
            {
                shopUI.gameObject.SetActive(false);
                Time.timeScale = 1f;
            }
            Debug.Log("[MerchantNPC] Player left range via trigger.");
        }
    }

    /// <summary>
    /// Abre o cierra la tienda. Al abrirla oculta el prompt "tienda" y pausa el
    /// juego; al cerrarla restaura el prompt (si el jugador sigue cerca) y reanuda el tiempo.
    /// </summary>
    void ToggleShop()
    {
        // Si perdimos la referencia, intentamos recuperarla.
        if (shopUI == null) FindReferences();

        if (shopUI != null)
        {
            // Invertimos el estado actual del panel de la tienda.
            bool isActive = !shopUI.gameObject.activeSelf;
            shopUI.gameObject.SetActive(isActive);
            Debug.Log($"[MerchantNPC] Shop Panel toggled to: {isActive}");
            
            if (isActive)
            {
                // Al abrir: ocultamos el cartel "tienda" para que no estorbe.
                if (promptUI != null) promptUI.SetActive(false);

                // Ponemos la tienda delante de todo lo demás y pausamos el juego (paramos el tiempo).
                shopUI.transform.SetAsLastSibling();
                Time.timeScale = 0f;
            }
            else
            {
                // Al cerrar: restauramos el cartel solo si el jugador sigue cerca.
                if (promptUI != null && playerInRange) promptUI.SetActive(true);

                // Reanudamos el tiempo del juego.
                Time.timeScale = 1f;
            }
        }
        else
        {
            Debug.LogError("[MerchantNPC] Shop UI reference is missing!");
        }
    }
}
