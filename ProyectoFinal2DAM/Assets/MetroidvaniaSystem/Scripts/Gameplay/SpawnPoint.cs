using UnityEngine;

namespace Metroidvania.Gameplay
{
    public class SpawnPoint : MonoBehaviour
    {
        public string spawnTag;

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
            Gizmos.DrawLine(transform.position, transform.position + transform.up);
        }
    }
}
