using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

/// <summary>
/// Gerencia o menu principal.
/// Não cria nenhum objeto — referencia os GameObjects já existentes na scene.
/// 
/// Hierarquia esperada na scene:
///   Canvas
///     ├── Background      (Image)
///     ├── Image           (Image — logo/menu-name, será animado)
///     └── ButtonsContainer
///           ├── btn-start   (Button + Image)
///           ├── btn-credits (Button + Image)
///           └── btn-leave   (Button + Image)
///   EventSystem
///   MainMenuController   ← este script
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("Configuração de Cena")]
    [Tooltip("Nome exato da scene da primeira fase (deve estar no Build Settings)")]
    public string nomePrimeiraFase = "FaseTerra";
    [Tooltip("Nome da dimensão exibido na transição pelo GameManager")]
    public string nomeDimensaoInicial = "TERRA";

    [Header("Referências da Scene")]
    [Tooltip("GameObject 'Image' que contém o logo/nome do jogo — será animado")]
    public RectTransform tituloRect;

    [Tooltip("Botão INICIAR")]
    public Button btnStart;
    [Tooltip("Sprite hover do botão INICIAR")]
    public Sprite btnStartHover;

    [Tooltip("Botão CRÉDITOS")]
    public Button btnCredits;
    [Tooltip("Sprite hover do botão CRÉDITOS")]
    public Sprite btnCreditsHover;

    [Tooltip("Botão SAIR")]
    public Button btnLeave;
    [Tooltip("Sprite hover do botão SAIR")]
    public Sprite btnLeaveHover;

    [Tooltip("Painel de créditos (SetActive false na scene)")]
    public GameObject painelCreditos;

    [Header("Animação do Título")]
    [Tooltip("Pixels que o título sobe/desce")]
    public float tituloAmplitude = 5f;
    [Tooltip("Velocidade da flutuação")]
    public float tituloVelocidade = 1.1f;

    // ── privados ──────────────────────────────────────────────────────
    private bool creditosVisiveis;
    private Vector2 tituloPosBase;

    // ─────────────────────────────────────────────────────────────────

    void Start()
    {
        // Garante que o tempo está normal (pode vir zerado de um Game Over)
        Time.timeScale = 1f;

        // Reseta estado do GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.faseAtual = 1;
            GameManager.Instance.vidaAtual = GameManager.Instance.vidaMaxima;
            GameManager.Instance.moedas = 0;
            GameManager.Instance.armaAtual = null;
        }

        // Guarda posição base do título para a animação
        if (tituloRect != null)
            tituloPosBase = tituloRect.anchoredPosition;

        // Conecta os botões
        ConfigurarBotao(btnStart, btnStartHover, AoClicarJogar);
        ConfigurarBotao(btnCredits, btnCreditsHover, AlternarCreditos);
        ConfigurarBotao(btnLeave, btnLeaveHover, AoClicarSair);

        // Garante painel de créditos fechado
        if (painelCreditos != null)
            painelCreditos.SetActive(false);

        StartCoroutine(PulsarTitulo());
    }

    void Update()
    {
        // ESC fecha os créditos
        if (creditosVisiveis && Input.GetKeyDown(KeyCode.Escape))
            AlternarCreditos();
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
        creditosVisiveis = !creditosVisiveis;
        if (painelCreditos != null)
            painelCreditos.SetActive(creditosVisiveis);
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

    // ── Configuração de botão com hover via EventTrigger ─────────────

    private void ConfigurarBotao(Button btn, Sprite hoverSprite,
                                  UnityEngine.Events.UnityAction acao)
    {
        if (btn == null) return;

        Image img = btn.GetComponent<Image>();
        Sprite spriteNormal = img != null ? img.sprite : null;

        var et = btn.gameObject.GetComponent<EventTrigger>()
                 ?? btn.gameObject.AddComponent<EventTrigger>();

        // CLIQUE — via EventTrigger, não onClick
        var click = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
        click.callback.AddListener((_) => acao());
        et.triggers.Add(click);

        if (img == null || spriteNormal == null || hoverSprite == null) return;

        btn.transition = Selectable.Transition.None;

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