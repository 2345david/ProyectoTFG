using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections.Generic;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager instance;

    private Vector3 lastCheckpointPosition;
    private bool hasCheckpoint = false;
    private string saveFilePath;
    private List<string> defeatedBosses = new List<string>();

    public void MarkBossDefeated(string bossID)
    {
        if (!defeatedBosses.Contains(bossID))
        {
            defeatedBosses.Add(bossID);
            SaveGame();
        }
    }

    public bool IsBossDefeated(string bossID)
    {
        return defeatedBosses.Contains(bossID);
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            saveFilePath = Path.Combine(Application.persistentDataPath, "savegame.json");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // On start, if a save exists, load it
        if (File.Exists(saveFilePath))
        {
            LoadGame();
        }
    }

    public void SaveCheckpoint(Vector3 position)
    {
        lastCheckpointPosition = position;
        hasCheckpoint = true;
        
        SaveGame();
        Debug.Log("Game Saved to file at " + position);
    }

    public void SaveGame()
    {
        SaveData data = new SaveData();

        // Sync with ProgressionManager if it exists
        if (Metroidvania.Core.ProgressionManager.Instance != null)
        {
            Metroidvania.Core.ProgressionManager.Instance.SaveGame();
        }

        // Position
        data.posX = lastCheckpointPosition.x;
        data.posY = lastCheckpointPosition.y;
        data.posZ = lastCheckpointPosition.z;
        
        if (Metroidvania.Core.WorldManager.Instance != null)
        {
            data.sceneName = Metroidvania.Core.WorldManager.Instance.currentBiome;
        }
        else
        {
            data.sceneName = SceneManager.GetActiveScene().name;
        }

        // Health & Mana
        if (PlayerHealth.instance != null)
        {
            data.currentHealth = PlayerHealth.instance.health;
            data.maxHealth = PlayerHealth.instance.maxHealth;
        }
        if (PlayerMana.instance != null) data.currentMana = PlayerMana.instance.mana;

        // Experience & Stats
        if (ExperienceScript.instance != null)
        {
            data.playerLevel = ExperienceScript.instance.currentLevel;
            data.currentXP = ExperienceScript.instance.currentExperience;
            data.expToNextLevel = ExperienceScript.instance.expTNL;
        }
        if (PlayerAttack.instance != null) data.playerDamage = PlayerAttack.instance.damage;

        // Money
        if (BankAccount.Instance != null) data.bankBalance = BankAccount.Instance.bank;

        // Arrows
        if (SubItems.Instance != null)
        {
            data.subItemsAmount = SubItems.Instance.subItemsAmount;
            data.reserveArrows = SubItems.Instance.reserveArrows;
        }

        // Inventory
        if (PlayerInventory.Instance != null)
        {
            data.healthPotions = PlayerInventory.Instance.GetPotionHealQueue();
            data.manaPotions = PlayerInventory.Instance.GetManaPotionQueue();
        }

        // Bosses
        data.defeatedBosses = new List<string>(defeatedBosses);

        // Abilities
        if (PlayerController.instance != null)
        {
            data.hasDash = PlayerController.instance.hasDash;
            data.hasDoubleJump = PlayerController.instance.hasDoubleJump;
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(saveFilePath, json);
    }

    public void LoadGame()
    {
        if (!File.Exists(saveFilePath)) return;

        string json = File.ReadAllText(saveFilePath);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        // Sync with ProgressionManager
        if (Metroidvania.Core.ProgressionManager.Instance != null)
        {
            Metroidvania.Core.ProgressionManager.Instance.LoadGame();
        }

        // Bosses
        defeatedBosses = data.defeatedBosses ?? new List<string>();

        // Abilities
        if (PlayerController.instance != null)
        {
            PlayerController.instance.hasDash = data.hasDash;
            PlayerController.instance.hasDoubleJump = data.hasDoubleJump;
        }

        // Restore Position and Scene
        lastCheckpointPosition = new Vector3(data.posX, data.posY, data.posZ);
        
        if (!string.IsNullOrEmpty(data.sceneName))
        {
            if (Metroidvania.Core.WorldManager.Instance != null)
            {
                if (Metroidvania.Core.WorldManager.Instance.currentBiome != data.sceneName)
                {
                    // Start coroutine to load biome and then set position
                    StartCoroutine(LoadBiomeAndSetPosition(data.sceneName, lastCheckpointPosition));
                }
                else
                {
                    SetPlayerPosition(lastCheckpointPosition);
                }
            }
            else
            {
                // Fallback for simple scene loading if WorldManager is missing
                if (SceneManager.GetActiveScene().name != data.sceneName)
                {
                    SceneManager.LoadScene(data.sceneName);
                    // Note: position will be lost unless we handle it after load.
                    // But in this project WorldManager is expected.
                }
                SetPlayerPosition(lastCheckpointPosition);
            }
        }
        else
        {
            SetPlayerPosition(lastCheckpointPosition);
        }

        // Restore Health & Mana
        if (PlayerHealth.instance != null)
        {
            PlayerHealth.instance.maxHealth = data.maxHealth > 0 ? data.maxHealth : PlayerHealth.instance.maxHealth;
            PlayerHealth.instance.health = data.currentHealth > 0 ? data.currentHealth : PlayerHealth.instance.maxHealth;
        }
        if (PlayerMana.instance != null)
        {
            PlayerMana.instance.mana = data.currentMana;
        }

        // Restore Experience & Stats
        if (ExperienceScript.instance != null)
        {
            ExperienceScript.instance.currentLevel = data.playerLevel > 0 ? data.playerLevel : 1;
            ExperienceScript.instance.currentExperience = data.currentXP;
            ExperienceScript.instance.expTNL = data.expToNextLevel > 0 ? data.expToNextLevel : ExperienceScript.instance.expTNL;
            
            // Trigger UI update for experience and level
            ExperienceScript.instance.UpdateUI(); 
        }
        if (PlayerAttack.instance != null)
        {
            PlayerAttack.instance.damage = data.playerDamage > 0 ? data.playerDamage : PlayerAttack.instance.damage;
        }

        // Restore Money
        if (BankAccount.Instance != null)
        {
            BankAccount.Instance.bank = data.bankBalance;
            BankAccount.Instance.Money(0); // Trigger UI update
        }

        // Restore Arrows
        if (SubItems.Instance != null)
        {
            SubItems.Instance.subItemsAmount = data.subItemsAmount;
            SubItems.Instance.reserveArrows = data.reserveArrows;
            SubItems.Instance.SubItem(0); // Trigger UI update
        }

        // Restore Inventory
        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.SetPotionQueues(data.healthPotions, data.manaPotions);
        }
    }

    private void SetPlayerPosition(Vector3 position)
    {
        if (PlayerController.instance != null)
        {
            PlayerController.instance.transform.position = position;
        }
        else
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                player.transform.position = position;
            }
        }
    }

    private System.Collections.IEnumerator LoadBiomeAndSetPosition(string biomeName, Vector3 position)
    {
        if (Metroidvania.Core.WorldManager.Instance != null)
        {
            // Wait if WorldManager is already doing something (like loading the initial biome)
            while (Metroidvania.Core.WorldManager.Instance.isTransitioning)
            {
                yield return null;
            }

            if (Metroidvania.Core.WorldManager.Instance.currentBiome == biomeName)
            {
                SetPlayerPosition(position);
                yield break;
            }

            Metroidvania.Core.WorldManager.Instance.TravelToBiome(biomeName, "LOADING_FROM_SAVE");
            
            // Wait until WorldManager is done transitioning
            while (Metroidvania.Core.WorldManager.Instance.isTransitioning || Metroidvania.Core.WorldManager.Instance.currentBiome != biomeName)
            {
                yield return null;
            }
            
            // Wait one more frame to ensure PositionPlayer("") has finished if it was called
            yield return null;
            
            SetPlayerPosition(position);
        }
    }

    public Vector3 GetRespawnPosition()
    {
        return lastCheckpointPosition;
    }

    public void ResetCheckpoint()
    {
        if (File.Exists(saveFilePath)) File.Delete(saveFilePath);
    }
}
