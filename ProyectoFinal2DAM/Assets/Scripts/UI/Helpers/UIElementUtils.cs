namespace UI.Elements
{
    using TMPro;
    using UnityEngine.UI;

    /// <summary>
    /// Ayudante para cambiar el texto que se ve dentro de un boton.
    /// Sirve para no repetir el mismo codigo en muchos sitios.
    /// </summary>
    public static class UIElementUtils
    {
        // Pone el texto indicado dentro del boton, sea del tipo que sea.
        public static void SetButtonText(Button button, string text)
        {
            // Primero buscamos un texto moderno (TextMeshPro) dentro del boton.
            TMP_Text tmp = button.GetComponentInChildren<TMP_Text>();
            if (tmp != null)
            {
                // Si lo encontramos, le ponemos el texto y terminamos.
                tmp.text = text;
                return;
            }

            // Si no habia texto moderno, buscamos el texto clasico (Text) de Unity.
            Text legacy = button.GetComponentInChildren<Text>();
            if (legacy != null)
                legacy.text = text;
        }
    }
}