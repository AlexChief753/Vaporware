using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("- - - - - - - - - Audio Sources - - - - - - - - -")]
    [SerializeField] private AudioSource backgroundMusicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("- - - - - - - - - Audio Clips - - - - - - - - -")]
    public AudioClip background;
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
        backgroundMusicSource.volume = 0.5f;
        backgroundMusicSource.clip = background;
        backgroundMusicSource.Play();
    }

    public void playSFX(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }
}
