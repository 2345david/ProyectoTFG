using UnityEngine;
using Metroidvania.Core;

namespace Metroidvania.Gameplay
{
    public class SavePoint : MonoBehaviour
    {
        public string savePointName;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                Save();
            }
        }

        public void Save()
        {
            Debug.Log($"Saving at {savePointName}");
            ProgressionManager.Instance.SaveGame();
            
            if (CheckpointManager.instance != null)
            {
                CheckpointManager.instance.SaveCheckpoint(transform.position);
            }
            // Optional: Restore health
        }
    }
}
