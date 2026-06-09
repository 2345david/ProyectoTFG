using UnityEngine;

/// <summary>
/// Solo guarda dos "pinturas" (materiales) para el efecto de destello cuando un enemigo
/// recibe daño. Este script no hace nada por sí mismo: solo deja a mano esas dos pinturas
/// para que otro script (por ejemplo EnemyHealth) las cambie y así el enemigo parpadee.
/// </summary>
public class Blink : MonoBehaviour
{
    // 'original': la pintura normal del dibujo del enemigo (su aspecto de siempre).
    // 'blink': la pintura del destello, que se pone un momentito al recibir un golpe.
    // Son públicas para poder ponerlas desde el Inspector y para que EnemyHealth las use.
    public Material original, blink;
}
