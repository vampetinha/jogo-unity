using System.Collections;
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

    [Header("Fade do Loop (metralhadora)")]
    [Range(0f, 0.3f)] public float fadeDuracaoLoop  = 0.06f;
    [Tooltip("Segundos a pular no início do clipe de loop (remove silêncio inicial)")]
    [Range(0f, 1f)]   public float offsetInicioLoop = 0f;

    private AudioSource sfxSource;
    private AudioSource musicaSource;
    private AudioSource sfxLoopSource;
    private Coroutine   fadeLoopCoroutine;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // SFX: one-shot sem loop
        sfxSource             = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.loop        = false;

        // SFX em loop (ex: metralhadora)
        sfxLoopSource             = gameObject.AddComponent<AudioSource>();
        sfxLoopSource.playOnAwake = false;
        sfxLoopSource.loop        = true;

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

    /// <summary>Inicia um SFX em loop com fade in suave.</summary>
    public void PlaySFXLoop(AudioClip clip)
    {
        if (clip == null) return;
        if (sfxLoopSource.clip == clip && sfxLoopSource.isPlaying) return;

        if (fadeLoopCoroutine != null) StopCoroutine(fadeLoopCoroutine);

        sfxLoopSource.clip   = clip;
        sfxLoopSource.volume = 0f;
        sfxLoopSource.Play();
        if (offsetInicioLoop > 0f)
            sfxLoopSource.time = Mathf.Min(offsetInicioLoop, clip.length - 0.01f);

        fadeLoopCoroutine = StartCoroutine(FadeLoop(0f, volumeSFX, fadeDuracaoLoop));
    }

    /// <summary>Para o loop com fade out suave.</summary>
    public void StopSFXLoop()
    {
        if (!sfxLoopSource.isPlaying) return;

        if (fadeLoopCoroutine != null) StopCoroutine(fadeLoopCoroutine);
        fadeLoopCoroutine = StartCoroutine(FadeOutAndStop());
    }

    private IEnumerator FadeLoop(float de, float para, float duracao)
    {
        float t = 0f;
        while (t < duracao)
        {
            sfxLoopSource.volume = Mathf.Lerp(de, para, t / duracao);
            t += Time.deltaTime;
            yield return null;
        }
        sfxLoopSource.volume = para;
    }

    private IEnumerator FadeOutAndStop()
    {
        float volumeInicial = sfxLoopSource.volume;
        float t = 0f;
        while (t < fadeDuracaoLoop)
        {
            sfxLoopSource.volume = Mathf.Lerp(volumeInicial, 0f, t / fadeDuracaoLoop);
            t += Time.deltaTime;
            yield return null;
        }
        sfxLoopSource.Stop();
        sfxLoopSource.volume = volumeSFX;
    }

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
