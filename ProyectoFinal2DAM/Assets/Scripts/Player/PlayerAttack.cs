using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public static PlayerAttack instance;

    public float damage = 2f; // Daño base del jugador

    private void Awake()
    {
        instance = this;
    }
}