using UnityEngine;

/// <summary>
/// Singleton de áudio. Crie um GameObject "AudioManager" na scene MenuPrincipal
/// e adicione este script — ele persiste em todas as fases.
/// Use AudioManager.Instance.PlaySFX(clip) e PlayMusica(clip) em qualquer lugar.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Volume Global")]
    [Range(0f, 1f)] public float volumeSFX    = 1f;
    [Range(0f, 1f)] public float volumeMusica  = 0.35f;

    [Header("Músicas (opcional)")]
    public AudioClip musicaMenu;
    public AudioClip musicaFase;
    public AudioClip musicaBoss;

    private AudioSource sfxSource;
    private AudioSource musicaSource;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // SFX: one-shot sem loop
        sfxSource             = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.loop        = false;

        // Música: loop contínuo
        musicaSource             = gameObject.AddComponent<AudioSource>();
        musicaSource.loop        = true;
        musicaSource.playOnAwake = false;
        musicaSource.volume      = volumeMusica;
    }

    // ── API pública ──────────────────────────────────────────────────

    /// <summary>Toca um efeito sonoro pontual (não bloqueia a música).</summary>
    public void PlaySFX(AudioClip clip, float escala = 1f)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip, Mathf.Clamp01(escala * volumeSFX));
    }

    /// <summary>Troca a música de fundo em loop.</summary>
    public void PlayMusica(AudioClip clip)
    {
        if (clip == null) { StopMusica(); return; }
        if (musicaSource.clip == clip && musicaSource.isPlaying) return;
        musicaSource.clip   = clip;
        musicaSource.volume = volumeMusica;
        musicaSource.Play();
    }

    public void StopMusica() => musicaSource.Stop();

    public void SetVolumeSFX(float v)
    {
        volumeSFX = Mathf.Clamp01(v);
    }

    public void SetVolumeMusica(float v)
    {
        volumeMusica            = Mathf.Clamp01(v);
        musicaSource.volume     = volumeMusica;
    }
}
