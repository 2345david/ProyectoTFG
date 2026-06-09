using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Este script lleva la cuenta de las flechas del jugador.
/// Separa las flechas "equipadas" (listas para disparar) de las que están
/// guardadas en la "reserva" (mochila). Solo puede existir uno en el juego.
/// </summary>
public class SubItems : MonoBehaviour
{
    // Texto en pantalla que muestra cuántas flechas hay equipadas (se conecta desde el editor de Unity).
    public Text subItemAmountText;

    [Header("Equipped (Ready to fire)")]
    // Flechas equipadas: las que se pueden disparar ya mismo.
    public int subItemsAmount;

    [Header("Reserve (In inventory)")]
    // Flechas guardadas en la mochila: hay que equiparlas antes de poder dispararlas.
    public int reserveArrows;

    // Atajo para que otros scripts puedan usar este (solo debe haber uno en todo el juego).
    public static SubItems Instance;

    // Aviso que se envía a otras partes del juego cada vez que cambia el número de flechas.
    public static event Action OnAmountChanged;

    // Se ejecuta al nacer el objeto: se asegura de que solo exista una copia de este script.
    private void Awake()
    {
        // Si todavía no hay ninguno, este pasa a ser el oficial.
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            // Si ya había otro, este sobra y se elimina.
            Destroy(gameObject);
        }
    }

    // Al empezar: muestra el número de flechas en pantalla y avisa a los demás del estado inicial.
    public void Start()
    {
        UpdateText();
        OnAmountChanged?.Invoke();
    }

    // Mete flechas en la mochila (reserva) y avisa del cambio.
    public void AddToReserve(int amount)
    {
        reserveArrows += amount;
        OnAmountChanged?.Invoke();
    }

    // Suma flechas directamente a las equipadas, refresca el texto y avisa del cambio.
    public void SubItem(int subItemAmount)
    {
        subItemsAmount += subItemAmount;
        UpdateText();
        OnAmountChanged?.Invoke();
    }

    // Pasa flechas de la mochila a las equipadas.
    // Devuelve false (no se pudo) si el número es inválido o no hay suficientes en la mochila.
    public bool EquipArrows(int amount)
    {
        // Comprobamos que la cantidad sea mayor que cero y que haya suficiente en la mochila.
        if (amount <= 0 || reserveArrows < amount) return false;

        // Quitamos de la mochila y sumamos a las equipadas.
        reserveArrows -= amount;
        subItemsAmount += amount;
        UpdateText();
        OnAmountChanged?.Invoke();
        return true;
    }

    // Devuelve flechas de las equipadas a la mochila (solo si hay suficientes equipadas).
    public void UnequipArrows(int amount)
    {
        // Comprobamos que la cantidad sea mayor que cero y que haya suficientes equipadas.
        if (amount <= 0 || subItemsAmount < amount) return;

        // Quitamos de las equipadas y sumamos a la mochila.
        subItemsAmount -= amount;
        reserveArrows += amount;
        UpdateText();
        OnAmountChanged?.Invoke();
    }

    // Escribe en el texto de la pantalla cuántas flechas hay equipadas.
    private void UpdateText()
    {
        if (subItemAmountText != null)
            subItemAmountText.text = "x " + subItemsAmount.ToString();
    }
}
