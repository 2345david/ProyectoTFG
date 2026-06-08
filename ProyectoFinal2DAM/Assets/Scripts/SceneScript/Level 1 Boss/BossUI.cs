using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BossUI : MonoBehaviour
{

    public GameObject bossPanel;
    public Image healthBar;
    
    public static BossUI instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
            
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (bossPanel != null)
        {
            bossPanel.SetActive(false);
        }
    }

    // Update is called once per frame
    public void BossActivator()
    {
        if (bossPanel != null)
        {
            bossPanel.SetActive(true);
        }
    }
    
    public void BossDeactivator()
    {
        if (bossPanel != null)
        {
            bossPanel.SetActive(false);
        }

        if (isActiveAndEnabled && gameObject.activeInHierarchy)
        {
            StartCoroutine(BossDefeated());
        }
    }

    public void ResetBossUI()
    {
        StopAllCoroutines();
        if (bossPanel != null)
        {
            bossPanel.SetActive(false);
        }
        if (PlayerController.instance != null)
        {
            PlayerController.instance.enabled = true;
        }
    }

    IEnumerator BossDefeated()
    {
        if (PlayerController.instance == null)
        {
            yield break;
        }

        PlayerController.instance.enabled = false;
        yield return new WaitForSeconds(5f);
        if (PlayerController.instance != null)
        {
            PlayerController.instance.enabled = true;
        }
    }
}
