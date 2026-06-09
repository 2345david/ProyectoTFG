using System;
using System.Collections.Generic;
// Nota: se eliminó "using UnityEngine;" porque esta clase es de datos puros
// y no utiliza ningún tipo de UnityEngine.

/// <summary>
/// Contenedor de datos puros que representa el estado guardado de la partida.
/// Es [Serializable] para poder convertirse a JSON con JsonUtility desde
/// CheckpointManager. TODOS los campos públicos se serializan al guardar,
/// por lo que no deben eliminarse aunque parezcan sin uso en código.
/// </summary>
[Serializable]
public class SaveData
{
    // --- Estadísticas del jugador ---
    public float currentHealth;   // Vida actual
    public float maxHealth;       // Vida máxima
    public float currentMana;     // Maná actual
    public float playerDamage;    // Daño de ataque del jugador
    
    // --- Experiencia del jugador ---
    public int playerLevel;       // Nivel actual
    public float currentXP;       // Experiencia acumulada en el nivel actual
    public float expToNextLevel;  // Experiencia necesaria para subir de nivel
    
    // --- Posición del jugador ---
    public float posX, posY, posZ; // Coordenadas del último checkpoint
    public string sceneName;       // Bioma/escena del último checkpoint

    // --- Metadatos de la ranura de guardado ---
    public string saveTime;        // Fecha/hora del guardado (la rellena SaveSystem)
    
    // --- Moneda e ítems ---
    public float bankBalance;     // Dinero en el banco
    public int subItemsAmount;    // Cantidad de subítem equipado (flechas en mano)
    public int reserveArrows;     // Flechas de reserva
    
    // --- Inventario ---
    public List<float> healthPotions; // Cola de pociones de vida (valores de curación)
    public List<float> manaPotions;   // Cola de pociones de maná

    // --- Progreso de jefes ---
    public List<string> defeatedBosses; // IDs de los jefes ya derrotados

    // --- Habilidades del jugador ---
    public bool hasDash;        // ¿Tiene desbloqueado el dash?
    public bool hasDoubleJump;  // ¿Tiene desbloqueado el doble salto?

    // Se ejecuta al crear una ficha nueva: prepara las listas vacías y las
    // habilidades en "false" (apagadas) para que nunca queden "sin valor".
    public SaveData()
    {
        healthPotions = new List<float>();
        manaPotions = new List<float>();
        defeatedBosses = new List<string>();
        hasDash = false;
        hasDoubleJump = false;
    }
}
