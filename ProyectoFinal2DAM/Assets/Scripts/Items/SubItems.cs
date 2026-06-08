using System;
using UnityEngine;
using UnityEngine.UI;

public class SubItems : MonoBehaviour
{
    public Text subItemAmountText;

    [Header("Equipped (Ready to fire)")]
    public int subItemsAmount;

    [Header("Reserve (In inventory)")]
    public int reserveArrows;

    public static SubItems Instance;

    public static event Action OnAmountChanged;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Start()
    {
        UpdateText();
        OnAmountChanged?.Invoke();
    }

    public void AddToReserve(int amount)
    {
        reserveArrows += amount;
        OnAmountChanged?.Invoke();
    }

    public void SubItem(int subItemAmount)
    {
        subItemsAmount += subItemAmount;
        UpdateText();
        OnAmountChanged?.Invoke();
    }

    public bool EquipArrows(int amount)
    {
        if (amount <= 0 || reserveArrows < amount) return false;
        
        reserveArrows -= amount;
        subItemsAmount += amount;
        UpdateText();
        OnAmountChanged?.Invoke();
        return true;
    }

    public void UnequipArrows(int amount)
    {
        if (amount <= 0 || subItemsAmount < amount) return;

        subItemsAmount -= amount;
        reserveArrows += amount;
        UpdateText();
        OnAmountChanged?.Invoke();
    }

    private void UpdateText()
    {
        if (subItemAmountText != null)
            subItemAmountText.text = "x " + subItemsAmount.ToString();
    }
}
