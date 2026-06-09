using System.Collections;
using UnityEngine;

/// <summary>
/// Reproduce la musica de fondo del menu de forma fiable.
/// Si se llama a Play() justo en el primer frame (mientras el sistema de audio aun
/// se esta inicializando), la reproduccion se pierde. Por eso esperamos a que el
/// AudioSource este listo y reintentamos durante unos frames hasta que suene.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class MenuMusic : MonoBehaviour
{
    private AudioSource _source;

    private void Awake()
    {
        _source = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        // Arrancamos la musica de forma diferida y con reintentos para evitar la
        // carrera del primer frame, donde un Play() temprano no surte efecto.
        StartCoroutine(EnsurePlaying());
    }

    private IEnumerator EnsurePlaying()
    {
        if (_source == null || _source.clip == null)
            yield break;

        // Reintentamos durante varios frames hasta confirmar que esta sonando.
        for (int i = 0; i < 30; i++)
        {
            if (_source.isPlaying)
                yield break;

            _source.Play();

            // Esperamos un par de frames para dar tiempo a cargar y arrancar.
            yield return null;
            yield return null;
        }
    }
}
