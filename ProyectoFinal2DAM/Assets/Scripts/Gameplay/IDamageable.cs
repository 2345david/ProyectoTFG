using UnityEngine;

namespace Metroidvania.Gameplay
{
    /// <summary>
    /// Es una "lista de reglas" (interfaz) que deben cumplir todos los objetos que puedan
    /// recibir daño, como los enemigos. Así el arma puede dañarlos sin saber qué son exactamente.
    /// </summary>
    public interface IDamageable
    {
        // Método para hacer daño. Todo objeto "dañable" debe tener esta acción.
        // amount = cuánto daño se hace.
        // hitDirection = desde dónde viene el golpe (sirve para empujar al objeto).
        void TakeDamage(float amount, Vector2 hitDirection);
    }
}
