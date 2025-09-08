using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Bootstrapper : MonoBehaviour
{
    [SerializeField] private SoundFXManager soundFXManagerPrefab;
    [SerializeField] private SoundMixerManager soundMixerManagerPrefab;
    private void Awake()
    {
        // Nếu chưa có SoundFXManager thì tạo mới
        if (SoundFXManager.Instance == null && soundFXManagerPrefab != null)
        {
            var sfx = Instantiate(soundFXManagerPrefab);
            DontDestroyOnLoad(sfx.gameObject);
        }

        // Nếu chưa có SoundMixerManager thì tạo mới
        if (SoundMixerManager.Instance == null && soundMixerManagerPrefab != null)
        {
            var mixer = Instantiate(soundMixerManagerPrefab);
            DontDestroyOnLoad(mixer.gameObject);
        }

        SceneManager.LoadScene("MainMenu");
    }
}
