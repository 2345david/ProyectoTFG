using UnityEngine;

public class ButtonSoundManager : MonoBehaviour
{
    public AudioSource AudioSource;
    public AudioClip ClickSound;

    public void PlayClickSound()
    {
        if (ClickSound != null)
            AudioSource.PlayOneShot(ClickSound);
    }
}
