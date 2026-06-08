using System;
using System.Collections;
using UnityEngine;

public class BossActivation : MonoBehaviour
{

    public GameObject bossGO;
    public GameObject muros;
    public string bossID = "Level1Boss";

    private bool isTriggered = false;

    private void Start()
    {
        if (CheckpointManager.instance != null && CheckpointManager.instance.IsBossDefeated(bossID))
        {
            if (bossGO != null) bossGO.SetActive(false);
            if (muros != null) muros.SetActive(false);
            gameObject.SetActive(false);
            return;
        }

        bossGO.SetActive(false);
        if (muros != null)
        {
            muros.SetActive(false);
        }
    }

    public void ResetTrigger()
    {
        isTriggered = false;
        gameObject.SetActive(true);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isTriggered) return;

        if (collision.gameObject.tag == "Player")
        {
            isTriggered = true;
            BossUI.instance.BossActivator();
            
            if (muros != null)
            {
                muros.SetActive(true);
            }

            StartCoroutine(WaitForBoss());
        }
    }

    IEnumerator WaitForBoss()
    {
        var currentSpeed = PlayerController.instance.speed;
        
        PlayerController.instance.speed = 0;
        bossGO.SetActive(true);
        
        BossBehaviour bb = bossGO.GetComponent<BossBehaviour>();
        if (bb != null)
        {
            bb.muros = muros;

            if (BossUI.instance != null)
            {
                BossUI.instance.BossActivator();
                bb.SetHealthBar(BossUI.instance.healthBar);
            }
        }

        yield return new WaitForSeconds(2f);
        PlayerController.instance.speed = currentSpeed;
        gameObject.SetActive(false);
    }
    
}
