using UnityEngine;
using UnityEngine.SceneManagement;
using UI.Navigation;

public class PauseMenuScript : MonoBehaviour
{
    public GameObject Menu;
    public GameObject SettingsMenu;

    public string MainMenuSceneName = "MainMenu";
    public string GameSceneName = "Game";

    public void QuickSave()
    {
        SaveData data = AutoSave.CollectSaveData();
        SaveSystem.SaveGame(data, SaveSystem.AutoSaveSlot);
        Debug.Log("Guardado rápido completado");
    }

    // Boton "Salir al menu": limpia los objetos del juego y vuelve al menu principal.
    public void QuitToMainMenu()
    {
        Time.timeScale = 1f;

        // WorldManager y CheckpointManager son DontDestroyOnLoad: hay que destruirlos
        // explícitamente para no arrastrarlos (con estado obsoleto) al menú principal.
        // Si no, al volver a entrar al juego sus Awake destruirían las instancias
        // nuevas y conservarían las viejas.
        if (CheckpointManager.instance != null)
            Destroy(CheckpointManager.instance.gameObject);
        if (Metroidvania.Core.WorldManager.Instance != null)
            Destroy(Metroidvania.Core.WorldManager.Instance.gameObject);

        // LoadScene en modo Single descarga el resto de escenas (juego + pausa).
        SceneManager.LoadScene(MainMenuSceneName, LoadSceneMode.Single);
    }

    // Boton "Ajustes": muestra el panel de ajustes y oculta el menu de pausa.
    public void OpenSettings()
    {
        UINavigator.SwitchMenu(SettingsMenu, Menu);
    }

    // Boton "Continuar": quita la pausa y vuelve al juego.
    public void ResumeGame()
    {
        Time.timeScale = 1f;
        PauseHandler.Instance.ResumeGame();
    }
}