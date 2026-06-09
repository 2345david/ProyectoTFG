using UnityEngine;

namespace UI.Navigation
{
    /// <summary>
    /// Ayudante para cambiar de un menu a otro: enciende uno y apaga el otro.
    /// Asi pasamos de una pantalla a otra de forma sencilla.
    /// </summary>
    public static class UINavigator
    {
        // Muestra el menu "toEnable" y esconde el menu "toDisable".
        public static void SwitchMenu(GameObject toEnable, GameObject toDisable)
        {
            // Encendemos (mostramos) el menu que queremos ver.
            toEnable.SetActive(true);
            // Apagamos (escondemos) el menu anterior.
            toDisable.SetActive(false);
        }
    }
}
