using System.Collections.Generic;
using UnityEngine;

using TMPro;

public class ResolutionScript : MonoBehaviour
{
    public TMP_Dropdown Resolution_Dropdown;

    // Guardamos solo las resoluciones �nicas (sin duplicados por frecuencia)
    List<Resolution> _uniqueResolutions = new List<Resolution>();

    void Start()
    {
        Resolution[] allResolutions = Screen.resolutions;

        Resolution_Dropdown.ClearOptions();

        List<string> options = new List<string>();
        int currentResolutionIndex = 0;

        foreach (Resolution res in allResolutions)
        {
            // Ignoramos duplicados de resoluci�n con distinta frecuencia
            if (_uniqueResolutions.Exists(r => r.width == res.width && r.height == res.height))
                continue;

            _uniqueResolutions.Add(res);

            string option = res.width + " x " + res.height + "  " + res.refreshRateRatio + "Hz";
            options.Add(option);

            // Comparamos tambi�n la frecuencia para encontrar la resoluci�n actual exacta
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

    // Cambia la pantalla a la resolucion que el jugador elija en la lista.
    public void SetResolution(int resolutionIndex)
    {
        Resolution res = _uniqueResolutions[resolutionIndex];

        // Aplicamos tambi�n la frecuencia de actualizaci�n
        Screen.SetResolution(
            res.width,
            res.height,
            Screen.fullScreenMode,
            res.refreshRateRatio
        );

        // Guardamos la selecci�n
        PlayerPrefs.SetInt("ResolutionIndex", resolutionIndex);
    }

    // Vuelve a la resolucion mas alta disponible (la mejor calidad).
    public void ResetToDefault()
    {
        // Seleccionamos la resoluci�n m�s alta � siempre es la �ltima de la lista
        int highestIndex = _uniqueResolutions.Count - 1;

        Resolution_Dropdown.value = highestIndex;
        Resolution_Dropdown.RefreshShownValue();

        SetResolution(highestIndex);
    }
}