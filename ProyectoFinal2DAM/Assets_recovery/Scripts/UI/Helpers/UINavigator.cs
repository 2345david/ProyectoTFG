using UnityEngine;

namespace UI.Navigation
{
    public static class UINavigator
    {
        public static void SwitchMenu(GameObject toEnable, GameObject toDisable)
        {
            toEnable.SetActive(true);
            toDisable.SetActive(false);
        }
    }
}
