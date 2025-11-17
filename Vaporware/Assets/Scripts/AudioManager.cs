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
<<<<<<< Updated upstream:Vaporware/Assets/AudioManager.cs
        backgroundMusicSource.volume = 0.5f;
        backgroundMusicSource.clip = background;
=======
        // Load saved volume or use default 0.5
        float musicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 0.5f);
        float sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 0.5f);

        backgroundMusicSource.volume = musicVolume;
        sfxSource.volume = sfxVolume;
        if (GameGrid.level % 4 == 0)
            backgroundMusicSource.clip = bossMusic;
        else
            backgroundMusicSource.clip = levelMusic;

>>>>>>> Stashed changes:Vaporware/Assets/Scripts/AudioManager.cs
        backgroundMusicSource.Play();
    }

    public void playSFX(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }
}
