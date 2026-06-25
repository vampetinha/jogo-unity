using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Exibe um painel de diálogo com efeito máquina de escrever ao iniciar a fase.
/// Adicione a qualquer GameObject de cada cena de fase.
/// Clique ou tecla qualquer para pular/fechar.
/// </summary>
public class IntroDialogue : MonoBehaviour
{
    [Header("Texto")]
    [TextArea(3, 8)]
    public string textoDialogo = "Texto de introdução da fase...";

    [Tooltip("Caracteres por segundo")]
    public float velocidadeDigitacao = 45f;

    [Tooltip("Segundos a aguardar antes de aparecer (deve ser >= duração do fade de entrada)")]
    public float atrasoInicial = 0.8f;

    [Tooltip("0 = não fecha sozinho; > 0 = fecha após X segundos do fim da digitação")]
    public float tempoAutoFechar = 0f;

    [Header("Aparência")]
    public TMP_FontAsset fonte;
    public Color corFundo  = new Color(0f, 0f, 0f, 0.82f);
    public Color corTexto  = Color.white;
    public float alturaPanel = 190f;

    [Header("Áudio (opcional)")]
    public AudioClip somDigitacao;

    // ────────────────────────────────────────────────────────────────
    private TMP_Text  textoUI;
    private GameObject canvasDialogo;
    private bool       dialogoFinalizado = false;
    private Coroutine  digitacaoCoroutine;

    void Start()
    {
        StartCoroutine(IniciarComAtraso());
    }

    void Update()
    {
        if (!dialogoFinalizado) return;
        if (Input.anyKeyDown || Input.GetMouseButtonDown(0))
            Fechar();
    }

    private IEnumerator IniciarComAtraso()
    {
        yield return new WaitForSecondsRealtime(atrasoInicial);
        CriarUI();
        Time.timeScale = 0f;
        digitacaoCoroutine = StartCoroutine(Digitar());
    }

    // ── Construção da UI ─────────────────────────────────────────────

    private void CriarUI()
    {
        // Canvas (sortingOrder 200 — acima do HUD, abaixo da transição)
        canvasDialogo = new GameObject("DialogoCanvas");
        Canvas canvas = canvasDialogo.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;

        CanvasScaler sc = canvasDialogo.AddComponent<CanvasScaler>();
        sc.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        sc.referenceResolution = new Vector2(1280f, 720f);
        canvasDialogo.AddComponent<GraphicRaycaster>();

        // Painel inferior
        GameObject painelObj = new GameObject("PainelDialogo");
        painelObj.transform.SetParent(canvasDialogo.transform, false);
        Image bg  = painelObj.AddComponent<Image>();
        bg.color  = corFundo;

        RectTransform rtP    = bg.rectTransform;
        rtP.anchorMin        = new Vector2(0f, 0f);
        rtP.anchorMax        = new Vector2(1f, 0f);
        rtP.pivot            = new Vector2(0.5f, 0f);
        rtP.anchoredPosition = Vector2.zero;
        rtP.sizeDelta        = new Vector2(0f, alturaPanel);

        // Texto principal (máquina de escrever)
        GameObject textoObj = new GameObject("Texto");
        textoObj.transform.SetParent(painelObj.transform, false);
        textoUI          = textoObj.AddComponent<TextMeshProUGUI>();
        textoUI.text     = "";
        textoUI.fontSize = 22f;
        textoUI.color    = corTexto;
        textoUI.alignment = TextAlignmentOptions.TopLeft;
        if (fonte != null) textoUI.font = fonte;

        RectTransform rtT = textoUI.rectTransform;
        rtT.anchorMin = Vector2.zero;
        rtT.anchorMax = Vector2.one;
        rtT.offsetMin = new Vector2(32f, 36f);
        rtT.offsetMax = new Vector2(-32f, -14f);

        // Dica de fechar
        GameObject dicaObj = new GameObject("Dica");
        dicaObj.transform.SetParent(painelObj.transform, false);
        TMP_Text dica  = dicaObj.AddComponent<TextMeshProUGUI>();
        dica.text      = "[ qualquer tecla para continuar ]";
        dica.fontSize  = 13f;
        dica.color     = new Color(0.55f, 0.55f, 0.6f);
        dica.alignment = TextAlignmentOptions.BottomRight;
        if (fonte != null) dica.font = fonte;

        RectTransform rtD = dica.rectTransform;
        rtD.anchorMin = Vector2.zero;
        rtD.anchorMax = Vector2.one;
        rtD.offsetMin = new Vector2(10f, 4f);
        rtD.offsetMax = new Vector2(-12f, -4f);
    }

    // ── Máquina de escrever ──────────────────────────────────────────

    private IEnumerator Digitar()
    {
        float intervalo = 1f / Mathf.Max(1f, velocidadeDigitacao);

        for (int i = 0; i <= textoDialogo.Length; i++)
        {
            textoUI.text = textoDialogo.Substring(0, i);

            // Som de digitação pontual (a cada 3 caracteres para não soar repetitivo demais)
            if (i % 3 == 0) AudioManager.Instance?.PlaySFX(somDigitacao, 0.25f);

            yield return new WaitForSecondsRealtime(intervalo);
        }

        dialogoFinalizado = true;

        if (tempoAutoFechar > 0f)
        {
            yield return new WaitForSecondsRealtime(tempoAutoFechar);
            Fechar();
        }
    }

    private void Fechar()
    {
        dialogoFinalizado = false;
        if (digitacaoCoroutine != null) StopCoroutine(digitacaoCoroutine);
        Time.timeScale = 1f;
        if (canvasDialogo != null) Destroy(canvasDialogo);
        Destroy(this);
    }
}
