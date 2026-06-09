namespace audio.MusicManager
{
    using System.Collections;
    using UnityEngine;

    public class MusicManager : MonoBehaviour
    {
        public static MusicManager Instance { get; private set; }

        public AudioSource MusicSource;
        public AudioSource SFXSource;

        public AudioClip MenuMusic;
        public AudioClip GameMusic;
        public AudioClip ClickSound;

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
            // Fundido de salida — reducimos el volumen gradualmente
            float startVolume = MusicSource.volume;

            while (MusicSource.volume > 0f)
            {
                MusicSource.volume -= startVolume * Time.deltaTime / FadeDuration;
                yield return null; // Esperamos al siguiente fotograma
            }

            // Cambiamos la pista y la reproducimos
            MusicSource.clip = newClip;
            MusicSource.loop = true;
            MusicSource.Play();

            // Fundido de entrada — aumentamos el volumen gradualmente
            while (MusicSource.volume < startVolume)
            {
                MusicSource.volume += startVolume * Time.deltaTime / FadeDuration;
                yield return null; // Esperamos al siguiente fotograma
            }

            // Corregimos posible imprecisión de float
            MusicSource.volume = startVolume;
        }
    }
}

