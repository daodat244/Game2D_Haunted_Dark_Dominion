using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [SerializeField] private AudioSource musicSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            if (Instance != this) Destroy(gameObject);
        }
    }

    // Phát nhạc mới
    public void PlayMusic(AudioClip newClip, float volume)
    {
        if (musicSource.clip == newClip) return;

        musicSource.clip = newClip;
        musicSource.volume = volume;
        musicSource.Play();
    }

    // Dừng nhạc
    public void StopMusic()
    {
        musicSource.Stop();
    }

    // Tạm dừng nhạc
    public void PauseMusic()
    {
        musicSource.Pause();
    }

    // Tiếp tục nhạc
    public void ResumeMusic()
    {
        musicSource.UnPause();
    }
}
