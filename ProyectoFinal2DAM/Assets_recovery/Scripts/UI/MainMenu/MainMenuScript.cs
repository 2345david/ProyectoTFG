using UI.Navigation;
using UnityEngine;

using audio.MusicManager;

public class UIManager : MonoBehaviour
{
    public GameObject MainMenu;
    public GameObject LoadMenu;
    public GameObject SettingsMenu;
    public void PlayGame()
    {
        UINavigator.SwitchMenu(LoadMenu, MainMenu);
        MusicManager.Instance.PlayMenuMusic();
    }

    public void OpenSettings()
    {
        UINavigator.SwitchMenu(SettingsMenu, MainMenu);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}