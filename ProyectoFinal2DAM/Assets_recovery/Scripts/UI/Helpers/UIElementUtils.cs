namespace UI.Elements
{
    using UnityEngine.UI;

    public static class UIElementUtils
    {
        public static void SetButtonText(Button button, string text)
        {
            button.GetComponentInChildren<Text>().text = text;
        }
    }
}