using UnityEngine;
using System.Collections.Generic;

namespace Metroidvania.Core
{
    [System.Serializable]
    public enum Ability
    {
        DoubleJump,
        Dash,
        WallJump,
        MorphBall,
        GrapplingHook,
        Swim
    }

    public class ProgressionManager : MonoBehaviour
    {
        public static ProgressionManager Instance { get; private set; }

        public HashSet<Ability> unlockedAbilities = new HashSet<Ability>();
        public List<string> collectedItems = new List<string>();

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public bool HasAbility(Ability ability) => unlockedAbilities.Contains(ability);

        public void UnlockAbility(Ability ability)
        {
            if (!unlockedAbilities.Contains(ability))
            {
                unlockedAbilities.Add(ability);
                Debug.Log($"Unlocked: {ability}");
            }
        }

        public void SaveGame()
        {
            // Placeholder for Json serialization
            string data = JsonUtility.ToJson(this);
            PlayerPrefs.SetString("SaveData", data);
            PlayerPrefs.Save();
        }

        public void LoadGame()
        {
            if (PlayerPrefs.HasKey("SaveData"))
            {
                string data = PlayerPrefs.GetString("SaveData");
                JsonUtility.FromJsonOverwrite(data, this);
            }
        }
    }
}
