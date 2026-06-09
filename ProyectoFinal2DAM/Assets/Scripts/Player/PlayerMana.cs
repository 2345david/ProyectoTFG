using UnityEngine;
using UnityEngine.UI;     // Sirve para usar la barra de maná (componente Image) en la pantalla.

/// <summary>
/// Este script lleva la cuenta del maná (energía mágica) del jugador.
/// Permite gastarlo, recuperarlo, no pasarse del máximo y mostrarlo en una barra.
/// Otras habilidades, como el doble salto y el dash, gastan este maná.
/// </summary>
public class PlayerMana : MonoBehaviour
{
    public float mana;                  // Maná que tiene ahora mismo el jugador.
    public float maxMana = 100f;        // Maná máximo que puede tener el jugador.
    public Image manaBar;               // La barra de maná que se ve en la pantalla.

    // "instance" es un atajo para que otros scripts puedan usar este maná fácilmente desde cualquier sitio.
    public static PlayerMana instance;

    // Esto se ejecuta al crearse el objeto. Guardamos el atajo "instance" para que otros lo encuentren.
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    // Esto se ejecuta una vez al empezar. Llenamos el maná al máximo y dibujamos la barra llena.
    void Start()
    {
        mana = maxMana;
        if (manaBar != null)
        {
            manaBar.fillAmount = mana / maxMana;
        }
    }

    // Esto se repite muchas veces por segundo. Actualiza la barra y evita pasarse del maná máximo.
    void Update()
    {
        // Ponemos la barra más o menos llena según el maná que quede.
        if (manaBar != null)
        {
            manaBar.fillAmount = mana / maxMana;
        }

        // Si por algún motivo el maná pasó del máximo, lo dejamos en el máximo.
        if (mana > maxMana)
        {
            mana = maxMana;
        }
    }

    // Gasta maná. Si quedaría por debajo de 0, lo dejamos en 0 (no puede haber maná negativo).
    public void UseMana(float amount)
    {
        mana -= amount;
        if (mana < 0) mana = 0;
    }

    // Recupera maná. Si pasaría del máximo, lo dejamos en el máximo.
    public void AddMana(float amount)
    {
        mana += amount;
        if (mana > maxMana) mana = maxMana;
    }

    // Responde "sí" o "no" a la pregunta: ¿tengo suficiente maná para hacer algo?
    public bool HasEnoughMana(float amount)
    {
        return mana >= amount;
    }
}
