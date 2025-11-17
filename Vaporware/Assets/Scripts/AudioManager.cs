using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("- - - - - - - - - Audio Sources - - - - - - - - -")]
    [SerializeField] private AudioSource backgroundMusicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("- - - - - - - - - Audio Clips - - - - - - - - -")]
    public AudioClip levelMusic;
    public AudioClip bossMusic;
    public AudioClip lineClear;
    public AudioClip pieceMove;
    public AudioClip rotate;
    public AudioClip hardDrop;
    public AudioClip menuClick;

    public static AudioManager instance;

    private const string MUSIC_VOLUME_KEY = "MusicVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {

        backgroundMusicSource.volume = musicVolume;
        sfxSource.volume = sfxVolume;

        // Load saved volume or use default 0.5
        float musicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 0.5f);
        float sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 0.5f);

        backgroundMusicSource.volume = musicVolume;
        sfxSource.volume = sfxVolume;
        if (GameGrid.level % 4 == 0)
            backgroundMusicSource.clip = bossMusic;
        else
            backgroundMusicSource.clip = levelMusic;

        backgroundMusicSource.Play();
    }

    public void playSFX(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }

    // Methods called by UI sliders
    public void SetMusicVolume(float value)
    {
        backgroundMusicSource.volume = value;
        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, value);
    }

    public void SetSFXVolume(float value)
    {
        sfxSource.volume = value;
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, value);
    }

    public float GetMusicVolume()
    {
        return backgroundMusicSource.volume;
    }

    public float GetSFXVolume()
    {
        return sfxSource.volume;
    }
}
