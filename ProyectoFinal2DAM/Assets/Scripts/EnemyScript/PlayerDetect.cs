using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

public class PlayerDetect : MonoBehaviour
{
    private EnemyProjectile parentProjectile;

    private void Start()
    {
        parentProjectile = GetComponentInParent<EnemyProjectile>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            parentProjectile.OnPlayerEnter();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            parentProjectile.OnPlayerExit();
        }
    }
}