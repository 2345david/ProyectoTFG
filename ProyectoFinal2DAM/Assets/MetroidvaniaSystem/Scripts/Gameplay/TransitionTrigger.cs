using UnityEngine;
using Metroidvania.Core;

namespace Metroidvania.Gameplay
{
    public class TransitionTrigger : MonoBehaviour
    {
        public string targetScene;
        public string targetSpawnTag;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                WorldManager.Instance.TravelToBiome(targetScene, targetSpawnTag);
            }
        }
    }
}
