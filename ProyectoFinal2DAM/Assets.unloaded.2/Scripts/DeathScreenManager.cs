using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class DeathScreenManager : MonoBehaviour
{
    public static DeathScreenManager instance;

    public GameObject deathScreenPanel;
    public Button respawnButton;
    public Button mainMenuButton;
    public string mainMenuSceneName = "MainMenu"; // Default name, change as needed

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    private void Start()
    {
        if (deathScreenPanel == null)
        {
            deathScreenPanel = GameObject.Find("DeathScreenPanel");
        }

        if (deathScreenPanel != null)
        {
            deathScreenPanel.SetActive(false);
        }

        if (respawnButton != null)
        {
            respawnButton.onClick.AddListener(Respawn);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(LoadMainMenu);
        }
    }

    public void ShowDeathScreen()
    {
        if (deathScreenPanel != null)
        {
            deathScreenPanel.SetActive(true);
            Time.timeScale = 0f; // Pause game
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void Respawn()
    {
        Time.timeScale = 1f;
        
        if (PlayerHealth.instance != null)
        {
            PlayerHealth.instance.gameObject.SetActive(true);
            
            if (CheckpointManager.instance != null)
            {
                CheckpointManager.instance.LoadGame();
            }
            
            ResetActiveBosses();
            PlayerHealth.instance.ResetStatus(); // Restore visibility/logic after loading
        }

        if (deathScreenPanel != null)
        {
            deathScreenPanel.SetActive(false);
        }
    }

    private void ResetActiveBosses()
    {
        if (BossUI.instance != null)
        {
            BossUI.instance.ResetBossUI();
        }

        // Reset any active boss behaviors
        BossBehaviour[] bosses = FindObjectsOfType<BossBehaviour>(true);
        foreach (var boss in bosses)
        {
            boss.ResetBoss();
        }

        // Reset triggers
        BossActivation[] triggers = FindObjectsOfType<BossActivation>(true);
        foreach (var trigger in triggers)
        {
            // Only reset if not already defeated
            if (CheckpointManager.instance != null && !CheckpointManager.instance.IsBossDefeated(trigger.bossID))
            {
                trigger.ResetTrigger();
            }
        }
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
