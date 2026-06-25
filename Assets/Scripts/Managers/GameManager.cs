using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Singleton persistente entre cenas.
/// Crie um GameObject vazio chamado "GameManager" na primeira cena
/// e adicione este script. Ele se mantém vivo em todas as fases.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // ── Estado do jogo ──────────────────────────────────────────────
    [Header("Estado do Jogo")]
    public int       faseAtual  = 1;
    public float     vidaAtual  = 100f;
    public float     vidaMaxima = 100f;
    public int       moedas     = 0;

    [Header("Arma da Fase")]
    [Tooltip("Arma que o jogador receberá ao entrar na próxima cena. Definida pelo PortalController.")]
    public WeaponData armaAtual;

    // ── Transição ───────────────────────────────────────────────────
    [Header("Transição de Cena")]
    [Tooltip("Duração do fade em segundos")]
    public float duracaoFade = 0.6f;

    [Header("Nome da Dimensão")]
    [Tooltip("Fonte para o texto do nome da fase (opcional)")]
    public TMP_FontAsset fonteTitulo;
    [Tooltip("Tempo em segundos que o nome fica na tela")]
    public float tempoNome = 2.5f;

    // ── Componentes de UI criados em tempo de execução ──────────────
    private Image    imagemFade;
    private TMP_Text textoNome;
    private string   nomeDimensaoPendente = "";
    private bool     transicionando       = false;

    // ────────────────────────────────────────────────────────────────

    void Awake()
    {
        // Padrão Singleton: garante uma única instância
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        CriarUITransicao();
    }

    void OnEnable()  => SceneManager.sceneLoaded += AoCarregarCena;
    void OnDisable() => SceneManager.sceneLoaded -= AoCarregarCena;

    // Chamado automaticamente pelo Unity quando uma nova cena termina de carregar
    private void AoCarregarCena(Scene cena, LoadSceneMode modo)
    {
        AplicarVidaNoPlayer();
        AplicarArmaNoPlayer();
        StartCoroutine(EntradaDeCena());
    }

    // ── API pública ─────────────────────────────────────────────────

    /// <summary>
    /// Chame este método para ir para a próxima fase com fade.
    /// nomeDimensao é o texto exibido ao entrar (ex: "MARTE").
    /// </summary>
    public void IrParaFase(string nomeScene, string nomeDimensao = "")
    {
        if (transicionando) return;
        if (string.IsNullOrEmpty(nomeScene))
        {
            Debug.LogWarning("GameManager.IrParaFase: nomeScene está vazio!");
            return;
        }

        SalvarVidaDoPlayer();
        nomeDimensaoPendente = nomeDimensao;
        faseAtual++;

        StartCoroutine(TransicaoParaFase(nomeScene));
    }

    /// <summary>Adiciona moedas ao total do jogador.</summary>
    public void AdicionarMoedas(int quantidade) => moedas += quantidade;

    /// <summary>Reinicia o jogo a partir da primeira fase.</summary>
    public void ReiniciarJogo(string nomePrimeiraScene)
    {
        faseAtual  = 1;
        vidaAtual  = vidaMaxima;
        moedas     = 0;
        IrParaFase(nomePrimeiraScene, "");
    }

    // ── Privados ────────────────────────────────────────────────────

    private IEnumerator TransicaoParaFase(string nomeScene)
    {
        transicionando = true;
        yield return StartCoroutine(Fade(0f, 1f));   // escurece
        SceneManager.LoadScene(nomeScene);
        // AoCarregarCena cuida do FadeIn
    }

    private IEnumerator EntradaDeCena()
    {
        // Exibe o nome da dimensão sobre o fundo preto
        if (!string.IsNullOrEmpty(nomeDimensaoPendente) && textoNome != null)
        {
            textoNome.text = nomeDimensaoPendente;
            textoNome.gameObject.SetActive(true);
        }

        yield return StartCoroutine(Fade(1f, 0f));   // clareia

        if (textoNome != null && textoNome.gameObject.activeSelf)
        {
            yield return new WaitForSecondsRealtime(tempoNome);
            textoNome.gameObject.SetActive(false);
            nomeDimensaoPendente = "";
        }

        transicionando = false;
    }

    private IEnumerator Fade(float de, float para)
    {
        float t = 0f;
        while (t < duracaoFade)
        {
            // Usa tempo não-escalado para que o fade funcione mesmo com TimeScale = 0
            t += Time.unscaledDeltaTime;
            SetAlpha(Mathf.Lerp(de, para, t / duracaoFade));
            yield return null;
        }
        SetAlpha(para);
    }

    private void SetAlpha(float a)
    {
        if (imagemFade == null) return;
        Color c = imagemFade.color;
        c.a = a;
        imagemFade.color = c;
    }

    private void SalvarVidaDoPlayer()
    {
        HealthSystem vida = BuscarVidaDoPlayer();
        if (vida == null) return;
        vidaAtual  = vida.VidaAtual;
        vidaMaxima = vida.VidaMaxima;
    }

    private void AplicarVidaNoPlayer()
    {
        HealthSystem vida = BuscarVidaDoPlayer();
        if (vida == null) return;
        vida.DefinirVida(vidaAtual, vidaMaxima);
    }

    private void AplicarArmaNoPlayer()
    {
        if (armaAtual == null) return;
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj == null) return;
        WeaponController arma = playerObj.GetComponent<WeaponController>();
        if (arma != null) arma.TrocarArma(armaAtual);
    }

    private HealthSystem BuscarVidaDoPlayer()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        return playerObj != null ? playerObj.GetComponent<HealthSystem>() : null;
    }

    // Cria o Canvas de fade e o texto de nome da dimensão em tempo de execução
    private void CriarUITransicao()
    {
        // Canvas
        GameObject canvasObj = new GameObject("_TransicaoCanvas");
        canvasObj.transform.SetParent(transform);
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode        = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        canvasObj.AddComponent<GraphicRaycaster>();

        // Painel preto de fade (começa opaco)
        GameObject painelObj = new GameObject("PainelFade");
        painelObj.transform.SetParent(canvasObj.transform, false);
        imagemFade = painelObj.AddComponent<Image>();
        imagemFade.color = Color.black;
        Esticar(imagemFade.rectTransform);

        // Texto do nome da dimensão
        GameObject textoObj = new GameObject("TextoNomeDimensao");
        textoObj.transform.SetParent(canvasObj.transform, false);
        textoNome            = textoObj.AddComponent<TextMeshProUGUI>();
        textoNome.alignment  = TextAlignmentOptions.Center;
        textoNome.fontSize   = 52f;
        textoNome.color      = Color.white;
        textoNome.fontStyle  = FontStyles.Bold;
        if (fonteTitulo != null) textoNome.font = fonteTitulo;

        RectTransform rtTexto = textoNome.rectTransform;
        rtTexto.anchorMin       = new Vector2(0f, 0.4f);
        rtTexto.anchorMax       = new Vector2(1f, 0.6f);
        rtTexto.sizeDelta       = Vector2.zero;
        rtTexto.anchoredPosition = Vector2.zero;
        textoObj.SetActive(false);
    }

    private static void Esticar(RectTransform rt)
    {
        rt.anchorMin        = Vector2.zero;
        rt.anchorMax        = Vector2.one;
        rt.sizeDelta        = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
    }
}
