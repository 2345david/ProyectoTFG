using UI.Navigation;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public GameObject MainMenu;
    public GameObject LoadMenu;
    public GameObject SettingsMenu;

    [Tooltip("Escena con las ranuras de guardado.")]
    public string LoadSaveSceneName = "LoadSaveMenu";

    public void PlayGame()
    {
        // Vamos directamente a la pantalla de ranuras (sin paso intermedio
        // "NEW GAME / LOAD GAME"). Allí: ranura vacía = partida nueva,
        // ranura con datos = cargar partida.
        SceneManager.LoadScene(LoadSaveSceneName, LoadSceneMode.Single);
    }

    // Boton "Ajustes": muestra la pantalla de ajustes y oculta el menu principal.
    public void OpenSettings()
    {
        UINavigator.SwitchMenu(SettingsMenu, MainMenu);
    }

    // Boton "Salir": cierra el juego (solo funciona en el juego ya compilado).
    public void QuitGame()
    {
        Application.Quit();
    }
}