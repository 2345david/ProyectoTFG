using audio.MusicManager;
using UI.Elements;
using UI.Navigation;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadMenu : MonoBehaviour
{
    public GameObject MainMenu;
    public GameObject LoadMenu_;

    public Button[] slotButtons;
    public Button[] deleteButtons;

    void Start()
    {
        RefreshSlots();
    }

    // Método separado para poder refrescar los botones después de borrar
    void RefreshSlots()
    {
        for (int i = 0; i < slotButtons.Length; i++)
        {
            int slotIndex = i;
            SaveData data = SaveSystem.LoadGame(slotIndex);

            if (data != null)
            {
                UIElementUtils.SetButtonText(slotButtons[i],
                    "Slot " + (i + 1) + "\n" + data.saveTime);

                // Mostramos el botón de borrado solo si hay datos
                if (deleteButtons != null && i < deleteButtons.Length)
                    deleteButtons[i].gameObject.SetActive(true);
            }
            else
            {
                UIElementUtils.SetButtonText(slotButtons[i], "Ranura vacía");

                // Ocultamos el botón de borrado si la ranura está vacía
                if (deleteButtons != null && i < deleteButtons.Length)
                    deleteButtons[i].gameObject.SetActive(false);
            }

            slotButtons[i].onClick.RemoveAllListeners();
            slotButtons[i].onClick.AddListener(() => LoadSlot(slotIndex));

            if (deleteButtons != null && i < deleteButtons.Length)
            {
                deleteButtons[i].onClick.RemoveAllListeners();
                deleteButtons[i].onClick.AddListener(() => DeleteSlot(slotIndex));
            }
        }
    }

    public void LoadSlot(int slot)
    {
        SaveData data = SaveSystem.LoadGame(slot);

        if (data == null)
        {
            Debug.LogError("No hay datos en la ranura " + slot);
            return;
        }

        Debug.Log("Cargando ranura " + slot);
        PlayerPrefs.SetInt("LoadSlot", slot);
        MusicManager.Instance.PlayGameMusic();
        SceneManager.LoadScene(data.sceneName);
    }

    public void DeleteSlot(int slot)
    {
        if (SaveSystem.DeleteSave(slot))
            RefreshSlots(); // Actualizamos los botones tras borrar
    }

    public void BackToPreviousMenu()
    {
        UINavigator.SwitchMenu(MainMenu, LoadMenu_);
    }
}