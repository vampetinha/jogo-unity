using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Gerencia o menu principal.
/// Não cria nenhum objeto — referencia os GameObjects já existentes na scene.
///
/// Hierarquia esperada na scene:
///   Canvas
///     ├── Background
///     ├── Image           (logo — animado)
///     └── ButtonsContainer
///           ├── btn-start
///           ├── btn-credits
///           └── btn-leave
///   PainelCreditos        (SetActive = false)
///   PainelConfig          (SetActive = false)
///         ├── SliderSFX
///         └── SliderMusica
///   EventSystem
///   MainMenuController
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("Configuração de Cena")]
    [Tooltip("Nome exato da scene da primeira fase (deve estar no Build Settings)")]
    public string nomePrimeiraFase = "FaseTerra";
    [Tooltip("Nome da dimensão exibido na transição pelo GameManager")]
    public string nomeDimensaoInicial = "TERRA";

    [Header("Referências — Botões")]
    public Button btnStart;
    public Sprite btnStartHover;

    public Button btnConfig;
    public Sprite btnConfigHover;

    public Button btnCredits;
    public Sprite btnCreditsHover;

    public Button btnLeave;
    public Sprite btnLeaveHover;

    [Header("Referências — Logo")]
    [Tooltip("RectTransform do GameObject com a imagem do nome do jogo")]
    public RectTransform tituloRect;

    [Header("Referências — Painel Créditos")]
    public GameObject painelCreditos;

    [Header("Referências — Painel Configurações")]
    public GameObject painelConfig;
    public Slider sliderSFX;
    public Slider sliderMusica;

    [Header("Botões bloqueados quando um painel está aberto")]
    [Tooltip("Arraste aqui todos os botões que devem ficar não-interativos com painel aberto")]
    public Button[] botoesParaBloqueio;

    [Header("Animação do Título")]
    public float tituloAmplitude = 5f;
    public float tituloVelocidade = 1.1f;

    // ── privados ──────────────────────────────────────────────────────
    private bool creditosVisiveis;
    private bool configVisiveis;
    private Vector2 tituloPosBase;

    // ─────────────────────────────────────────────────────────────────

    void Start()
    {
        Time.timeScale = 1f;

        // Reseta estado do GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.faseAtual = 1;
            GameManager.Instance.vidaAtual = GameManager.Instance.vidaMaxima;
            GameManager.Instance.moedas = 0;
            GameManager.Instance.armaAtual = null;
        }

        // Posição base do título para animação
        if (tituloRect != null)
            tituloPosBase = tituloRect.anchoredPosition;

        // Conecta botões do menu
        ConfigurarBotao(btnStart, btnStartHover, AoClicarJogar);
        ConfigurarBotao(btnConfig, btnConfigHover, AlternarConfig);
        ConfigurarBotao(btnCredits, btnCreditsHover, AlternarCreditos);
        ConfigurarBotao(btnLeave, btnLeaveHover, AoClicarSair);

        // Fecha painéis no início
        if (painelCreditos != null) painelCreditos.SetActive(false);
        if (painelConfig != null) painelConfig.SetActive(false);

        // Inicializa sliders com valores atuais do AudioManager
        if (AudioManager.Instance != null)
        {
            if (sliderSFX != null)
            {
                sliderSFX.value = AudioManager.Instance.volumeSFX;
                sliderSFX.onValueChanged.AddListener(AoMudarSFX);
            }
            if (sliderMusica != null)
            {
                sliderMusica.value = AudioManager.Instance.volumeMusica;
                sliderMusica.onValueChanged.AddListener(AoMudarMusica);
            }
        }

        StartCoroutine(PulsarTitulo());
    }

    void Update()
    {
        // ESC fecha qualquer painel aberto
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (creditosVisiveis) AlternarCreditos();
            else if (configVisiveis) AlternarConfig();
        }
    }

    // ── Animação do título ────────────────────────────────────────────

    private IEnumerator PulsarTitulo()
    {
        float t = 0f;
        while (tituloRect != null)
        {
            t += Time.deltaTime;
            float dy = Mathf.Sin(t * tituloVelocidade) * tituloAmplitude;
            tituloRect.anchoredPosition = tituloPosBase + new Vector2(0f, dy);
            yield return null;
        }
    }

    // ── Ações dos botões ─────────────────────────────────────────────

    public void AoClicarJogar()
    {
        Debug.Log("[Menu] AoClicarJogar chamado!");
        if (GameManager.Instance != null)
            GameManager.Instance.IrParaFase(nomePrimeiraFase, nomeDimensaoInicial);
        else
            SceneManager.LoadScene(nomePrimeiraFase);
    }

    public void AlternarCreditos()
    {
        Debug.Log("[Menu] AlternarCreditos chamado! Visivel: " + !creditosVisiveis);

        // Fecha config se estiver aberto
        if (configVisiveis) FecharConfig();

        creditosVisiveis = !creditosVisiveis;
        if (painelCreditos != null)
            painelCreditos.SetActive(creditosVisiveis);

        AtualizarBloqueio();
    }

    public void AlternarConfig()
    {
        Debug.Log("[Menu] AlternarConfig chamado! Visivel: " + !configVisiveis);

        // Fecha créditos se estiver aberto
        if (creditosVisiveis) FecharCreditos();

        configVisiveis = !configVisiveis;
        if (painelConfig != null)
            painelConfig.SetActive(configVisiveis);

        AtualizarBloqueio();
    }

    public void AoClicarSair()
    {
        Debug.Log("[Menu] AoClicarSair chamado!");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ── Sliders de áudio ─────────────────────────────────────────────

    private void AoMudarSFX(float valor)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetVolumeSFX(valor);
    }

    private void AoMudarMusica(float valor)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetVolumeMusica(valor);
    }

    // ── Helpers de painel ────────────────────────────────────────────

    private void FecharCreditos()
    {
        creditosVisiveis = false;
        if (painelCreditos != null) painelCreditos.SetActive(false);
    }

    private void FecharConfig()
    {
        configVisiveis = false;
        if (painelConfig != null) painelConfig.SetActive(false);
    }

    /// <summary>
    /// Bloqueia ou libera os botões configurados em botoesParaBloqueio
    /// sempre que qualquer painel estiver aberto.
    /// </summary>
    private void AtualizarBloqueio()
    {
        bool algumPainelAberto = creditosVisiveis || configVisiveis;
        foreach (var btn in botoesParaBloqueio)
        {
            if (btn != null)
                btn.interactable = !algumPainelAberto;
        }
    }

    // ── Configuração de botão com hover via EventTrigger ─────────────

    private void ConfigurarBotao(Button btn, Sprite hoverSprite,
                                  UnityEngine.Events.UnityAction acao)
    {
        if (btn == null) return;

        Image img = btn.GetComponent<Image>();
        Sprite spriteNormal = img != null ? img.sprite : null;

        // Clique via onClick nativo
        btn.onClick.AddListener(acao);

        if (img == null || spriteNormal == null || hoverSprite == null) return;

        btn.transition = Selectable.Transition.None;

        var et = btn.gameObject.GetComponent<EventTrigger>()
                 ?? btn.gameObject.AddComponent<EventTrigger>();

        var entrar = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        entrar.callback.AddListener((_) => img.sprite = hoverSprite);
        et.triggers.Add(entrar);

        var sair = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        sair.callback.AddListener((_) => img.sprite = spriteNormal);
        et.triggers.Add(sair);

        var down = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        down.callback.AddListener((_) => img.color = new Color(0.75f, 0.75f, 0.75f));
        et.triggers.Add(down);

        var up = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
        up.callback.AddListener((_) => { img.color = Color.white; img.sprite = hoverSprite; });
        et.triggers.Add(up);
    }
}