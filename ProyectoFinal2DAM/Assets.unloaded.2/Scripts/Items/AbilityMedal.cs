using UnityEngine;

public class AbilityMedal : MonoBehaviour
{
    public enum AbilityType { DoubleJump, Dash }
    public AbilityType abilityToUnlock;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                if (abilityToUnlock == AbilityType.DoubleJump)
                {
                    player.hasDoubleJump = true;
                }
                else if (abilityToUnlock == AbilityType.Dash)
                {
                    player.hasDash = true;
                }

                if (CheckpointManager.instance != null)
                {
                    CheckpointManager.instance.SaveGame();
                }

                // Play sound if possible
                if (AudioManager.instance != null && AudioManager.instance.LvlUp != null)
                {
                    AudioManager.instance.PlayAudio(AudioManager.instance.LvlUp);
                }

                Destroy(gameObject);
            }
        }
    }
}
