namespace settings
{
    using UnityEngine;
    using UnityEngine.UI;

    using UI.Navigation;
    using settings.audio.audioScript;

    public class Settings : MonoBehaviour
    {
        // Panels
        public GameObject PreviousMenu;
        public GameObject SettingsMenu;

        public GameObject GameButtonPanel;
        public GameObject AuidoButtonPanel;
        // Buttons
        public GameObject ReturnButton;
        public GameObject Settings_GameButton;
        public GameObject Settings_AuidoButton;
        // Toggle
        public Toggle FullScreenToggle;
        // Referencias para restablecer valores
        public AudioScript AudioScript;
        public ResolutionScript ResolutionScript;

        // Valores por defecto
        const bool DefaultFullScreen = true;
        const float DefaultVolume = 1f;

        private void Start()
        {
            GameButtonPanel.SetActive(true);
            AuidoButtonPanel.SetActive(false);

            FullScreenToggle.isOn = Screen.fullScreen;
        }
        public void OpenGameSettings()
        {
            //Settings_GameButton.SetActive(true);
            GameButtonPanel.SetActive(true);
            AuidoButtonPanel.SetActive(false);
        }
        public void OpenAudioSettings()
        {
            AuidoButtonPanel.SetActive(true);
            GameButtonPanel.SetActive(false);
        }

        public void OpenPreviousMenu()
        {
            UINavigator.SwitchMenu(PreviousMenu, SettingsMenu);
        }

        public void SetFullScreen(bool isFullScreen)
        {
            Screen.fullScreen = isFullScreen;
        }

        public void ResetToDefaults()
        {
            // Restablecemos pantalla completa
            FullScreenToggle.isOn = DefaultFullScreen;
            Screen.fullScreen = DefaultFullScreen;

            // Restablecemos volumen
            PlayerPrefs.SetFloat("MasterVolume", DefaultVolume);
            PlayerPrefs.DeleteKey("IsMuted");
            AudioScript.ResetToDefault();

            // Restablecemos resolución a la más alta disponible
            ResolutionScript.ResetToDefault();

            // Guardamos los cambios
            PlayerPrefs.Save();

            Debug.Log("Ajustes restablecidos por defecto");
        }
    }
}