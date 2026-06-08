using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Runtime.CompilerServices;

public class ExperienceScript : MonoBehaviour
{
    
    public Image expImage;
    public Text crrentLevelText;
    public  float currentExperience;
    public float expTNL;
    public int currentLevel;

    public static ExperienceScript instance;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }

    void Start()
    {
        currentLevel = 1;
        crrentLevelText.text = currentLevel.ToString();
        expImage.fillAmount = currentExperience / expTNL;
    }


    public void expModifer(float experience)
    {

        currentExperience += experience;
        expImage.fillAmount = currentExperience / expTNL;

        if(currentExperience >= expTNL)
        {
            expTNL = expTNL * 2;
            currentExperience = 0;
            PlayerHealth.instance.maxHealth += 10f;
            PlayerAttack.instance.damage += 10f;
            currentLevel++;
            crrentLevelText.text = currentLevel.ToString();

            print("Level Up");
        }
        
    }


}
