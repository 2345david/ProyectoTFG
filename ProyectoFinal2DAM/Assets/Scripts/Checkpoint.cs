using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class Checkpoint : MonoBehaviour
{
    public GameObject promptUI; // The "Press E to save" text object
    private bool playerInRange = false;

    private void Start()
    {
        if (promptUI == null)
        {
            promptUI = GameObject.Find("CheckpointPrompt");
        }

        if (promptUI != null)
        {
            promptUI.SetActive(false);
        }
    }

    private void Update()
    {
        if (playerInRange)
        {
            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                SaveGame();
            }
        }
    }

    private void SaveGame()
    {
        if (CheckpointManager.instance != null)
        {
            CheckpointManager.instance.SaveCheckpoint(transform.position);
            // Optionally add a feedback effect here (sound, particle, etc.)
            Debug.Log("Game Saved at " + transform.position);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Try to find the tag on the object or its parent
        bool isPlayer = other.CompareTag("Player") || (other.attachedRigidbody != null && other.attachedRigidbody.CompareTag("Player"));
        
        Debug.Log($"[Checkpoint] Trigger Enter by {other.name}. IsPlayer: {isPlayer}");
        
        if (isPlayer)
        {
            playerInRange = true;
            if (promptUI != null)
            {
                promptUI.SetActive(true);
                Debug.Log("[Checkpoint] Prompt UI Activated");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        bool isPlayer = other.CompareTag("Player") || (other.attachedRigidbody != null && other.attachedRigidbody.CompareTag("Player"));
        
        if (isPlayer)
        {
            playerInRange = false;
            if (promptUI != null)
            {
                promptUI.SetActive(false);
                Debug.Log("[Checkpoint] Prompt UI Deactivated");
            }
        }
    }
}
