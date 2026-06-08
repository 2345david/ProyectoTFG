using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BankAccount : MonoBehaviour
{
    
    public float bank;

    public Text bankText;

    public static BankAccount Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void Start()
    {
        bankText.text = "x " + bank.ToString();
    }

    public void Money(float cashCollected)
    {
        bank += cashCollected;
        bankText.text = "x " + bank.ToString();
    }


}
