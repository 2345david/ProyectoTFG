namespace audio.MusicManager
{
    using System.Collections;
    using UnityEngine;

    public class MusicManager : MonoBehaviour
    {
        public static MusicManager Instance { get; private set; }

        public AudioSource MusicSource;     // Fuente de audio para la música de fondo
        public AudioSource SFXSource;       // Fuente de audio para efectos de sonido (botones, UI)

        public AudioClip MenuMusic;         // Pista de música del menú principal
        public AudioClip GameMusic;         // Pista de música del juego
        public AudioClip ClickSound;        // Sonido al pulsar botones

        [Range(0f, 5f)]
        public float FadeDuration = 1f;

        void Awake()
        {
            // Si ya existe una instancia, destruimos el duplicado
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject); // Persiste entre escenas
        }

        public void PlayMenuMusic() => SwitchTrack(MenuMusic);
        public void PlayGameMusic() => SwitchTrack(GameMusic);
        public void PlayClick() => SFXSource.PlayOneShot(ClickSound);

        void SwitchTrack(AudioClip clip)
        {
            if (MusicSource.clip == clip) return;
            StopAllCoroutines(); // Detenemos cualquier fundido en curso antes de iniciar uno nuevo
            StartCoroutine(FadeTrack(clip));
        }

        IEnumerator FadeTrack(AudioClip newClip)
        {
            float startVolume = MusicSource.volume;

            // Fundido de salida — saltamos si la duración es 0
            if (FadeDuration > 0f)
            {
                while (MusicSource.volume > 0f)
                {
                    MusicSource.volume -= startVolume * Time.deltaTime / FadeDuration;
                    yield return null;
                }
            }

            MusicSource.volume = 0f;

            // Cambiamos la pista y la reproducimos
            MusicSource.clip = newClip;
            MusicSource.loop = true;
            MusicSource.Play();

            // Fundido de entrada — saltamos si la duración es 0
            if (FadeDuration > 0f)
            {
                while (MusicSource.volume < startVolume)
                {
                    MusicSource.volume += startVolume * Time.deltaTime / FadeDuration;
                    yield return null;
                }
            }

            MusicSource.volume = startVolume;
        }
    }
}

