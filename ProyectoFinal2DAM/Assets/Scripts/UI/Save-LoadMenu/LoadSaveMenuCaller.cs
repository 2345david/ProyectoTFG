using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSaveMenuCaller : MonoBehaviour
{
    public string LoadSaveSceneName = "LoadSaveMenu";

    /*
    public void OpenForLoading()
    {
        PlayerPrefs.SetString("LoadSaveMode", "Load");
        PlayerPrefs.SetString("PreviousScene",
            SceneManager.GetActiveScene().name);

        // Descargamos MainMenu antes de cargar LoadSaveMenu
        Scene mainMenuScene = SceneManager.GetSceneByName("MainMenu");
        if (mainMenuScene.isLoaded)
            SceneManager.UnloadSceneAsync(mainMenuScene);

        SceneManager.LoadSceneAsync("LoadSaveMenu", LoadSceneMode.Additive);
    }

    public void OpenForSaving()
    {
        if (PauseHandler.Instance != null)
            PauseHandler.Instance.ForceReset();

        // Guardamos datos antes de abrir el men�
        SaveData data = AutoSave.CollectSaveData();
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString("PendingSaveData", json);

        PlayerPrefs.SetString("LoadSaveMode", "Save");
        PlayerPrefs.SetString("PreviousScene",
            SceneManager.GetActiveScene().name);

        Time.timeScale = 1f;

        // Descargamos PauseMenu antes de cargar LoadSaveMenu
        Scene pauseScene = SceneManager.GetSceneByName("PauseMenu");
        if (pauseScene.isLoaded)
            SceneManager.UnloadSceneAsync(pauseScene);

        SceneManager.LoadSceneAsync("LoadSaveMenu", LoadSceneMode.Additive);
    }
    */
    // Abre la pantalla de ranuras en modo "Cargar" partida guardada.
    public void OpenForLoading()
    {
        PlayerPrefs.SetString("LoadSaveMode", "Load");
        PlayerPrefs.SetString("PreviousScene",
            SceneManager.GetActiveScene().name);

        SceneManager.LoadScene(LoadSaveSceneName, LoadSceneMode.Single);
    }

    // Abre la pantalla de ranuras en modo "Guardar": antes prepara los datos a guardar.
    public void OpenForSaving()
    {
        if (PauseHandler.Instance != null)
            PauseHandler.Instance.ForceReset();

        // Guardamos datos del jugador antes de salir de la escena
        SaveData data = AutoSave.CollectSaveData();
        PlayerPrefs.SetString("PendingSaveData", JsonUtility.ToJson(data));
        PlayerPrefs.SetString("LoadSaveMode", "Save");
        // Guardamos nombre de escena del juego expl�citamente
        PlayerPrefs.SetString("PreviousScene",
            SceneManager.GetActiveScene().name);

        Time.timeScale = 1f;
        SceneManager.LoadScene(LoadSaveSceneName, LoadSceneMode.Single);
    }
}