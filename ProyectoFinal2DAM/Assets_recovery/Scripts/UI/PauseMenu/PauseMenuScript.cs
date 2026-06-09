using UnityEngine;
using UnityEngine.SceneManagement;

using UI.Navigation;

public class PauseMenuScript : MonoBehaviour
{
    public GameObject Menu;
    public GameObject SettingsMenu;

    // Nombres de las escenas para facilitar la gestión
    public string MainMenuSceneName = "MainMenu";
    public string GameSceneName = "Game";

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
    }

    public void UnloadScene(string sceneName)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (scene.isLoaded)
        {
            SceneManager.UnloadSceneAsync(scene);
        }
    }

    public void SetActiveScene(string sceneName)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (scene.isLoaded)
        {
            SceneManager.SetActiveScene(scene);
        }
    }

    // Salir al menú principal, cerrando la escena del juego y la escena de pausa
    public void QuitToMainMenu()
    {
        Scene gameScene = SceneManager.GetSceneByName(GameSceneName);
        // Primero cerramos la escena del juego
        if (gameScene.isLoaded)
        {
            SceneManager.UnloadSceneAsync(gameScene);
        }

        // Luego cerramos la escena de pausa
        Scene pauseScene = gameObject.scene;
        if (pauseScene.isLoaded)
        {
            SceneManager.UnloadSceneAsync(pauseScene);
        }

        // Finalmente, cargamos el menú principal
        SceneManager.LoadScene(MainMenuSceneName, LoadSceneMode.Single);
    }

    public void OpenSettings()
    {
        UINavigator.SwitchMenu(Menu, SettingsMenu);
    }
}
