using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Gestiona la interfaz de la tienda: muestra precios y saldo, conecta los
/// botones de compra y aplica las transacciones (descontar dinero y entregar
/// el ítem) validando que el jugador tenga monedas suficientes.
/// </summary>
public class ShopUI : MonoBehaviour
{
    [Header("Prices")]
    public float healthPotionPrice = 20f; // Precio de la poción de vida
    public float manaPotionPrice = 20f;   // Precio de la poción de maná
    public float arrowPrice = 10f;        // Precio del lote de flechas
    public int arrowAmount = 10;          // Cantidad de flechas que da cada compra

    [Header("UI References")]
    public TextMeshProUGUI healthPotionPriceText; // Etiqueta de precio de poción de vida
    public TextMeshProUGUI manaPotionPriceText;   // Etiqueta de precio de poción de maná
    public TextMeshProUGUI arrowPriceText;        // Etiqueta de precio de flechas
    public TextMeshProUGUI balanceText;           // Etiqueta del saldo de monedas

    [Header("Buttons")]
    public Button buyHealthBtn; // Botón de comprar poción de vida
    public Button buyManaBtn;   // Botón de comprar poción de maná
    public Button buyArrowsBtn; // Botón de comprar flechas

    void Awake()
    {
        // Conectamos los botones lo antes posible.
        InitializeButtons();
    }

    void OnEnable()
    {
        // Cada vez que se abre la tienda nos aseguramos de tener los listeners
        // conectados y refrescamos los textos de precios y saldo.
        InitializeButtons(); // Garantiza que los listeners estén añadidos
        UpdatePriceLabels();
        UpdateBalance();
    }

    /// <summary>
    /// Conecta cada botón con su método de compra. Primero quita el listener
    /// y luego lo añade para evitar suscripciones duplicadas si se llama varias veces.
    /// </summary>
    private void InitializeButtons()
    {
        if (buyHealthBtn != null)
        {
            buyHealthBtn.onClick.RemoveListener(BuyHealthPotion);
            buyHealthBtn.onClick.AddListener(BuyHealthPotion);
        }
        if (buyManaBtn != null)
        {
            buyManaBtn.onClick.RemoveListener(BuyManaPotion);
            buyManaBtn.onClick.AddListener(BuyManaPotion);
        }
        if (buyArrowsBtn != null)
        {
            buyArrowsBtn.onClick.RemoveListener(BuyArrows);
            buyArrowsBtn.onClick.AddListener(BuyArrows);
        }
    }

    void Update()
    {
        // Mientras la tienda esté abierta, mantenemos el saldo actualizado en pantalla.
        if (gameObject.activeInHierarchy)
        {
            UpdateBalance();
        }
    }

    /// <summary>
    /// Vuelca los precios configurados en sus etiquetas de texto.
    /// </summary>
    void UpdatePriceLabels()
    {
        if (healthPotionPriceText != null) healthPotionPriceText.text = healthPotionPrice.ToString();
        if (manaPotionPriceText != null) manaPotionPriceText.text = manaPotionPrice.ToString();
        if (arrowPriceText != null) arrowPriceText.text = arrowPrice.ToString();
    }

    /// <summary>
    /// Actualiza la etiqueta del saldo con el dinero actual del banco del jugador.
    /// </summary>
    void UpdateBalance()
    {
        if (balanceText != null && BankAccount.Instance != null)
        {
            balanceText.text = "Tus Monedas: " + BankAccount.Instance.bank.ToString();
        }
    }

    /// <summary>
    /// Compra una poción de vida si el jugador tiene saldo suficiente:
    /// descuenta el precio, añade la poción al inventario y reproduce el sonido.
    /// </summary>
    public void BuyHealthPotion()
    {
        // Comprobamos que existan las piezas necesarias (el banco de monedas y la mochila) antes de comprar.
        if (BankAccount.Instance == null) { Debug.LogError("[ShopUI] BankAccount.Instance is missing!"); return; }
        if (PlayerInventory.Instance == null) { Debug.LogError("[ShopUI] PlayerInventory.Instance is missing!"); return; }

        Debug.Log($"[ShopUI] Attempting to buy Health Potion. Cost: {healthPotionPrice}, Bank: {BankAccount.Instance.bank}");
        
        // Solo compramos si hay monedas suficientes.
        if (BankAccount.Instance.bank >= healthPotionPrice)
        {
            BankAccount.Instance.Money(-healthPotionPrice); // Descontar el precio
            PlayerInventory.Instance.AddPotion(50f);        // Añadir poción (cura 50)
            PlayBuySound();
            Debug.Log("[ShopUI] Purchased Health Potion successfully.");
        }
        else
        {
            Debug.LogWarning("[ShopUI] Not enough money for Health Potion.");
        }
    }

    /// <summary>
    /// Compra una poción de maná si el jugador tiene saldo suficiente.
    /// </summary>
    public void BuyManaPotion()
    {
        if (BankAccount.Instance == null) { Debug.LogError("[ShopUI] BankAccount.Instance is missing!"); return; }
        if (PlayerInventory.Instance == null) { Debug.LogError("[ShopUI] PlayerInventory.Instance is missing!"); return; }

        Debug.Log($"[ShopUI] Attempting to buy Mana Potion. Cost: {manaPotionPrice}, Bank: {BankAccount.Instance.bank}");

        if (BankAccount.Instance.bank >= manaPotionPrice)
        {
            BankAccount.Instance.Money(-manaPotionPrice);   // Descontar el precio
            PlayerInventory.Instance.AddManaPotion(50f);    // Añadir poción de maná (restaura 50)
            PlayBuySound();
            Debug.Log("[ShopUI] Purchased Mana Potion successfully.");
        }
        else
        {
            Debug.LogWarning("[ShopUI] Not enough money for Mana Potion.");
        }
    }

    /// <summary>
    /// Compra un lote de flechas (arrowAmount) si el jugador tiene saldo suficiente.
    /// </summary>
    public void BuyArrows()
    {
        if (BankAccount.Instance == null) { Debug.LogError("[ShopUI] BankAccount.Instance is missing!"); return; }
        if (SubItems.Instance == null) { Debug.LogError("[ShopUI] SubItems.Instance is missing!"); return; }

        Debug.Log($"[ShopUI] Attempting to buy Arrows. Cost: {arrowPrice}, Bank: {BankAccount.Instance.bank}");

        if (BankAccount.Instance.bank >= arrowPrice)
        {
            BankAccount.Instance.Money(-arrowPrice);          // Descontar el precio
            SubItems.Instance.AddToReserve(arrowAmount);      // Añadir flechas a la reserva
            PlayBuySound();
            Debug.Log("[ShopUI] Purchased Arrows successfully.");
        }
        else
        {
            Debug.LogWarning("[ShopUI] Not enough money for Arrows.");
        }
    }

    void OnDisable()
    {
        // Al cerrar/ocultar la tienda nos aseguramos de reanudar el tiempo del juego.
        Time.timeScale = 1f;
    }

    /// <summary>
    /// Cierra la tienda desactivando su GameObject (dispara OnDisable).
    /// </summary>
    public void CloseShop()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Hace sonar el efecto de compra usando el AudioManager (el que maneja los sonidos), si existe.
    /// </summary>
    void PlayBuySound()
    {
        if (AudioManager.instance != null && AudioManager.instance.potion != null)
        {
            AudioManager.instance.PlayAudio(AudioManager.instance.potion);
        }
    }
}
