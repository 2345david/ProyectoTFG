using UnityEngine;

/// <summary>
/// Hace que un objeto (normalmente un objeto que se puede recoger) suba y baje
/// suavemente todo el rato, como si flotara, para que se note y llame la atención.
/// </summary>
public class animaticn_items : MonoBehaviour
{
    // Lo rápido que sube y baja el objeto.
    public float floatSpeed = 2f;
    // Cuánto se aleja del centro al subir y al bajar (qué tan grande es el movimiento).
    public float floatHeight = 0.25f;

    // Sitio donde empezó el objeto; lo usamos como "centro" para subir y bajar desde ahí.
    private Vector3 startPos;

    // Al empezar, recordamos dónde estaba el objeto para siempre moverlo respecto a ese punto.
    void Start()
    {
        startPos = transform.position;
    }

    // Cada cuadro (frame) calculamos una nueva altura para que el objeto suba y baje suavemente.
    void Update()
    {
        // Mathf.Sin es una función que da un número que sube y baja entre -1 y 1 una y otra vez.
        // Al multiplicarlo por floatHeight obtenemos cuánto subir o bajar respecto a la altura inicial.
        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        // Colocamos el objeto en su nueva altura; el ancho (X) y la profundidad (Z) no cambian.
        transform.position = new Vector3(startPos.x, newY, startPos.z);
    }
}