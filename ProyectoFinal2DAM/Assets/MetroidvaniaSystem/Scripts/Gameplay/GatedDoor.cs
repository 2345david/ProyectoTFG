using UnityEngine;
using Metroidvania.Core;

namespace Metroidvania.Gameplay
{
    public class GatedDoor : MonoBehaviour
    {
        public Ability requiredAbility;
        public bool isLocked = true;

        [Header("Visuals")]
        public GameObject lockVisual;

        private void Start()
        {
            UpdateDoorState();
        }

        public void UpdateDoorState()
        {
            if (ProgressionManager.Instance.HasAbility(requiredAbility))
            {
                isLocked = false;
                if (lockVisual != null) lockVisual.SetActive(false);
            }
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (isLocked && other.collider.CompareTag("Player"))
            {
                Debug.Log($"Door locked. Requires {requiredAbility}");
            }
        }
    }
}
