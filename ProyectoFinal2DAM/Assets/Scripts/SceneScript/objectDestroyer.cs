using UnityEngine;

/// <summary>
/// Borra (elimina) solo este objeto del juego después de unos segundos.
/// Sirve para cosas que duran poco, como un golpe, un disparo o una chispa.
/// </summary>
public class objectDestroyer : MonoBehaviour
{
    // Cuántos segundos esperar antes de borrar el objeto (lo eliges en el Inspector)
    public float secondsToDestroy;

    // Al empezar: deja programado que este objeto se borre solo cuando pase el tiempo indicado.
    void Start()
    {
        // Borra este objeto cuando pasen 'secondsToDestroy' segundos
        Destroy(gameObject, secondsToDestroy);
    }
}
