using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Cria e gerencia o menu principal em código.
/// Adicione este script em um GameObject "MenuController" na scene MenuPrincipal.
/// GameManager e HUDManager devem existir também nesta scene.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("Configuração")]
    [Tooltip("Nome da primeira fase do jogo")]
    public string nomePrimeiraFase      = "FaseTerra";
    [Tooltip("Nome da dimensão exibido na transição")]
    public string nomeDimensaoInicial   = "TERRA";
    [Tooltip("Título exibido na tela — use \\n para quebrar linha")]
    public string nomeTitulo            = "DIMENSÕES\nDO CAOS";
    [Tooltip("Subtítulo abaixo do nome")]
    public string subtitulo             = "Um roguelike de exploração dimensional";
    [Tooltip("Texto dos créditos (HTML-like do TMP aceito)")]
    [TextArea(4, 10)]
    public string textoCreditos =
        "<size=36><b>CRÉDITOS</b></size>\n\n" +
        "<size=22>Desenvolvido para a disciplina de Jogos Digitais\n\n" +
        "Programação e Design: [Seu Nome]\n\n" +
        "Engine: Unity 6\n\n" +
        "<i>Pressione ESC ou clique para fechar</i></size>";

    [Tooltip("Fonte opcional — usa padrão TMP se vazio")]
    public TMP_FontAsset fonte;

    private GameObject painelCreditos;
    private bool       creditosVisiveis;
    private GameObject tituloObj;

    // ────────────────────────────────────────────────────────────────

    void Start()
    {
        // Garante que o tempo está normal (pode vir zerado de um Game Over)
        Time.timeScale = 1f;

        // Reseta estado do GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.faseAtual  = 1;
            GameManager.Instance.vidaAtual  = GameManager.Instance.vidaMaxima;
            GameManager.Instance.moedas     = 0;
            GameManager.Instance.armaAtual  = null;
        }

        CriarUI();
    }

    void Update()
    {
        // ESC fecha os créditos
        if (creditosVisiveis && Input.GetKeyDown(KeyCode.Escape))
            AlternarCreditos();
    }

    // ── Construção da UI ─────────────────────────────────────────────

    private void CriarUI()
    {
        // Canvas do menu (sortingOrder 50 — abaixo do HUD e da transição)
        GameObject canvasObj = new GameObject("MenuCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        canvasObj.AddComponent<GraphicRaycaster>();

        CriarFundo(canvasObj);
        CriarTituloUI(canvasObj);
        CriarBotoes(canvasObj);
        CriarPainelCreditos(canvasObj);
    }

    private void CriarFundo(GameObject canvas)
    {
        GameObject bgObj = new GameObject("Fundo");
        bgObj.transform.SetParent(canvas.transform, false);
        Image bg = bgObj.AddComponent<Image>();
        bg.color = new Color(0.04f, 0.04f, 0.10f);
        Esticar(bg.rectTransform);
    }

    private void CriarTituloUI(GameObject canvas)
    {
        // Título principal
        tituloObj = new GameObject("Titulo");
        tituloObj.transform.SetParent(canvas.transform, false);
        TMP_Text t   = tituloObj.AddComponent<TextMeshProUGUI>();
        t.text        = nomeTitulo;
        t.fontSize    = 68f;
        t.color       = new Color(0.88f, 0.68f, 1f);
        t.fontStyle   = FontStyles.Bold;
        t.alignment   = TextAlignmentOptions.Center;
        if (fonte != null) t.font = fonte;

        RectTransform rt    = t.rectTransform;
        rt.anchorMin        = new Vector2(0.5f, 0.5f);
        rt.anchorMax        = new Vector2(0.5f, 0.5f);
        rt.pivot            = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0f, 150f);
        rt.sizeDelta        = new Vector2(800f, 220f);

        // Subtítulo
        GameObject subObj = new GameObject("Subtitulo");
        subObj.transform.SetParent(canvas.transform, false);
        TMP_Text sub   = subObj.AddComponent<TextMeshProUGUI>();
        sub.text        = subtitulo;
        sub.fontSize    = 20f;
        sub.color       = new Color(0.65f, 0.65f, 0.85f);
        sub.alignment   = TextAlignmentOptions.Center;
        if (fonte != null) sub.font = fonte;

        RectTransform rtS    = sub.rectTransform;
        rtS.anchorMin        = new Vector2(0.5f, 0.5f);
        rtS.anchorMax        = new Vector2(0.5f, 0.5f);
        rtS.pivot            = new Vector2(0.5f, 0.5f);
        rtS.anchoredPosition = new Vector2(0f, 75f);
        rtS.sizeDelta        = new Vector2(700f, 40f);

        StartCoroutine(PulsarTitulo());
    }

    private IEnumerator PulsarTitulo()
    {
        float t = 0f;
        while (tituloObj != null)
        {
            t += Time.deltaTime;
            float escala = 1f + Mathf.Sin(t * 1.1f) * 0.022f;
            tituloObj.transform.localScale = Vector3.one * escala;
            yield return null;
        }
    }

    private void CriarBotoes(GameObject canvas)
    {
        // Container vertical dos botões, centralizado
        GameObject container = new GameObject("ContainerBotoes");
        container.transform.SetParent(canvas.transform, false);
        RectTransform rt  = container.AddComponent<RectTransform>();
        rt.anchorMin      = new Vector2(0.5f, 0.5f);
        rt.anchorMax      = new Vector2(0.5f, 0.5f);
        rt.pivot          = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0f, -60f);
        rt.sizeDelta      = new Vector2(280f, 220f);

        CriarBotao(container, "JOGAR",     0f,    AoClicarJogar);
        CriarBotao(container, "CRÉDITOS", -80f,   AlternarCreditos);
        CriarBotao(container, "SAIR",     -160f,  () => Application.Quit());
    }

    private void AoClicarJogar()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.IrParaFase(nomePrimeiraFase, nomeDimensaoInicial);
        else
            SceneManager.LoadScene(nomePrimeiraFase);
    }

    private void CriarPainelCreditos(GameObject canvas)
    {
        painelCreditos = new GameObject("PainelCreditos");
        painelCreditos.transform.SetParent(canvas.transform, false);

        // Fundo escuro cobrindo toda a tela
        Image bg  = painelCreditos.AddComponent<Image>();
        bg.color  = new Color(0f, 0f, 0f, 0.90f);
        Esticar(bg.rectTransform);

        // Texto dos créditos
        GameObject textoObj = new GameObject("TextoCreditos");
        textoObj.transform.SetParent(painelCreditos.transform, false);
        TMP_Text texto  = textoObj.AddComponent<TextMeshProUGUI>();
        texto.text      = textoCreditos;
        texto.alignment = TextAlignmentOptions.Center;
        texto.color     = Color.white;
        if (fonte != null) texto.font = fonte;

        RectTransform rt    = texto.rectTransform;
        rt.anchorMin        = new Vector2(0.15f, 0.1f);
        rt.anchorMax        = new Vector2(0.85f, 0.9f);
        rt.sizeDelta        = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;

        // Botão invisível sobre o painel inteiro — clique qualquer fecha
        GameObject btnOverlay = new GameObject("BtnFechar");
        btnOverlay.transform.SetParent(painelCreditos.transform, false);
        Image btnImg = btnOverlay.AddComponent<Image>();
        btnImg.color = Color.clear;
        Button btn   = btnOverlay.AddComponent<Button>();
        btn.transition = Selectable.Transition.None;
        btn.onClick.AddListener(AlternarCreditos);
        Esticar(btnOverlay.GetComponent<RectTransform>());

        painelCreditos.SetActive(false);
    }

    private void AlternarCreditos()
    {
        creditosVisiveis = !creditosVisiveis;
        if (painelCreditos != null) painelCreditos.SetActive(creditosVisiveis);
    }

    // ── Helper botão ─────────────────────────────────────────────────

    private void CriarBotao(GameObject pai, string rotulo, float offsetY,
                             UnityEngine.Events.UnityAction acao)
    {
        GameObject obj = new GameObject("Btn_" + rotulo);
        obj.transform.SetParent(pai.transform, false);

        Image bg  = obj.AddComponent<Image>();
        bg.color  = new Color(0.12f, 0.08f, 0.22f);

        Button btn = obj.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.normalColor      = new Color(0.12f, 0.08f, 0.22f);
        cb.highlightedColor = new Color(0.30f, 0.15f, 0.50f);
        cb.pressedColor     = new Color(0.07f, 0.04f, 0.13f);
        btn.colors = cb;
        btn.onClick.AddListener(acao);

        RectTransform rt    = obj.GetComponent<RectTransform>();
        rt.anchorMin        = new Vector2(0f, 1f);
        rt.anchorMax        = new Vector2(1f, 1f);
        rt.pivot            = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, offsetY);
        rt.sizeDelta        = new Vector2(0f, 62f);

        // Texto
        GameObject textoObj = new GameObject("Texto");
        textoObj.transform.SetParent(obj.transform, false);
        TMP_Text t  = textoObj.AddComponent<TextMeshProUGUI>();
        t.text      = rotulo;
        t.fontSize  = 26f;
        t.color     = new Color(0.95f, 0.85f, 1f);
        t.alignment = TextAlignmentOptions.Center;
        t.fontStyle = FontStyles.Bold;
        if (fonte != null) t.font = fonte;
        Esticar(t.rectTransform);
    }

    private static void Esticar(RectTransform rt)
    {
        rt.anchorMin        = Vector2.zero;
        rt.anchorMax        = Vector2.one;
        rt.sizeDelta        = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
    }
}
