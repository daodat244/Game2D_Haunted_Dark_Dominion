using UnityEngine;
using UnityEngine.Audio;

public class SoundMixerManager : MonoBehaviour
{
    public static SoundMixerManager Instance { get; private set; }

    [SerializeField] private AudioMixer audioMixer;

    private void Awake()
    {
        if (audioMixer == null)
        {
            Debug.LogError("AudioMixer is not assigned!");
            return;
        }
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Lấy giá trị, nếu chưa có thì mặc định = 0.5 cho Master & Music, 1.0 cho SFX
        float master = PlayerPrefs.GetFloat("MasterVolume", 0.5f);
        float sfx = PlayerPrefs.GetFloat("SFXVolume", 1f);
        float music = PlayerPrefs.GetFloat("MusicVolume", 0.5f);

        SetMasterVolume(master);
        SetSoundFXVolume(sfx);
        SetMusicVolume(music);
    }


    public void SetMasterVolume(float level)
    {
        audioMixer.SetFloat("masterVolume", Mathf.Log10(Mathf.Clamp(level, 0.0001f, 1f)) * 20f);
        PlayerPrefs.SetFloat("MasterVolume", level);
    }

    public void SetSoundFXVolume(float level)
    {
        audioMixer.SetFloat("soundFXVolume", Mathf.Log10(Mathf.Clamp(level, 0.0001f, 1f)) * 20f);
        PlayerPrefs.SetFloat("SFXVolume", level);
    }

    public void SetMusicVolume(float level)
    {
        audioMixer.SetFloat("musicVolume", Mathf.Log10(Mathf.Clamp(level, 0.0001f, 1f)) * 20f);
        PlayerPrefs.SetFloat("MusicVolume", level);
    }
}
