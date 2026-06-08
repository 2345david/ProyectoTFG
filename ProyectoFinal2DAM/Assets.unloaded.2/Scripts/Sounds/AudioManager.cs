using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public AudioMixer music;
    public AudioMixer effects;
    public AudioSource backgroundMusic;
    public AudioSource hit;
    public AudioSource enemyDead;
    public AudioSource coin;
    public AudioSource arrow;
    public AudioSource playerDead;
    public AudioSource LvlUp;
    public AudioSource flame;
    public AudioSource potion;
    public AudioSource recoger_flechas;
    public float masterVol;
    public float effectsVol;
    public Slider masterSldr;
    public Slider effectsSldr;

    const string MusicVolumeParam = "masterVolume";
    const string EffectsVolumeParam = "effectsVolume";

    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }

    void Start()
    {
        if (masterSldr != null)
        {
            masterSldr.onValueChanged.AddListener(OnMasterSlider);
            OnMasterSlider(masterSldr.value);
        }

        if (effectsSldr != null)
        {
            effectsSldr.onValueChanged.AddListener(OnEffectsSlider);
            OnEffectsSlider(effectsSldr.value);
        }

        if (backgroundMusic != null && !backgroundMusic.isPlaying)
            backgroundMusic.Play();
    }

    void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    void OnMasterSlider(float linear)
    {
        masterVol = linear;
        ApplyMixerVolume(music, MusicVolumeParam, linear);
    }

    void OnEffectsSlider(float linear)
    {
        effectsVol = linear;
        ApplyMixerVolume(effects, EffectsVolumeParam, linear);
    }

    static void ApplyMixerVolume(AudioMixer mixer, string paramName, float linear)
    {
        if (mixer == null)
            return;
        float db = linear > 0.0001f ? Mathf.Log10(linear) * 20f : -80f;
        mixer.SetFloat(paramName, db);
    }

    public void PlayAudio(AudioSource source)
    {
        if (source == null)
            return;
        source.Play();
    }
}
