using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Lleva la cuenta del dinero del jugador (su "hucha") y mantiene actualizado
/// el texto de la pantalla que muestra cuánto dinero tiene.
/// Solo puede haber una cuenta en todo el juego.
/// </summary>
public class BankAccount : MonoBehaviour
{
    // Dinero que tiene el jugador ahora mismo. El sistema de guardado lo recuerda entre partidas.
    public float bank;

    // Texto en pantalla donde se ve el dinero (se conecta desde el editor de Unity).
    public Text bankText;

    // Atajo para que otros scripts usen esta cuenta (solo debe haber una en todo el juego).
    public static BankAccount Instance;

    // Se ejecuta al nacer: se asegura de que solo exista una cuenta de dinero.
    private void Awake()
    {
        // Si todavía no hay ninguna, esta pasa a ser la oficial.
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            // Si ya había otra, esta sobra y se elimina.
            Destroy(gameObject);
        }
    }

    // Al empezar: muestra en pantalla el dinero que hay ahora mismo.
    public void Start()
    {
        bankText.text = "x " + bank.ToString();
    }

    // Suma el dinero recogido a la cuenta y vuelve a escribir el total en pantalla.
    /// <param name="cashCollected">Dinero recogido que se va a sumar.</param>
    public void Money(float cashCollected)
    {
        bank += cashCollected;
        bankText.text = "x " + bank.ToString();
    }
}
