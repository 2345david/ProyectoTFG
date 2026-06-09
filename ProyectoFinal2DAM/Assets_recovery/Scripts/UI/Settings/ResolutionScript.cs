using System.Collections.Generic;
using UnityEngine;

using TMPro;

public class ResolutionScript : MonoBehaviour
{
    public TMP_Dropdown Resolution_Dropdown;

    // Guardamos solo las resoluciones únicas (sin duplicados por frecuencia)
    List<Resolution> _uniqueResolutions = new List<Resolution>();

    void Start()
    {
        Resolution[] allResolutions = Screen.resolutions;

        Resolution_Dropdown.ClearOptions();

        List<string> options = new List<string>();
        int currentResolutionIndex = 0;

        foreach (Resolution res in allResolutions)
        {
            // Ignoramos duplicados de resolución con distinta frecuencia
            if (_uniqueResolutions.Exists(r => r.width == res.width && r.height == res.height))
                continue;

            _uniqueResolutions.Add(res);

            string option = res.width + " x " + res.height + "  " + res.refreshRateRatio + "Hz";
            options.Add(option);

            // Comparamos también la frecuencia para encontrar la resolución actual exacta
            if (res.width == Screen.currentResolution.width &&
                res.height == Screen.currentResolution.height &&
                res.refreshRateRatio.Equals(Screen.currentResolution.refreshRateRatio))
            {
                currentResolutionIndex = _uniqueResolutions.Count - 1;
            }
        }

        Resolution_Dropdown.AddOptions(options);
        Resolution_Dropdown.value = currentResolutionIndex;
        Resolution_Dropdown.RefreshShownValue();
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution res = _uniqueResolutions[resolutionIndex];

        // Aplicamos también la frecuencia de actualización
        Screen.SetResolution(
            res.width,
            res.height,
            Screen.fullScreenMode,
            res.refreshRateRatio
        );

        // Guardamos la selección
        PlayerPrefs.SetInt("ResolutionIndex", resolutionIndex);
    }

    public void ResetToDefault()
    {
        // Seleccionamos la resolución más alta — siempre es la última de la lista
        int highestIndex = _uniqueResolutions.Count - 1;

        Resolution_Dropdown.value = highestIndex;
        Resolution_Dropdown.RefreshShownValue();

        SetResolution(highestIndex);
    }
}