using System;
using EnemyScript;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class BossBehaviour : MonoBehaviour
{
    public Transform[] transforms;
    public GameObject flame;
    public GameObject muros;

    public float timeToShoot, countdown;
    public float timeToTP, countdownToTP;
    
    public float bossHealth, currentHealth;
    public Image healthImg;

    
    public void SetHealthBar(UnityEngine.UI.Image bar)
    {
        healthImg = bar;
        bossHealth = GetComponent<Enemy>().healthPoints;
    }

    private void Start()
    {
        transform.position = transforms[1].position;
        countdown = timeToShoot;
        countdownToTP = timeToTP;

        if (BossUI.instance != null)
        {
            healthImg = BossUI.instance.healthBar;
        }

        bossHealth = GetComponent<Enemy>().healthPoints;
    }

    public void ResetBoss()
    {
        transform.position = transforms[1].position;
        countdown = timeToShoot;
        countdownToTP = timeToTP;
        
        Enemy enemy = GetComponent<Enemy>();
        if (enemy != null)
        {
            // We need a way to restore the original health points.
            // Let's assume bossHealth was the initial health.
            enemy.healthPoints = bossHealth;
        }
        
        if (muros != null)
        {
            muros.SetActive(false);
            // If muros have FallingDoor script, reset them too.
            FallingDoor[] doors = muros.GetComponentsInChildren<FallingDoor>(true);
            foreach(var door in doors)
            {
                door.ResetDoor();
            }
        }
        
        gameObject.SetActive(false);
    }

    private void Update()
    {
        CountDowns();
        DamageBoss();
        BossScale();
    }

    public void CountDowns()
    {
        countdown -= Time.deltaTime;
        countdownToTP -= Time.deltaTime;
        
        if (countdown <= 0f)
        {
            ShootPlayer();
            countdown = timeToShoot;
        }

        if (countdownToTP <= 0f)
        {
            countdownToTP = timeToTP;
            Teleport();
        }
    }
    
    public void ShootPlayer()
    {
        GameObject spell = Instantiate(flame, transform.position, Quaternion.identity);
    }

    public void Teleport()
    {
        var initialPosition = Random.Range(0, transforms.Length);
        transform.position = transforms[initialPosition].position;
    }

    public void DamageBoss()
    {
        if (healthImg == null) return;
        
        currentHealth = GetComponent<Enemy>().healthPoints;
        healthImg.fillAmount = currentHealth / bossHealth;
    }

    private void OnDestroy()
    {
        if (BossUI.instance != null)
        {
            BossUI.instance.BossDeactivator();
        }

        if (muros != null)
        {
            muros.SetActive(false);
        }
    }

    public void BossScale()
    {
        if (transform.position.x > PlayerController.instance.transform.position.x)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
    }
}
