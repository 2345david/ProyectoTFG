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
        // Solo inicializamos si no ha sido cargado por el CheckpointManager
        if (currentLevel <= 0) currentLevel = 1;
        if (expTNL <= 0) expTNL = 100f; // Valor base por defecto
        
        crrentLevelText.text = currentLevel.ToString();
        expImage.fillAmount = expTNL > 0 ? currentExperience / expTNL : 0;
    }


    public void expModifer(float experience)
    {

        currentExperience += experience;
        
        while (currentExperience >= expTNL)
        {
            currentExperience -= expTNL;
            
            // Incrementar dificultad de nivel (30% más cada nivel)
            expTNL = Mathf.Floor(expTNL * 1.3f);
            
            // Subir estadísticas paulatinamente
            PlayerHealth.instance.maxHealth += 10f; // +10 de vida máxima
            PlayerAttack.instance.damage += 1f;     // +1 de daño base (antes era +10, muy brusco)
            
            // Curar al jugador un poco al subir de nivel (opcional pero común)
            PlayerHealth.instance.health = PlayerHealth.instance.maxHealth;

            currentLevel++;
            
            if (AudioManager.instance != null && AudioManager.instance.LvlUp != null)
                AudioManager.instance.PlayAudio(AudioManager.instance.LvlUp);

            Debug.Log($"Level Up! Now Level {currentLevel}. Next level needs {expTNL} XP.");
        }
        
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (crrentLevelText != null)
        {
            crrentLevelText.text = currentLevel.ToString();
        }
        
        if (expImage != null)
        {
            expImage.fillAmount = expTNL > 0 ? currentExperience / expTNL : 0;
        }
    }


}
