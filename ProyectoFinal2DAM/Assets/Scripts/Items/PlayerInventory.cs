using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }

    readonly List<float> _potionHealQueue = new List<float>();
    readonly List<float> _manaPotionQueue = new List<float>();

    public int PotionCount => _potionHealQueue.Count;
    public int ManaPotionCount => _manaPotionQueue.Count;

    public event Action OnChanged;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public List<float> GetPotionHealQueue() => new List<float>(_potionHealQueue);
    public List<float> GetManaPotionQueue() => new List<float>(_manaPotionQueue);

    public void SetPotionQueues(List<float> healthPotions, List<float> manaPotions)
    {
        _potionHealQueue.Clear();
        if (healthPotions != null) _potionHealQueue.AddRange(healthPotions);
        
        _manaPotionQueue.Clear();
        if (manaPotions != null) _manaPotionQueue.AddRange(manaPotions);
        
        OnChanged?.Invoke();
    }

    public void AddPotion(float healAmount)
    {
        if (healAmount <= 0f)
            return;
        _potionHealQueue.Add(healAmount);
        OnChanged?.Invoke();
    }

    public void AddManaPotion(float manaAmount)
    {
        if (manaAmount <= 0f)
            return;
        _manaPotionQueue.Add(manaAmount);
        OnChanged?.Invoke();
    }

    public bool TryUsePotion()
    {
        if (_potionHealQueue.Count == 0)
            return false;
        var heal = _potionHealQueue[0];
        _potionHealQueue.RemoveAt(0);
        var ph = PlayerHealth.instance;
        if (ph != null)
        {
            ph.health = Mathf.Min(ph.health + heal, ph.maxHealth);
            if (AudioManager.instance != null && AudioManager.instance.potion != null)
                AudioManager.instance.PlayAudio(AudioManager.instance.potion);
        }
        OnChanged?.Invoke();
        return true;
    }

    public bool TryUseManaPotion()
    {
        if (_manaPotionQueue.Count == 0)
            return false;
        var manaAmount = _manaPotionQueue[0];
        _manaPotionQueue.RemoveAt(0);
        if (PlayerMana.instance != null)
        {
            PlayerMana.instance.AddMana(manaAmount);
            if (AudioManager.instance != null && AudioManager.instance.potion != null)
                AudioManager.instance.PlayAudio(AudioManager.instance.potion);
        }
        OnChanged?.Invoke();
        return true;
    }
}
