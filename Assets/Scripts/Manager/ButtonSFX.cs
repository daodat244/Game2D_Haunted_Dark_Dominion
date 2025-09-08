using UnityEngine;

public class ButtonSFX : MonoBehaviour
{
    [SerializeField] private AudioClip clickSound;
    public void PlayClickSound()
    {
        if (clickSound != null && SoundFXManager.Instance != null)
        {
            SoundFXManager.Instance.PlaySoundFXClip(clickSound, transform, 1f);
        }
    }
}
