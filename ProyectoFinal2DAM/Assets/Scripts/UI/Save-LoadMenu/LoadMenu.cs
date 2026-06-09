using UI.Elements;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Menú de ranuras de guardado. Al pulsar una ranura aparece un panel de confirmación
/// con dos botones: "Cargar partida" y "Nueva partida".
/// - Cargar partida: restaura los datos guardados en esa ranura (solo si los hay).
/// - Nueva partida: empieza una partida nueva EN esa ranura (sobrescribe lo guardado).
/// En ambos casos se arranca el juego por la PersistentScene, y los checkpoints
/// guardarán en la ranura elegida (no hay autoguardado).
/// </summary>
public class LoadMenu : MonoBehaviour
{
    public Button[] slotButtons;
    public Button[] deleteButtons;

    [Header("Panel de confirmación (Cargar / Nueva partida)")]
    [Tooltip("Panel emergente que se muestra al pulsar una ranura.")]
    public GameObject confirmPanel;
    [Tooltip("Botón 'Cargar partida' dentro del panel de confirmación.")]
    public Button confirmLoadButton;
    [Tooltip("Botón 'Nueva partida' dentro del panel de confirmación.")]
    public Button confirmNewGameButton;
    [Tooltip("Botón 'Cancelar' dentro del panel de confirmación (opcional).")]
    public Button confirmCancelButton;
    [Tooltip("Texto de título del panel de confirmación (opcional).")]
    public TextMeshProUGUI confirmTitle;

    [Tooltip("Escena raíz del juego (contiene WorldManager y CheckpointManager).")]
    public string PersistentSceneName = "PersistentScene";

    // Ranura seleccionada actualmente en el panel de confirmación.
    int _selectedSlot = -1;

    void Start()
    {
        // El panel de confirmación arranca oculto.
        if (confirmPanel != null)
            confirmPanel.SetActive(false);

        // Cableamos los botones del panel una sola vez.
        if (confirmLoadButton != null)
        {
            confirmLoadButton.onClick.RemoveAllListeners();
            confirmLoadButton.onClick.AddListener(ConfirmLoad);
        }
        if (confirmNewGameButton != null)
        {
            confirmNewGameButton.onClick.RemoveAllListeners();
            confirmNewGameButton.onClick.AddListener(ConfirmNewGame);
        }
        if (confirmCancelButton != null)
        {
            confirmCancelButton.onClick.RemoveAllListeners();
            confirmCancelButton.onClick.AddListener(CloseConfirmPanel);
        }

        RefreshSlots();
    }

    void RefreshSlots()
    {
        for (int i = 0; i < slotButtons.Length; i++)
        {
            int slotIndex = i;
            SaveData data = SaveSystem.LoadGame(slotIndex);

            if (data != null)
            {
                UIElementUtils.SetButtonText(slotButtons[i],
                    "Ranura " + (i + 1) + "\n" + data.saveTime);

                if (deleteButtons != null && i < deleteButtons.Length)
                    deleteButtons[i].gameObject.SetActive(true);
            }
            else
            {
                UIElementUtils.SetButtonText(slotButtons[i],
                    "Ranura " + (i + 1) + "\n(vacia - nueva partida)");

                if (deleteButtons != null && i < deleteButtons.Length)
                    deleteButtons[i].gameObject.SetActive(false);
            }

            slotButtons[i].onClick.RemoveAllListeners();
            slotButtons[i].onClick.AddListener(() => SelectSlot(slotIndex));

            if (deleteButtons != null && i < deleteButtons.Length)
            {
                deleteButtons[i].onClick.RemoveAllListeners();
                deleteButtons[i].onClick.AddListener(() => DeleteSlot(slotIndex));
            }
        }
    }

    void SelectSlot(int slot)
    {
        _selectedSlot = slot;

        // Si no hay panel asignado, mantenemos el comportamiento directo (fallback).
        if (confirmPanel == null)
        {
            if (SaveSystem.LoadGame(slot) != null)
                LoadSlot(slot);
            else
                NewGameInSlot(slot);
            return;
        }

        bool hasData = SaveSystem.LoadGame(slot) != null;

        // Título informativo.
        if (confirmTitle != null)
            confirmTitle.text = "Ranura " + (slot + 1);

        // "Cargar partida" solo tiene sentido si la ranura tiene datos.
        if (confirmLoadButton != null)
            confirmLoadButton.interactable = hasData;

        confirmPanel.SetActive(true);
        confirmPanel.transform.SetAsLastSibling(); // Se dibuja por encima del resto.
    }

    /// <summary>Botón "Cargar partida" del panel de confirmación.</summary>
    public void ConfirmLoad()
    {
        if (_selectedSlot < 0) return;
        if (SaveSystem.LoadGame(_selectedSlot) == null)
        {
            Debug.LogWarning("No hay datos que cargar en la ranura " + (_selectedSlot + 1));
            return;
        }
        LoadSlot(_selectedSlot);
    }

    /// <summary>Botón "Nueva partida" del panel de confirmación.</summary>
    public void ConfirmNewGame()
    {
        if (_selectedSlot < 0) return;
        // Empezar partida nueva sobrescribe lo guardado: borramos la ranura para
        // que no se cargue progreso antiguo al entrar.
        SaveSystem.DeleteSave(_selectedSlot);
        NewGameInSlot(_selectedSlot);
    }

    /// <summary>Cierra el panel de confirmación sin hacer nada.</summary>
    public void CloseConfirmPanel()
    {
        _selectedSlot = -1;
        if (confirmPanel != null)
            confirmPanel.SetActive(false);
    }

    void LoadSlot(int slot)
    {
        // El CheckpointManager leerá esta ranura y el WorldManager viajará al bioma
        // guardado restaurando nuestro SaveData enriquecido.
        PlayerPrefs.SetInt("LoadSlot", slot);
        PlayerPrefs.Save();
        SceneManager.LoadScene(PersistentSceneName, LoadSceneMode.Single);
    }

    // Empieza una partida nueva en esa ranura y entra al juego.
    void NewGameInSlot(int slot)
    {
        // Partida nueva: el progreso se escribirá en esta ranura al pasar por un
        // checkpoint. No se guarda nada todavía (sin autoguardado).
        PlayerPrefs.SetInt("LoadSlot", slot);
        PlayerPrefs.Save();
        Debug.Log("Nueva partida en ranura " + (slot + 1));
        SceneManager.LoadScene(PersistentSceneName, LoadSceneMode.Single);
    }

    void DeleteSlot(int slot)
    {
        if (SaveSystem.DeleteSave(slot))
            RefreshSlots();
    }

    // Boton "Volver": regresa al menu principal.
    public void BackToPreviousMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }
}