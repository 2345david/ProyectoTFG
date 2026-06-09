namespace settings
{
    using settings.audio.audioScript;
    using UI.Navigation;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Controla el menu de Ajustes (opciones del juego).
    /// Permite cambiar entre las pestanas de Juego y Audio, activar la pantalla
    /// completa y volver a dejar todo como estaba al principio.
    /// </summary>
    public class Settings : MonoBehaviour
    {
        // Las distintas pantallas (paneles) que podemos mostrar u ocultar.
        public GameObject PreviousMenu;     // Menu al que volvemos al salir de Ajustes.
        public GameObject SettingsMenu;     // El propio menu de Ajustes.
        public GameObject GameButtonPanel;  // Pestana de opciones de Juego.
        public GameObject AudioButtonPanel; // Pestana de opciones de Audio.

        // Interruptor (casilla) para activar o desactivar la pantalla completa.
        public Toggle FullScreenToggle;

        // Valores que usamos cuando reiniciamos los ajustes a su estado original.
        const bool DefaultFullScreen = true; // Por defecto, pantalla completa encendida.
        const float DefaultVolume = 1f;      // Por defecto, volumen al maximo.

        // Componentes en el mismo objeto � no necesitan asignarse en Inspector
        AudioScript _audioScript;
        ResolutionScript _resolutionScript;

        // Se ejecuta al abrir el menu: prepara los componentes y muestra la pestana de Juego.
        void Start()
        {
            // Obtenemos los componentes del mismo objeto autom�ticamente
            _audioScript = GetComponent<AudioScript>();
            _resolutionScript = GetComponent<ResolutionScript>();

            GameButtonPanel.SetActive(true);
            AudioButtonPanel.SetActive(false);

            FullScreenToggle.isOn = Screen.fullScreen;
        }

        public void OpenGameSettings()
        {
            GameButtonPanel.SetActive(true);
            AudioButtonPanel.SetActive(false);
        }

        public void OpenAudioSettings()
        {
            AudioButtonPanel.SetActive(true);
            GameButtonPanel.SetActive(false);
        }

        // Cierra Ajustes y vuelve al menu anterior.
        public void OpenPreviousMenu()
        {
            UINavigator.SwitchMenu(PreviousMenu, SettingsMenu);
        }

        public void SetFullScreen(bool isFullScreen)
        {
            Screen.fullScreen = isFullScreen;
        }

        // Deja todos los ajustes (pantalla, volumen y resolucion) como al principio.
        public void ResetToDefaults()
        {
            // Restablecemos pantalla completa
            FullScreenToggle.isOn = DefaultFullScreen;
            Screen.fullScreen = DefaultFullScreen;

            // Restablecemos volumen
            PlayerPrefs.SetFloat("MasterVolume", DefaultVolume);
            PlayerPrefs.DeleteKey("IsMuted");
            _audioScript.ResetToDefault();

            // Restablecemos resoluci�n a la m�s alta disponible
            _resolutionScript.ResetToDefault();

            PlayerPrefs.Save();
        }
    }
}