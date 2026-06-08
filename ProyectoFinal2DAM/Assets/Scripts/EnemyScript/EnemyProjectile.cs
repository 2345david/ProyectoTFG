using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

public class EnemyProjectile : MonoBehaviour
{
    public GameObject projectile;

    public float timeToShoot;
    private float shootCooldown;

    public bool freqShooter;
    public bool watcher;
    private bool hasShotWatcher = false;
    private bool playerDetected = false;
    private bool isKnockedBack = false;

    void Start()
    {
        shootCooldown = timeToShoot;
    }

    void Update()
    {
        isKnockedBack = GetComponent<EnemyMovements>()?.isKnockedBack ?? false; // Integrar con EnemyMovements

        if(freqShooter && playerDetected && !isKnockedBack)
        {
            shootCooldown -= Time.deltaTime;

            if(shootCooldown <= 0f)
            {
                Shoot();
                shootCooldown = timeToShoot;
            }
        }
    }

    public void OnPlayerEnter()
    {
        playerDetected = true;
        if (watcher && !isKnockedBack && !hasShotWatcher)
        {
            Shoot(); // Disparo inmediato para watcher, solo una vez por detección
            hasShotWatcher = true;
        }
        if (freqShooter)
        {
            shootCooldown = timeToShoot;
        }
    }

    public void OnPlayerExit()
    {
        playerDetected = false;
        hasShotWatcher = false;
    }




    public void Shoot()
    {
        GameObject cross = Instantiate(projectile, transform.position, Quaternion.identity);

        if(transform.localScale.x < 0)
        {
            cross.GetComponent<Rigidbody2D>().AddForce(new Vector2(300f, 0f), ForceMode2D.Force);
        }
        else
        {
            cross.GetComponent<Rigidbody2D>().AddForce(new Vector2(-300f, 0f), ForceMode2D.Force);
        }
    }
}