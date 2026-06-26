using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Gerencia todo o HUD do jogo: barra de vida, moedas, Game Over e Vitória.
/// Crie um GameObject "HUDManager" na primeira cena e adicione este script.
/// Ele persiste entre cenas — não precisa ser recriado.
/// </summary>
public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance { get; private set; }

    [Header("Configuração")]
    [Tooltip("Primeira fase do jogo (botão Reiniciar vai para cá)")]
    public string nomeCenaPrincipal = "FaseTerra";

    [Tooltip("Scene do menu principal (botão Menu Principal vai para cá)")]
    public string nomeSceneMenu = "MenuPrincipal";

    [Tooltip("Fonte usada em todos os textos do HUD (opcional)")]
    public TMP_FontAsset fonte;

    // ── Componentes criados em código ───────────────────────────────
    private Image    imagemFillVida;
    private TMP_Text textoMoedas;
    private TMP_Text textoMoedasVitoria;
    private GameObject painelGameOver;
    private GameObject painelVitoria;

    // Ícone da arma atual
    private Image           iconeArma;
    private WeaponController armaConectada;

    // Boss health bar
    private GameObject painelBarraBoss;
    private Image      imagemFillBoss;
    private HealthSystem vidaBossConectada;

    // Barra de calor (lança-chamas)
    private Image               imagemFillCalor;
    private TMP_Text            textoCalorStatus;
    private GameObject          containerCalor;
    private FlamethrowerAttack  lancaChamasConectado;

    // HUD jogável (oculto no menu principal)
    private GameObject containerHUD;

    private HealthSystem vidaConectada;

    // ────────────────────────────────────────────────────────────────

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        CriarHUD();
    }

    void OnEnable()  => SceneManager.sceneLoaded += AoCarregarCena;
    void OnDisable() => SceneManager.sceneLoaded -= AoCarregarCena;

    private void AoCarregarCena(Scene cena, LoadSceneMode modo)
    {
        // Garante que o tempo volta ao normal ao trocar de cena
        Time.timeScale = 1f;
        OcultarPaineis();
        // Aguarda um frame para o GameManager restaurar a vida antes de ler
        StartCoroutine(ConectarComPlayerNoProximoFrame());
    }

    private IEnumerator ConectarComPlayerNoProximoFrame()
    {
        yield return null;
        ConectarComPlayer();
    }

    // ── Conexão com o HealthSystem do Player ────────────────────────

    private void ConectarComPlayer()
    {
        // Desinscreve do player da cena anterior
        if (vidaConectada != null)
        {
            vidaConectada.aoReceberDano.RemoveListener(AtualizarVida);
            vidaConectada.aoMorrer.RemoveListener(MostrarGameOver);
        }

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj == null) return;

        vidaConectada = playerObj.GetComponent<HealthSystem>();
        if (vidaConectada == null) return;

        vidaConectada.aoReceberDano.AddListener(AtualizarVida);
        vidaConectada.aoMorrer.AddListener(MostrarGameOver);

        AtualizarVida(vidaConectada.VidaAtual, vidaConectada.VidaMaxima);
        if (containerHUD != null) containerHUD.SetActive(true);

        // Conecta ícone da arma ao WeaponController
        if (armaConectada != null)
            armaConectada.onSpriteArmaAlterado.RemoveListener(AtualizarIconeArma);

        armaConectada = playerObj.GetComponent<WeaponController>();
        if (armaConectada != null)
        {
            armaConectada.onSpriteArmaAlterado.AddListener(AtualizarIconeArma);
            AtualizarIconeArma(armaConectada.armaAtual?.sprite);
        }

        // Conecta barra de calor ao FlamethrowerAttack (filho do player)
        if (lancaChamasConectado != null)
            lancaChamasConectado.onCalorAlterado.RemoveListener(AtualizarCalor);

        lancaChamasConectado = playerObj.GetComponentInChildren<FlamethrowerAttack>();
        if (lancaChamasConectado != null)
        {
            lancaChamasConectado.onCalorAlterado.AddListener(AtualizarCalor);
            AtualizarCalor(lancaChamasConectado.CalorAtual, lancaChamasConectado.CalorMaximo);
            if (containerCalor != null) containerCalor.SetActive(true);
        }
        else if (containerCalor != null)
            containerCalor.SetActive(false);
    }

    private void AtualizarVida(float vidaAtual, float vidaMaxima)
    {
        if (imagemFillVida == null) return;
        float t = vidaAtual / Mathf.Max(1f, vidaMaxima);
        imagemFillVida.fillAmount = t;

        // Verde → Amarelo → Vermelho conforme a vida cai
        imagemFillVida.color = t > 0.5f
            ? Color.Lerp(Color.yellow, Color.green,  (t - 0.5f) * 2f)
            : Color.Lerp(Color.red,   Color.yellow,  t * 2f);
    }

    void Update()
    {
        // Mantém o contador de moedas sincronizado
        if (textoMoedas != null && GameManager.Instance != null)
            textoMoedas.text = $"Moedas: {GameManager.Instance.moedas}";
    }

    // ── API pública ──────────────────────────────────────────────────

    /// <summary>Exibe a tela de Game Over e pausa o jogo.</summary>
    public void MostrarGameOver()
    {
        AudioManager.Instance?.StopSFXLoop();
        AudioManager.Instance?.StopMusica();
        if (painelGameOver != null) painelGameOver.SetActive(true);
        Time.timeScale = 0f;
    }

    /// <summary>Exibe a tela de Vitória e pausa o jogo.</summary>
    public void MostrarVitoria()
    {
        if (textoMoedasVitoria != null && GameManager.Instance != null)
            textoMoedasVitoria.text = $"Moedas coletadas: {GameManager.Instance.moedas}";
        if (painelVitoria != null) painelVitoria.SetActive(true);
        Time.timeScale = 0f;
    }

    private void OcultarPaineis()
    {
        if (painelGameOver != null) painelGameOver.SetActive(false);
        if (painelVitoria  != null) painelVitoria.SetActive(false);
        if (containerHUD   != null) containerHUD.SetActive(false);
    }

    // ── Boss Health Bar ──────────────────────────────────────────────

    /// <summary>Exibe a barra de vida do boss e conecta ao HealthSystem dele.</summary>
    public void MostrarBarraBoss(string nomeBoss, HealthSystem vidaBoss)
    {
        if (painelBarraBoss != null)
        {
            // Atualiza nome
            TMP_Text txt = painelBarraBoss.GetComponentInChildren<TMP_Text>();
            if (txt != null) txt.text = nomeBoss;
            painelBarraBoss.SetActive(true);
        }

        vidaBossConectada = vidaBoss;
        vidaBoss.aoReceberDano.AddListener(AtualizarVidaBoss);
        AtualizarVidaBoss(vidaBoss.VidaAtual, vidaBoss.VidaMaxima);
    }

    /// <summary>Oculta a barra do boss (chamado pelo BossController ao morrer).</summary>
    public void OcultarBarraBoss()
    {
        if (vidaBossConectada != null)
            vidaBossConectada.aoReceberDano.RemoveListener(AtualizarVidaBoss);
        vidaBossConectada = null;
        if (painelBarraBoss != null) painelBarraBoss.SetActive(false);
    }

    private void AtualizarVidaBoss(float vidaAtual, float vidaMaxima)
    {
        if (imagemFillBoss == null) return;
        float t = vidaAtual / Mathf.Max(1f, vidaMaxima);
        imagemFillBoss.fillAmount = t;
        // Roxo → vermelho conforme HP cai
        imagemFillBoss.color = Color.Lerp(Color.red, new Color(0.7f, 0.2f, 1f), t);
    }

    // ── Criação do Canvas em código ──────────────────────────────────

    private void CriarHUD()
    {
        GameObject canvasObj = new GameObject("_HUDCanvas");
        canvasObj.transform.SetParent(transform);

        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;  // abaixo do canvas de transição (999)

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        canvasObj.AddComponent<GraphicRaycaster>();

        // Container do HUD jogável (oculto no menu principal)
        containerHUD = new GameObject("ContainerHUD");
        containerHUD.transform.SetParent(canvasObj.transform, false);
        RectTransform rtHUD = containerHUD.AddComponent<RectTransform>();
        rtHUD.anchorMin = Vector2.zero;
        rtHUD.anchorMax = Vector2.one;
        rtHUD.sizeDelta = Vector2.zero;
        rtHUD.anchoredPosition = Vector2.zero;
        containerHUD.SetActive(false); // fica oculto até achar o Player

        CriarBarraDeVida(containerHUD);
        CriarIconeArma(containerHUD);
        CriarContadorMoedas(containerHUD);
        CriarBarraDeCalor(containerHUD);
        CriarBarraBoss(canvasObj);
        CriarPainelGameOver(canvasObj);
        CriarPainelVitoria(canvasObj);
    }

    // ── Ícone da arma ───────────────────────────────────────────────

    private void CriarIconeArma(GameObject pai)
    {
        GameObject container = new GameObject("ContainerArma");
        container.transform.SetParent(pai.transform, false);
        RectTransform rt     = container.AddComponent<RectTransform>();
        rt.anchorMin         = new Vector2(0f, 1f);
        rt.anchorMax         = new Vector2(0f, 1f);
        rt.pivot             = new Vector2(0f, 1f);
        rt.anchoredPosition  = new Vector2(20f, -52f); // logo abaixo da barra de vida
        rt.sizeDelta         = new Vector2(44f, 44f);

        // Fundo escuro
        GameObject bgObj = new GameObject("Fundo");
        bgObj.transform.SetParent(container.transform, false);
        Image bg = bgObj.AddComponent<Image>();
        bg.color = new Color(0.08f, 0.08f, 0.08f, 0.75f);
        Esticar(bg.rectTransform);

        // Ícone da arma (imagem com preserveAspect)
        GameObject iconObj = new GameObject("IconeArma");
        iconObj.transform.SetParent(container.transform, false);
        iconeArma                  = iconObj.AddComponent<Image>();
        iconeArma.color            = Color.white;
        iconeArma.preserveAspect   = true;
        iconeArma.enabled          = false; // fica oculto até ter sprite
        RectTransform rtI          = iconeArma.rectTransform;
        rtI.anchorMin              = new Vector2(0.1f, 0.1f);
        rtI.anchorMax              = new Vector2(0.9f, 0.9f);
        rtI.sizeDelta              = Vector2.zero;
        rtI.anchoredPosition       = Vector2.zero;
    }

    private void AtualizarIconeArma(Sprite sprite)
    {
        if (iconeArma == null) return;
        iconeArma.sprite  = sprite;
        iconeArma.enabled = sprite != null;
    }

    // ── Barra de calor (lança-chamas) ───────────────────────────────

    private void CriarBarraDeCalor(GameObject pai)
    {
        containerCalor = new GameObject("ContainerCalor");
        containerCalor.transform.SetParent(pai.transform, false);
        RectTransform rtC    = containerCalor.AddComponent<RectTransform>();
        rtC.anchorMin        = new Vector2(1f, 0f);
        rtC.anchorMax        = new Vector2(1f, 0f);
        rtC.pivot            = new Vector2(1f, 0f);
        rtC.anchoredPosition = new Vector2(-20f, 20f);
        rtC.sizeDelta        = new Vector2(200f, 28f);

        // Label acima da barra
        GameObject labelObj  = new GameObject("LabelCalor");
        labelObj.transform.SetParent(containerCalor.transform, false);
        textoCalorStatus     = labelObj.AddComponent<TextMeshProUGUI>();
        textoCalorStatus.text      = "PRONTO";
        textoCalorStatus.fontSize  = 13f;
        textoCalorStatus.color     = new Color(1f, 0.55f, 0f);
        textoCalorStatus.alignment = TextAlignmentOptions.Right;
        if (fonte != null) textoCalorStatus.font = fonte;
        RectTransform rtL    = textoCalorStatus.rectTransform;
        rtL.anchorMin        = new Vector2(0f, 1f);
        rtL.anchorMax        = new Vector2(1f, 1f);
        rtL.pivot            = new Vector2(0.5f, 0f);
        rtL.anchoredPosition = new Vector2(0f, 2f);
        rtL.sizeDelta        = new Vector2(0f, 18f);

        // Fundo
        GameObject bgObj = new GameObject("CalorFundo");
        bgObj.transform.SetParent(containerCalor.transform, false);
        Image bg = bgObj.AddComponent<Image>();
        bg.color = new Color(0.15f, 0.05f, 0f);
        Esticar(bg.rectTransform);

        // Fill
        GameObject fillObj       = new GameObject("CalorFill");
        fillObj.transform.SetParent(containerCalor.transform, false);
        imagemFillCalor            = fillObj.AddComponent<Image>();
        imagemFillCalor.color      = new Color(1f, 0.55f, 0f);
        imagemFillCalor.type       = Image.Type.Filled;
        imagemFillCalor.fillMethod = Image.FillMethod.Horizontal;
        imagemFillCalor.fillOrigin = 0;
        imagemFillCalor.fillAmount = 1f;
        Esticar(imagemFillCalor.rectTransform);

        containerCalor.SetActive(false);
    }

    private void AtualizarCalor(float atual, float maximo)
    {
        if (imagemFillCalor == null || lancaChamasConectado == null) return;

        float t = maximo > 0f ? atual / maximo : 0f;
        imagemFillCalor.fillAmount = t;

        if (lancaChamasConectado.EmRecarga)
        {
            imagemFillCalor.color = new Color(0.4f, 0.4f, 0.4f);
            if (textoCalorStatus != null) textoCalorStatus.text = "SUPERAQUECIDO";
        }
        else if (lancaChamasConectado.Ativo)
        {
            imagemFillCalor.color = new Color(1f, 0.25f, 0f);
            if (textoCalorStatus != null) textoCalorStatus.text = "ATIVO";
        }
        else if (t <= 0.3f)
        {
            imagemFillCalor.color = new Color(1f, 0.1f, 0.1f);
            if (textoCalorStatus != null) textoCalorStatus.text = "RECARREGANDO";
        }
        else
        {
            imagemFillCalor.color = new Color(1f, 0.55f, 0f);
            if (textoCalorStatus != null) textoCalorStatus.text = t >= 1f ? "PRONTO" : "RECARREGANDO";
        }
    }

    // ── Barra de vida ────────────────────────────────────────────────

    private void CriarBarraDeVida(GameObject canvas)
    {
        // Container ancorado no canto superior esquerdo
        GameObject container = new GameObject("ContainerVida");
        container.transform.SetParent(canvas.transform, false);
        RectTransform rtC = container.AddComponent<RectTransform>();
        rtC.anchorMin        = new Vector2(0f, 1f);
        rtC.anchorMax        = new Vector2(0f, 1f);
        rtC.pivot            = new Vector2(0f, 1f);
        rtC.anchoredPosition = new Vector2(20f, -15f);
        rtC.sizeDelta        = new Vector2(220f, 30f);

        // Ícone ♥
        GameObject iconObj  = new GameObject("Icone");
        iconObj.transform.SetParent(container.transform, false);
        TMP_Text icone      = iconObj.AddComponent<TextMeshProUGUI>();
        icone.text          = "♥";
        icone.fontSize      = 26f;
        icone.color         = new Color(1f, 0.3f, 0.3f);
        icone.alignment     = TextAlignmentOptions.Center;
        if (fonte != null) icone.font = fonte;
        RectTransform rtIcon   = icone.rectTransform;
        rtIcon.anchorMin       = new Vector2(0f, 0f);
        rtIcon.anchorMax       = new Vector2(0f, 1f);
        rtIcon.pivot           = new Vector2(0f, 0.5f);
        rtIcon.sizeDelta       = new Vector2(30f, 0f);
        rtIcon.anchoredPosition = Vector2.zero;

        // Fundo escuro da barra
        GameObject bgObj = new GameObject("BarraFundo");
        bgObj.transform.SetParent(container.transform, false);
        Image bg = bgObj.AddComponent<Image>();
        bg.color = new Color(0.18f, 0.04f, 0.04f);
        RectTransform rtBg = bg.rectTransform;
        rtBg.anchorMin = Vector2.zero;
        rtBg.anchorMax = Vector2.one;
        rtBg.offsetMin = new Vector2(36f, 4f);
        rtBg.offsetMax = new Vector2(-4f, -4f);

        // Fill da vida (imagem preenchida horizontalmente)
        GameObject fillObj = new GameObject("BarraFill");
        fillObj.transform.SetParent(container.transform, false);
        imagemFillVida            = fillObj.AddComponent<Image>();
        imagemFillVida.color      = Color.green;
        imagemFillVida.type       = Image.Type.Filled;
        imagemFillVida.fillMethod = Image.FillMethod.Horizontal;
        imagemFillVida.fillOrigin = 0;   // esquerda → direita
        imagemFillVida.fillAmount = 1f;
        RectTransform rtFill = imagemFillVida.rectTransform;
        rtFill.anchorMin = Vector2.zero;
        rtFill.anchorMax = Vector2.one;
        rtFill.offsetMin = new Vector2(36f, 4f);
        rtFill.offsetMax = new Vector2(-4f, -4f);
    }

    // ── Contador de moedas ───────────────────────────────────────────

    private void CriarContadorMoedas(GameObject canvas)
    {
        GameObject obj      = new GameObject("TextoMoedas");
        obj.transform.SetParent(canvas.transform, false);
        textoMoedas         = obj.AddComponent<TextMeshProUGUI>();
        textoMoedas.text    = "Moedas: 0";
        textoMoedas.fontSize = 22f;
        textoMoedas.color   = new Color(1f, 0.88f, 0.15f);
        textoMoedas.alignment = TextAlignmentOptions.Right;
        if (fonte != null) textoMoedas.font = fonte;

        RectTransform rt  = textoMoedas.rectTransform;
        rt.anchorMin      = new Vector2(1f, 1f);
        rt.anchorMax      = new Vector2(1f, 1f);
        rt.pivot          = new Vector2(1f, 1f);
        rt.anchoredPosition = new Vector2(-20f, -20f);
        rt.sizeDelta      = new Vector2(200f, 38f);
    }

    // ── Painel de Game Over ──────────────────────────────────────────

    private void CriarPainelGameOver(GameObject canvas)
    {
        painelGameOver = CriarOverlay(canvas, "PainelGameOver");

        AdicionarTexto(painelGameOver, "GAME OVER",
            new Color(1f, 0.15f, 0.15f), 72f, new Vector2(0f, 90f));

        CriarBotao(painelGameOver, "Reiniciar", new Vector2(0f, -10f), () =>
        {
            Time.timeScale = 1f;
            if (GameManager.Instance != null)
                GameManager.Instance.ReiniciarJogo(nomeCenaPrincipal);
            else
                SceneManager.LoadScene(nomeCenaPrincipal);
        });

        CriarBotao(painelGameOver, "Menu Principal", new Vector2(0f, -80f), () =>
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(nomeSceneMenu);
        });

        painelGameOver.SetActive(false);
    }

    // ── Painel de Vitória ────────────────────────────────────────────

    private void CriarPainelVitoria(GameObject canvas)
    {
        painelVitoria = CriarOverlay(canvas, "PainelVitoria");

        AdicionarTexto(painelVitoria, "VITÓRIA!",
            new Color(1f, 0.85f, 0.1f), 72f, new Vector2(0f, 110f));

        // Texto de moedas — guardamos a referência para atualizar ao exibir
        GameObject moedasObj = AdicionarTexto(painelVitoria, "Moedas coletadas: 0",
            Color.white, 28f, new Vector2(0f, 30f));
        textoMoedasVitoria = moedasObj.GetComponent<TMP_Text>();

        CriarBotao(painelVitoria, "Menu Principal", new Vector2(0f, -60f), () =>
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(nomeSceneMenu);
        });

        painelVitoria.SetActive(false);
    }

    // ── Barra do Boss (centro inferior) ─────────────────────────────

    private void CriarBarraBoss(GameObject canvas)
    {
        painelBarraBoss = new GameObject("ContainerBoss");
        painelBarraBoss.transform.SetParent(canvas.transform, false);
        RectTransform rtP    = painelBarraBoss.AddComponent<RectTransform>();
        rtP.anchorMin        = new Vector2(0.5f, 0f);
        rtP.anchorMax        = new Vector2(0.5f, 0f);
        rtP.pivot            = new Vector2(0.5f, 0f);
        rtP.anchoredPosition = new Vector2(0f, 18f);
        rtP.sizeDelta        = new Vector2(420f, 40f);

        // Nome do boss (acima da barra)
        GameObject nomeObj = new GameObject("TextoNomeBoss");
        nomeObj.transform.SetParent(painelBarraBoss.transform, false);
        TMP_Text nomeTxt   = nomeObj.AddComponent<TextMeshProUGUI>();
        nomeTxt.text       = "";
        nomeTxt.fontSize   = 16f;
        nomeTxt.color      = new Color(0.9f, 0.7f, 1f);
        nomeTxt.alignment  = TextAlignmentOptions.Center;
        nomeTxt.fontStyle  = FontStyles.Bold;
        if (fonte != null) nomeTxt.font = fonte;
        RectTransform rtN    = nomeTxt.rectTransform;
        rtN.anchorMin        = new Vector2(0f, 1f);
        rtN.anchorMax        = new Vector2(1f, 1f);
        rtN.pivot            = new Vector2(0.5f, 0f);
        rtN.anchoredPosition = new Vector2(0f, 2f);
        rtN.sizeDelta        = new Vector2(0f, 22f);

        // Fundo escuro da barra
        GameObject bgObj = new GameObject("BarraFundo");
        bgObj.transform.SetParent(painelBarraBoss.transform, false);
        Image bg = bgObj.AddComponent<Image>();
        bg.color = new Color(0.1f, 0.04f, 0.14f);
        Esticar(bg.rectTransform);

        // Fill roxo -> vermelho
        GameObject fillObj       = new GameObject("BarraFill");
        fillObj.transform.SetParent(painelBarraBoss.transform, false);
        imagemFillBoss            = fillObj.AddComponent<Image>();
        imagemFillBoss.color      = new Color(0.7f, 0.2f, 1f);
        imagemFillBoss.type       = Image.Type.Filled;
        imagemFillBoss.fillMethod = Image.FillMethod.Horizontal;
        imagemFillBoss.fillOrigin = 0;
        imagemFillBoss.fillAmount = 1f;
        Esticar(imagemFillBoss.rectTransform);

        painelBarraBoss.SetActive(false);
    }

    // ── Helpers de construção de UI ──────────────────────────────────

    private GameObject CriarOverlay(GameObject canvas, string nome)
    {
        GameObject painel = new GameObject(nome);
        painel.transform.SetParent(canvas.transform, false);
        Image bg  = painel.AddComponent<Image>();
        bg.color  = new Color(0f, 0f, 0f, 0.80f);
        Esticar(bg.rectTransform);
        return painel;
    }

    private GameObject AdicionarTexto(GameObject pai, string conteudo,
                                       Color cor, float tamanho, Vector2 posicao)
    {
        GameObject obj = new GameObject("Texto_" + conteudo);
        obj.transform.SetParent(pai.transform, false);
        TMP_Text t     = obj.AddComponent<TextMeshProUGUI>();
        t.text         = conteudo;
        t.fontSize     = tamanho;
        t.color        = cor;
        t.fontStyle    = FontStyles.Bold;
        t.alignment    = TextAlignmentOptions.Center;
        if (fonte != null) t.font = fonte;

        RectTransform rt    = t.rectTransform;
        rt.anchorMin        = new Vector2(0.5f, 0.5f);
        rt.anchorMax        = new Vector2(0.5f, 0.5f);
        rt.pivot            = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = posicao;
        rt.sizeDelta        = new Vector2(700f, 100f);

        return obj;
    }

    private void CriarBotao(GameObject pai, string rotulo,
                             Vector2 posicao, UnityEngine.Events.UnityAction acao)
    {
        GameObject obj = new GameObject("Botao_" + rotulo);
        obj.transform.SetParent(pai.transform, false);

        Image bg  = obj.AddComponent<Image>();
        bg.color  = new Color(0.13f, 0.13f, 0.15f);

        Button btn = obj.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.highlightedColor = new Color(0.28f, 0.28f, 0.35f);
        cb.pressedColor     = new Color(0.08f, 0.08f, 0.10f);
        btn.colors = cb;
        btn.onClick.AddListener(acao);

        RectTransform rt    = obj.GetComponent<RectTransform>();
        rt.anchorMin        = new Vector2(0.5f, 0.5f);
        rt.anchorMax        = new Vector2(0.5f, 0.5f);
        rt.pivot            = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = posicao;
        rt.sizeDelta        = new Vector2(240f, 55f);

        // Texto do botão
        GameObject textoObj = new GameObject("Texto");
        textoObj.transform.SetParent(obj.transform, false);
        TMP_Text t     = textoObj.AddComponent<TextMeshProUGUI>();
        t.text         = rotulo;
        t.fontSize     = 24f;
        t.color        = Color.white;
        t.alignment    = TextAlignmentOptions.Center;
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
