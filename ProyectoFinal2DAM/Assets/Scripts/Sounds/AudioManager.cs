using UnityEngine;
using UnityEngine.Audio;   // AudioMixer (control de volumen de música/efectos)
using UnityEngine.UI;      // Slider (controles de volumen en el menú de opciones)

/// <summary>
/// Es el encargado de todos los sonidos del juego: la música y los efectos (golpes,
/// monedas, etc.). También aplica el volumen que el jugador elige en las opciones.
/// Solo existe uno (singleton) y los demás scripts reproducen sonidos llamando a PlayAudio.
/// </summary>
public class AudioManager : MonoBehaviour
{
    // El único AudioManager que existe; se puede usar desde cualquier script.
    public static AudioManager instance;

    // Los "mezcladores" de audio: sirven para subir o bajar el volumen por grupos.
    public AudioMixer music;     // Mezclador de la música de fondo.
    public AudioMixer effects;   // Mezclador de los efectos de sonido.

    // Cada sonido del juego. Son públicos para poder asignarlos en el Inspector
    // y para que otros scripts puedan reproducirlos con PlayAudio().
    public AudioSource backgroundMusic;   // Música de fondo del nivel.
    public AudioSource hit;               // Golpe / impacto.
    public AudioSource enemyDead;         // Muerte de enemigo.
    public AudioSource coin;              // Recoger moneda.
    public AudioSource arrow;             // Disparo de flecha.
    public AudioSource playerDead;        // Muerte del jugador.
    public AudioSource LvlUp;             // Subida de nivel.
    public AudioSource flame;             // Llama / fuego.
    public AudioSource potion;            // Uso de poción.
    public AudioSource recoger_flechas;   // Recoger flechas.

    // El volumen actual (de 0 a 1) según las barras deslizantes; útil para recordar los ajustes.
    public float masterVol;
    public float effectsVol;

    // Las barras deslizantes del menú de opciones que controlan los volúmenes.
    public Slider masterSldr;
    public Slider effectsSldr;

    // Los nombres de los controles de volumen dentro de los mezcladores (deben escribirse igual).
    const string MusicVolumeParam = "masterVolume";
    const string EffectsVolumeParam = "effectsVolume";

    // Se ejecuta al nacer: deja a este como el único AudioManager (si ya había otro, este se borra).
    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }

    // Se ejecuta al empezar: conecta las barras de volumen, aplica el volumen y arranca la música.
    void Start()
    {
        // Barra de volumen general: la "escuchamos" para enterarnos cuando cambie y aplicamos su valor.
        if (masterSldr != null)
        {
            masterSldr.onValueChanged.AddListener(OnMasterSlider);
            OnMasterSlider(masterSldr.value);
        }

        // Barra de volumen de efectos: igual que la anterior, la escuchamos y aplicamos su valor.
        if (effectsSldr != null)
        {
            effectsSldr.onValueChanged.AddListener(OnEffectsSlider);
            OnEffectsSlider(effectsSldr.value);
        }

        // Ponemos la música de fondo solo si no estaba ya sonando.
        if (backgroundMusic != null && !backgroundMusic.isPlaying)
            backgroundMusic.Play();
    }

    // Se ejecuta al borrarse este objeto: limpia la referencia para no dejar un "fantasma".
    void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    // Se llama cuando se mueve la barra de volumen general: guarda el valor y lo aplica a la música.
    void OnMasterSlider(float linear)
    {
        masterVol = linear;
        ApplyMixerVolume(music, MusicVolumeParam, linear);
    }

    // Se llama cuando se mueve la barra de efectos: guarda el valor y lo aplica a los efectos.
    void OnEffectsSlider(float linear)
    {
        effectsVol = linear;
        ApplyMixerVolume(effects, EffectsVolumeParam, linear);
    }

    /// <summary>
    /// Convierte el volumen (de 0 a 1) a la unidad que entienden los mezcladores (decibelios)
    /// y se lo pone. Si el volumen es casi cero, lo deja en silencio total (-80).
    /// </summary>
    static void ApplyMixerVolume(AudioMixer mixer, string paramName, float linear)
    {
        if (mixer == null)
            return;
        // El oído nota el volumen de forma especial, por eso hay que convertirlo con esta fórmula.
        float db = linear > 0.0001f ? Mathf.Log10(linear) * 20f : -80f;
        mixer.SetFloat(paramName, db);
    }

    // Reproduce el sonido que se le pase (comprobando antes que no sea nulo).
    // Es la forma en la que el resto del juego pide que suene un efecto.
    public void PlayAudio(AudioSource source)
    {
        if (source == null)
            return;
        source.Play();
    }
}
