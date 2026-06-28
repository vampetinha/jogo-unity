using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class MusicaCena
{
#if UNITY_EDITOR
    public SceneAsset cena;
#endif
    [HideInInspector] public string nomeCena;
    public AudioClip  musica;
    [Range(0f, 1f)] public float volume = 1f;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    // ── Volumes globais ──────────────────────────────────────────────
    [Header("Volume Global")]
    [Range(0f, 1f)] public float volumeSFX     = 1f;
    [Range(0f, 1f)] public float volumeDisparo = 0.5f;
    [Range(0f, 1f)] public float volumeMusica  = 0.35f;

    // ── Música por cena ──────────────────────────────────────────────
    [Header("Música por Cena")]
    public MusicaCena[] musicasPorCena;

    // ── Loop de SFX (metralhadora) ───────────────────────────────────
    [Header("Loop de SFX")]
    [Range(0f, 0.3f)] public float fadeDuracaoLoop  = 0.06f;
    [Range(0f, 1f)]   public float offsetInicioLoop = 0f;

    // ── Privados ─────────────────────────────────────────────────────
    private AudioSource musicaSource;
    private AudioSource sfxSource;
    private AudioSource sfxLoopSource;

    private Coroutine fadeLoopCoroutine;
    private Coroutine loopManualCoroutine;

    private float  volumeCenaAtual = 1f;
    private string ultimaCena      = "";

    // ────────────────────────────────────────────────────────────────

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        sfxSource             = CriarAudioSource(loop: false);
        sfxLoopSource         = CriarAudioSource(loop: true);
        musicaSource          = CriarAudioSource(loop: true);
        musicaSource.volume   = volumeMusica;

        SincronizarNomesCena(); // garante nomeCena populado no editor
    }

    void OnValidate()
    {
        SincronizarNomesCena();
    }

    void Update()
    {
        // Troca música automaticamente ao mudar de cena
        string cenaAtual = SceneManager.GetActiveScene().name;
        if (cenaAtual != ultimaCena)
        {
            ultimaCena = cenaAtual;
            AplicarMusicaDaCena(cenaAtual);
        }

        // Mantém volume da música sempre sincronizado com os sliders
        if (musicaSource != null)
            musicaSource.volume = volumeMusica * volumeCenaAtual;
    }

    // ── Música ───────────────────────────────────────────────────────

    public void PlayMusica(AudioClip clip)
    {
        if (clip == null) { musicaSource.Stop(); return; }
        if (musicaSource.clip == clip && musicaSource.isPlaying) return;
        musicaSource.clip   = clip;
        musicaSource.volume = volumeMusica * volumeCenaAtual;
        musicaSource.Play();
    }

    public void StopMusica() => musicaSource.Stop();

    // ── SFX ──────────────────────────────────────────────────────────

    public void PlaySFX(AudioClip clip, float escala = 1f)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip, Mathf.Clamp01(escala * volumeSFX));
    }

    public void PlaySFXDisparo(AudioClip clip)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip, Mathf.Clamp01(volumeDisparo));
    }

    // ── SFX Loop ─────────────────────────────────────────────────────

    public void PlaySFXLoop(AudioClip clip, float delayEntreLoops = 0f)
    {
        if (clip == null) return;
        if (fadeLoopCoroutine   != null) StopCoroutine(fadeLoopCoroutine);
        if (loopManualCoroutine != null) StopCoroutine(loopManualCoroutine);

        if (delayEntreLoops > 0f)
        {
            sfxLoopSource.loop  = false;
            loopManualCoroutine = StartCoroutine(LoopComDelay(clip, delayEntreLoops));
        }
        else
        {
            sfxLoopSource.loop   = true;
            sfxLoopSource.clip   = clip;
            sfxLoopSource.volume = 0f;
            sfxLoopSource.Play();
            if (offsetInicioLoop > 0f)
                sfxLoopSource.time = Mathf.Min(offsetInicioLoop, clip.length - 0.01f);
            fadeLoopCoroutine = StartCoroutine(FadeLoop(0f, volumeDisparo, fadeDuracaoLoop));
        }
    }

    public void StopSFXLoop()
    {
        if (loopManualCoroutine != null) { StopCoroutine(loopManualCoroutine); loopManualCoroutine = null; }
        if (!sfxLoopSource.isPlaying) return;
        if (fadeLoopCoroutine != null) StopCoroutine(fadeLoopCoroutine);
        fadeLoopCoroutine = StartCoroutine(FadeOutAndStop());
    }

    // ── Setters de volume ────────────────────────────────────────────

    public void SetVolumeSFX(float v)     => volumeSFX     = Mathf.Clamp01(v);
    public void SetVolumeDisparo(float v) => volumeDisparo = Mathf.Clamp01(v);
    public void SetVolumeMusica(float v)  => volumeMusica  = Mathf.Clamp01(v);

    // ── Internos ─────────────────────────────────────────────────────

    private void AplicarMusicaDaCena(string nomeCena)
    {
        if (musicasPorCena == null) return;
        foreach (var e in musicasPorCena)
        {
            if (e == null || e.musica == null) continue;
            if (e.nomeCena != nomeCena) continue;
            volumeCenaAtual = e.volume;
            PlayMusica(e.musica);
            return;
        }
    }

    private void SincronizarNomesCena()
    {
#if UNITY_EDITOR
        if (musicasPorCena == null) return;
        foreach (var e in musicasPorCena)
            if (e != null && e.cena != null)
                e.nomeCena = e.cena.name;
#endif
    }

    private AudioSource CriarAudioSource(bool loop)
    {
        var src        = gameObject.AddComponent<AudioSource>();
        src.playOnAwake = false;
        src.loop        = loop;
        return src;
    }

    private IEnumerator LoopComDelay(AudioClip clip, float delay)
    {
        while (true)
        {
            sfxLoopSource.clip   = clip;
            sfxLoopSource.volume = volumeDisparo;
            sfxLoopSource.Play();
            yield return new WaitForSeconds(clip.length + delay);
        }
    }

    private IEnumerator FadeLoop(float de, float para, float duracao)
    {
        float t = 0f;
        while (t < duracao)
        {
            sfxLoopSource.volume = Mathf.Lerp(de, para, t / duracao);
            t += Time.unscaledDeltaTime;
            yield return null;
        }
        sfxLoopSource.volume = para;
    }

    private IEnumerator FadeOutAndStop()
    {
        float inicio = sfxLoopSource.volume;
        float t      = 0f;
        while (t < fadeDuracaoLoop)
        {
            sfxLoopSource.volume = Mathf.Lerp(inicio, 0f, t / fadeDuracaoLoop);
            t += Time.unscaledDeltaTime;
            yield return null;
        }
        sfxLoopSource.Stop();
        sfxLoopSource.volume = volumeDisparo;
    }
}
