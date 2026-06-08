using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopUI : MonoBehaviour
{
    [Header("Prices")]
    public float healthPotionPrice = 20f;
    public float manaPotionPrice = 20f;
    public float arrowPrice = 10f;
    public int arrowAmount = 10;

    [Header("UI References")]
    public TextMeshProUGUI healthPotionPriceText;
    public TextMeshProUGUI manaPotionPriceText;
    public TextMeshProUGUI arrowPriceText;
    public TextMeshProUGUI balanceText;

    [Header("Buttons")]
    public Button buyHealthBtn;
    public Button buyManaBtn;
    public Button buyArrowsBtn;

    void Awake()
    {
        InitializeButtons();
    }

    void OnEnable()
    {
        InitializeButtons(); // Ensure listeners are added
        UpdatePriceLabels();
        UpdateBalance();
    }

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
        // Keep balance updated while open
        if (gameObject.activeInHierarchy)
        {
            UpdateBalance();
        }
    }

    void UpdatePriceLabels()
    {
        if (healthPotionPriceText != null) healthPotionPriceText.text = healthPotionPrice.ToString();
        if (manaPotionPriceText != null) manaPotionPriceText.text = manaPotionPrice.ToString();
        if (arrowPriceText != null) arrowPriceText.text = arrowPrice.ToString();
    }

    void UpdateBalance()
    {
        if (balanceText != null && BankAccount.Instance != null)
        {
            balanceText.text = "Tus Monedas: " + BankAccount.Instance.bank.ToString();
        }
    }

    public void BuyHealthPotion()
    {
        if (BankAccount.Instance == null) { Debug.LogError("[ShopUI] BankAccount.Instance is missing!"); return; }
        if (PlayerInventory.Instance == null) { Debug.LogError("[ShopUI] PlayerInventory.Instance is missing!"); return; }

        Debug.Log($"[ShopUI] Attempting to buy Health Potion. Cost: {healthPotionPrice}, Bank: {BankAccount.Instance.bank}");
        
        if (BankAccount.Instance.bank >= healthPotionPrice)
        {
            BankAccount.Instance.Money(-healthPotionPrice);
            PlayerInventory.Instance.AddPotion(50f);
            PlayBuySound();
            Debug.Log("[ShopUI] Purchased Health Potion successfully.");
        }
        else
        {
            Debug.LogWarning("[ShopUI] Not enough money for Health Potion.");
        }
    }

    public void BuyManaPotion()
    {
        if (BankAccount.Instance == null) { Debug.LogError("[ShopUI] BankAccount.Instance is missing!"); return; }
        if (PlayerInventory.Instance == null) { Debug.LogError("[ShopUI] PlayerInventory.Instance is missing!"); return; }

        Debug.Log($"[ShopUI] Attempting to buy Mana Potion. Cost: {manaPotionPrice}, Bank: {BankAccount.Instance.bank}");

        if (BankAccount.Instance.bank >= manaPotionPrice)
        {
            BankAccount.Instance.Money(-manaPotionPrice);
            PlayerInventory.Instance.AddManaPotion(50f);
            PlayBuySound();
            Debug.Log("[ShopUI] Purchased Mana Potion successfully.");
        }
        else
        {
            Debug.LogWarning("[ShopUI] Not enough money for Mana Potion.");
        }
    }

    public void BuyArrows()
    {
        if (BankAccount.Instance == null) { Debug.LogError("[ShopUI] BankAccount.Instance is missing!"); return; }
        if (SubItems.Instance == null) { Debug.LogError("[ShopUI] SubItems.Instance is missing!"); return; }

        Debug.Log($"[ShopUI] Attempting to buy Arrows. Cost: {arrowPrice}, Bank: {BankAccount.Instance.bank}");

        if (BankAccount.Instance.bank >= arrowPrice)
        {
            BankAccount.Instance.Money(-arrowPrice);
            SubItems.Instance.AddToReserve(arrowAmount);
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
        Time.timeScale = 1f;
    }

    public void CloseShop()
    {
        gameObject.SetActive(false);
    }

    void PlayBuySound()
{
        if (AudioManager.instance != null && AudioManager.instance.potion != null)
        {
            AudioManager.instance.PlayAudio(AudioManager.instance.potion);
        }
    }
}
