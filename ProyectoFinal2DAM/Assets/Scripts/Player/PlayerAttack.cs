using UnityEngine;

/// <summary>
/// Este script guarda cuánto daño hace el jugador al atacar.
/// Otros scripts (como el de atacar o el de subir de nivel) leen y cambian este número.
/// </summary>
public class PlayerAttack : MonoBehaviour
{
    // "instance" es un atajo para que otros scripts puedan leer el daño desde cualquier sitio.
    public static PlayerAttack instance;

    public float damage = 2f; // Cuánto daño hace el jugador con cada golpe.

    // Esto se ejecuta al crearse el objeto. Guardamos el atajo "instance" para que otros lo encuentren.
    private void Awake()
    {
        instance = this;
    }
}