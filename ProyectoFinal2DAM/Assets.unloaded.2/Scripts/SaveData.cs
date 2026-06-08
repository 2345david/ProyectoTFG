using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SaveData
{
    // Player Stats
    public float currentHealth;
    public float maxHealth;
    public float currentMana;
    public float playerDamage;
    
    // Player Experience
    public int playerLevel;
    public float currentXP;
    public float expToNextLevel;
    
    // Player Position
    public float posX, posY, posZ;
    public string sceneName;
    
    // Currency and Items
    public float bankBalance;
    public int subItemsAmount;
    public int reserveArrows;
    
    // Inventory
    public List<float> healthPotions;
    public List<float> manaPotions;

    // Boss Progress
    public List<string> defeatedBosses;

    // Player Abilities
    public bool hasDash;
    public bool hasDoubleJump;

    public SaveData()
    {
        healthPotions = new List<float>();
        manaPotions = new List<float>();
        defeatedBosses = new List<string>();
        hasDash = false;
        hasDoubleJump = false;
    }
}
