using UnityEngine;

namespace EnemyScript
{
    public class Enemy : MonoBehaviour
    {
        public string enemyNam;
        public float healthPoints;
        public float speed;
        public float knockbackForceX;
        public float knockbackForceY;
        public float damageToGive;

        public float ExperienceToGive;
    }
}
