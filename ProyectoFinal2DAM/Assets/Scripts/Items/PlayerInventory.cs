using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// La mochila (inventario) del jugador. Guarda las pociones de vida y de maná
/// que el jugador recoge para poder usarlas más tarde. Permite añadir, consultar,
/// usar y volver a cargar las pociones (al continuar una partida guardada).
/// Solo puede haber una mochila en todo el juego.
/// </summary>
public class PlayerInventory : MonoBehaviour
{
    // Atajo para que otros scripts usen esta mochila (solo debe haber una en todo el juego).
    public static PlayerInventory Instance { get; private set; }

    // Lista de pociones de vida. Cada número guardado es cuánta vida cura esa poción.
    readonly List<float> _potionHealQueue = new List<float>();
    // Lista de pociones de maná. Cada número guardado es cuánto maná da esa poción.
    readonly List<float> _manaPotionQueue = new List<float>();

    // Cuántas pociones hay de cada tipo. Solo se pueden leer (las usa la pantalla para mostrar el número).
    public int PotionCount => _potionHealQueue.Count;
    public int ManaPotionCount => _manaPotionQueue.Count;

    // Aviso que se envía cada vez que cambia la mochila (para que la pantalla se actualice).
    public event Action OnChanged;

    // Se ejecuta al nacer: se asegura de que solo exista una mochila.
    void Awake()
    {
        // Si ya había otra mochila, esta sobra y se elimina.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        // Si no había ninguna, esta pasa a ser la oficial.
        Instance = this;
    }

    // Se ejecuta cuando este objeto se elimina: borra el atajo si era la mochila oficial.
    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    // Entregan una copia de las listas (para que otros no modifiquen sin querer las listas de dentro).
    public List<float> GetPotionHealQueue() => new List<float>(_potionHealQueue);
    public List<float> GetManaPotionQueue() => new List<float>(_manaPotionQueue);

    // Reemplaza las dos listas de pociones por otras.
    // Lo usa el sistema de guardado para dejar la mochila igual que en la partida guardada.
    public void SetPotionQueues(List<float> healthPotions, List<float> manaPotions)
    {
        // Vaciamos la lista de vida y metemos las pociones guardadas (si las hay).
        _potionHealQueue.Clear();
        if (healthPotions != null) _potionHealQueue.AddRange(healthPotions);

        // Vaciamos la lista de maná y metemos las pociones guardadas (si las hay).
        _manaPotionQueue.Clear();
        if (manaPotions != null) _manaPotionQueue.AddRange(manaPotions);

        // Avisamos del cambio para que la pantalla se actualice.
        OnChanged?.Invoke();
    }

    // Guarda una poción de vida en la mochila (no hace nada si la cantidad es cero o negativa).
    public void AddPotion(float healAmount)
    {
        if (healAmount <= 0f)
            return;
        _potionHealQueue.Add(healAmount);
        OnChanged?.Invoke();
    }

    // Guarda una poción de maná en la mochila (no hace nada si la cantidad es cero o negativa).
    public void AddManaPotion(float manaAmount)
    {
        if (manaAmount <= 0f)
            return;
        _manaPotionQueue.Add(manaAmount);
        OnChanged?.Invoke();
    }

    // Intenta usar la primera poción de vida guardada.
    // Devuelve false (no se pudo) si no hay ninguna; si la hay, cura al jugador y suena el efecto.
    public bool TryUsePotion()
    {
        // Si no hay pociones, no se puede usar nada.
        if (_potionHealQueue.Count == 0)
            return false;

        // Cogemos la primera poción de la lista (la más antigua) y la sacamos de la mochila.
        var heal = _potionHealQueue[0];
        _potionHealQueue.RemoveAt(0);

        // Sumamos vida al jugador, pero sin pasarnos de su vida máxima.
        var ph = PlayerHealth.instance;
        if (ph != null)
        {
            ph.health = Mathf.Min(ph.health + heal, ph.maxHealth);
            if (AudioManager.instance != null && AudioManager.instance.potion != null)
                AudioManager.instance.PlayAudio(AudioManager.instance.potion);
        }

        // Avisamos del cambio y confirmamos que se usó una poción.
        OnChanged?.Invoke();
        return true;
    }

    // Intenta usar la primera poción de maná guardada.
    // Devuelve false (no se pudo) si no hay ninguna; si la hay, da maná al jugador y suena el efecto.
    public bool TryUseManaPotion()
    {
        // Si no hay pociones de maná, no se puede usar nada.
        if (_manaPotionQueue.Count == 0)
            return false;

        // Cogemos la primera poción de la lista (la más antigua) y la sacamos de la mochila.
        var manaAmount = _manaPotionQueue[0];
        _manaPotionQueue.RemoveAt(0);

        // Damos maná al jugador y hacemos sonar el efecto.
        if (PlayerMana.instance != null)
        {
            PlayerMana.instance.AddMana(manaAmount);
            if (AudioManager.instance != null && AudioManager.instance.potion != null)
                AudioManager.instance.PlayAudio(AudioManager.instance.potion);
        }

        // Avisamos del cambio y confirmamos que se usó una poción.
        OnChanged?.Invoke();
        return true;
    }
}
