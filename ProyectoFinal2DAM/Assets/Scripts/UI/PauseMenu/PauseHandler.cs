using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseHandler : MonoBehaviour
{
    public static PauseHandler Instance { get; private set; }

    public string PauseMenuSceneName = "PauseMenu";

    bool _isPaused = false;
    bool _isLoading = false;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (_isLoading) return;

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (_isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    // Pausa el juego: congela el tiempo, guarda la partida y abre el menu de pausa.
    public void PauseGame()
    {
        if (SceneManager.GetSceneByName(PauseMenuSceneName).isLoaded) return;

        _isPaused = true;
        _isLoading = true;
        Time.timeScale = 0f;

        // Guardamos autom�ticamente al pausar
        SaveData data = AutoSave.CollectSaveData();
        SaveSystem.SaveGame(data, SaveSystem.AutoSaveSlot);

        var op = SceneManager.LoadSceneAsync(PauseMenuSceneName, LoadSceneMode.Additive);
        op.completed += _ => _isLoading = false;
    }

    public void ResumeGame()
    {
        _isPaused = false;
        Time.timeScale = 1f;

        Scene pauseScene = SceneManager.GetSceneByName(PauseMenuSceneName);
        if (pauseScene.isLoaded)
            SceneManager.UnloadSceneAsync(pauseScene);
    }

    // Reinicia el estado de pausa de golpe (sin abrir ni cerrar menus). Util al cambiar de escena.
    public void ForceReset()
    {
        _isPaused = false;
        _isLoading = false;
        Time.timeScale = 1f;
    }
}