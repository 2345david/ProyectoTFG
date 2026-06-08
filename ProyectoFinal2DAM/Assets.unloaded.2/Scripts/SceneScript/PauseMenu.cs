using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Runtime.CompilerServices;
using UnityEngine.Audio;
using System;

public class PauseMenu : MonoBehaviour
{
    
    public GameObject pauseMenu;
    [SerializeField] Sprite inventoryPotionSprite;
    [SerializeField] Sprite inventoryArrowSprite;
    [SerializeField] Sprite inventoryManaPotionSprite;
    bool isPaused;


    void Awake()
    {
        Time.timeScale = 1;
        pauseMenu.SetActive(false);
        isPaused = false;
        EnsurePlayerInventory();
        EnsurePauseInventoryUI();
    }

    void EnsurePlayerInventory()
    {
        if (FindAnyObjectByType<PlayerInventory>(FindObjectsInactive.Include) != null)
            return;
        var go = new GameObject("PlayerInventory");
        go.AddComponent<PlayerInventory>();
    }

    void EnsurePauseInventoryUI()
    {
        if (pauseMenu == null)
            return;
        var ui = pauseMenu.GetComponent<PauseInventoryUI>();
        if (ui == null)
            ui = pauseMenu.AddComponent<PauseInventoryUI>();
        ui.Configure(inventoryPotionSprite, inventoryArrowSprite, inventoryManaPotionSprite);
    }

    public void Update()
    {
        Pause();
    }


    public void Pause()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !isPaused)
        {
            Time.timeScale = 0;
            pauseMenu.SetActive(true);
            isPaused = true;
        }else if(Input.GetKeyDown(KeyCode.Escape) && isPaused)
        {
            Time.timeScale = 1;
            pauseMenu.SetActive(false);
            isPaused = false;
        }
      
    }


}