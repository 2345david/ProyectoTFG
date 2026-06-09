namespace settings.audio.audioScript
{
    using UnityEngine;
    using UnityEngine.UI;

    public class AudioScript : MonoBehaviour
    {
        public Slider Audio_Slider;
        public Toggle Audio_Toggle;

        const float MaxVolume = 1f;
        // Volumen global por defecto (30%). El juego sonaba demasiado alto al 100%.
        const float DefaultVolume = 0.3f;
        float _savedVolume;

        void Start()
        {
            // Clamp para evitar valores fuera de rango (p. ej. ajustes antiguos guardados
            // con una escala 0-100) que dispararían AudioListener.volume a niveles enormes.
            float saved = Mathf.Clamp01(PlayerPrefs.GetFloat("MasterVolume", DefaultVolume));

            Audio_Slider.minValue = 0f;
            Audio_Slider.maxValue = MaxVolume;
            // Importante: usar SetValueWithoutNotify/SetIsOnWithoutNotify para que
            // inicializar los controles NO dispare los callbacks (incluidos los
            // persistentes asignados en el Inspector). Si se disparan durante Start,
            // la lógica de mute re-guarda MasterVolume=0 y deja el juego en silencio.
            Audio_Slider.SetValueWithoutNotify(saved);
            AudioListener.volume = saved;
            _savedVolume = saved;

            bool wasMuted = PlayerPrefs.GetInt("IsMuted", 0) == 1;
            Audio_Toggle.SetIsOnWithoutNotify(wasMuted);
            if (wasMuted)
            {
                Audio_Slider.SetValueWithoutNotify(0f);
                Audio_Slider.interactable = false;
                AudioListener.volume = 0f;
            }

            // Evitar doble-cableado: si el control ya tiene un listener persistente
            // (asignado en el Inspector), no añadimos otro por código.
            if (Audio_Slider.onValueChanged.GetPersistentEventCount() == 0)
                Audio_Slider.onValueChanged.AddListener(OnVolumeChanged);
            if (Audio_Toggle.onValueChanged.GetPersistentEventCount() == 0)
                Audio_Toggle.onValueChanged.AddListener(OnMuteToggled);
        }

        void OnDestroy()
        {
            Audio_Slider.onValueChanged.RemoveListener(OnVolumeChanged);
            Audio_Toggle.onValueChanged.RemoveListener(OnMuteToggled);
        }

        // Se llama al mover la barra: cambia el volumen real y lo guarda.
        public void OnVolumeChanged(float value)
        {
            AudioListener.volume = value;

            if (!Audio_Toggle.isOn)
                _savedVolume = value;

            PlayerPrefs.SetFloat("MasterVolume", _savedVolume);
        }

        public void OnMuteToggled(bool isMuted)
        {
            if (isMuted)
            {
                _savedVolume = Audio_Slider.value > 0f
                    ? Audio_Slider.value
                    : _savedVolume;

                Audio_Slider.value = 0f;
                Audio_Slider.interactable = false;
            }
            else
            {
                Audio_Slider.value = _savedVolume;
                Audio_Slider.interactable = true;
            }

            AudioListener.volume = Audio_Slider.value;
            PlayerPrefs.SetInt("IsMuted", isMuted ? 1 : 0);
        }

        // Deja el audio como al principio: sin silencio y con el volumen por defecto.
        public void ResetToDefault()
        {
            // Desactivamos el silencio si estaba activo
            Audio_Toggle.isOn = false;

            // Restablecemos el volumen al valor por defecto (30%)
            Audio_Slider.value = DefaultVolume;
            Audio_Slider.interactable = true;
        }
    }
}