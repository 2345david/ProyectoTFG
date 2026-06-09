namespace settings.audio.audioScript
{
    using UnityEngine;
    using UnityEngine.UI;

    public class AudioScript : MonoBehaviour
    {
        public Slider Audio_Slider;
        public Toggle Audio_Toggle;

        const float MaxVolume = 1f;
        float _savedVolume;

        void Start()
        {
            float saved = PlayerPrefs.GetFloat("MasterVolume", MaxVolume);

            Audio_Slider.maxValue = MaxVolume;
            Audio_Slider.value = saved;
            AudioListener.volume = saved;
            _savedVolume = saved;

            bool wasMuted = PlayerPrefs.GetInt("IsMuted", 0) == 1;
            Audio_Toggle.isOn = wasMuted;
            if (wasMuted)
            {
                Audio_Slider.value = 0f;
                Audio_Slider.interactable = false;
            }

            Audio_Slider.onValueChanged.AddListener(OnVolumeChanged);
            Audio_Toggle.onValueChanged.AddListener(OnMuteToggled);
        }

        void OnDestroy()
        {
            Audio_Slider.onValueChanged.RemoveListener(OnVolumeChanged);
            Audio_Toggle.onValueChanged.RemoveListener(OnMuteToggled);
        }

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

        public void ResetToDefault()
        {
            // Desactivamos el silencio si estaba activo
            Audio_Toggle.isOn = false;

            // Restablecemos el volumen al máximo
            Audio_Slider.value = MaxVolume;
            Audio_Slider.interactable = true;
        }
    }
}