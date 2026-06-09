using UnityEngine;

namespace EnemyScript
{
    /// <summary>
    /// Es como la "ficha de datos" de un enemigo. Aquí solo se guardan números y textos
    /// (vida, velocidad, daño...) que puedes cambiar desde el Inspector. Este script no
    /// hace acciones: otros scripts leen estos datos para mover o dañar al enemigo.
    /// </summary>
    public class Enemy : MonoBehaviour
    {
        // El nombre del enemigo. Solo sirve para identificarlo fácilmente.
        public string enemyNam;
        // La vida que le queda al enemigo. Baja cuando recibe golpes.
        public float healthPoints;
        // Lo rápido que se mueve el enemigo.
        public float speed;
        // Empujón horizontal (hacia los lados) que recibe al ser golpeado.
        public float knockbackForceX;
        // Empujón vertical (hacia arriba) que recibe al ser golpeado.
        public float knockbackForceY;
        // Cuánto daño le hace este enemigo al jugador.
        public float damageToGive;

        // Cuánta experiencia gana el jugador cuando este enemigo muere.
        public float ExperienceToGive;
    }
}
