using UnityEngine;

/// <summary>
/// Adicione na raiz da sala. Configure chão e aberturas,
/// depois clique com botão direito no componente → "Gerar Chão e Paredes".
/// </summary>
public class RoomWallGenerator : MonoBehaviour
{
    [Header("Chão")]
    [Tooltip("Sprite do chão. Se vazio, usa quadrado branco padrão.")]
    public Sprite spriteChao = null;
    public Color  corChao    = new Color(0.22f, 0.55f, 0.22f);
    public int    ordemChao  = 0;

    [Header("Posicionamento das Colisões de Parede")]
    [Tooltip("Empurra cada parede para dentro da sala. Valor positivo = para dentro.")]
    public float   insetParedes   = 0f;
    [Tooltip("Empurra cada parede para fora da sala. Valor positivo = para fora.")]
    public float   outsetParedes  = 0f;
    [Tooltip("Desloca todas as paredes num eixo fixo (X/Y).")]
    public Vector2 offsetParedes  = Vector2.zero;
    [HideInInspector] public float   insetAplicado  = 0f;
    [HideInInspector] public float   outsetAplicado = 0f;
    [HideInInspector] public Vector2 offsetAplicado = Vector2.zero;

    [Header("Largura da abertura do corredor")]
    public float larguraCorredor = 3f;

    [Header("Lados com abertura para corredor")]
    public bool aberturaNoRte  = false;
    public bool aberturaSul    = false;
    public bool aberturaLeste  = false;
    public bool aberturaOeste  = false;

    // ── Auto-aplicar no Inspector ─────────────────

    void OnValidate()
    {
        Transform chaoTransform = transform.Find("Chao");
        if (chaoTransform == null) return;

        SpriteRenderer sr = chaoTransform.GetComponent<SpriteRenderer>();
        if (sr == null) return;

        sr.sprite       = spriteChao != null ? spriteChao : sr.sprite;
        sr.color        = corChao;
        sr.sortingOrder = ordemChao;
    }

    // ── Menu de contexto ──────────────────────────

    [Header("Corredor (use só se for um corredor, não uma sala)")]
    [Tooltip("Tamanho do corredor em unidades. X = largura, Y = altura.")]
    public Vector2 tamanhoCorredor = new Vector2(5f, 3f);

    [ContextMenu("Gerar Corredor (Chão + Paredes Laterais)")]
    public void GerarCorredor()
    {
        foreach (string n in new[] { "Chao", "Square" })
        {
            Transform t = transform.Find(n);
            if (t != null) DestroyImmediate(t.gameObject);
        }
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform filho = transform.GetChild(i);
            if (filho.name.StartsWith("Parede_"))
                DestroyImmediate(filho.gameObject);
        }

        GameObject chao = new GameObject("Chao");
        chao.transform.SetParent(transform);
        chao.transform.localPosition = Vector3.zero;
        chao.transform.localScale    = new Vector3(tamanhoCorredor.x, tamanhoCorredor.y, 1f);
        SpriteRenderer sr = chao.AddComponent<SpriteRenderer>();
        sr.sprite       = spriteChao != null ? spriteChao : CriarSpriteWhite();
        sr.color        = corChao;
        sr.sortingOrder = ordemChao;

        float halfX = tamanhoCorredor.x / 2f;
        float halfY = tamanhoCorredor.y / 2f;
        const float esp = 0.3f;

        bool horizontal = tamanhoCorredor.x >= tamanhoCorredor.y;
        if (horizontal)
        {
            CriarParede("Parede_Cima",  new Vector2(0,  halfY), new Vector2(tamanhoCorredor.x, esp));
            CriarParede("Parede_Baixo", new Vector2(0, -halfY), new Vector2(tamanhoCorredor.x, esp));
        }
        else
        {
            CriarParede("Parede_Direita",  new Vector2( halfX, 0), new Vector2(esp, tamanhoCorredor.y));
            CriarParede("Parede_Esquerda", new Vector2(-halfX, 0), new Vector2(esp, tamanhoCorredor.y));
        }

        Debug.Log($"{name}: corredor {(horizontal ? "horizontal" : "vertical")} gerado.");
    }

    [ContextMenu("Ajustar Sala ao Sprite do Chão")]
    public void AjustarSalaAoSprite()
    {
        if (spriteChao == null) { Debug.LogWarning($"{name}: atribua um Sprite Chao primeiro."); return; }

        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col == null) { Debug.LogWarning($"{name}: precisa de BoxCollider2D."); return; }

        transform.localScale = Vector3.one;

        Vector2 tamanho = spriteChao.bounds.size;
        col.size   = tamanho;
        col.offset = Vector2.zero;

        GerarTudo();

        Debug.Log($"{name}: sala ajustada para {tamanho.x:F2} x {tamanho.y:F2} unidades.");
    }

    [ContextMenu("Gerar Chão e Paredes")]
    public void GerarTudo()
    {
        GerarChao();
        GerarParedes();
    }

    [ContextMenu("Aplicar Inset/Outset/Offset nas Paredes Existentes")]
    public void AplicarNasParedes()
    {
        float   deltaInset  = insetParedes  - insetAplicado;
        float   deltaOutset = outsetParedes - outsetAplicado;
        Vector2 deltaOffset = offsetParedes - offsetAplicado;

        if (Mathf.Approximately(deltaInset, 0f) && Mathf.Approximately(deltaOutset, 0f) && deltaOffset == Vector2.zero)
        {
            Debug.Log($"{name}: inset/outset/offset já estão aplicados.");
            return;
        }

        int count = 0;
        foreach (Transform filho in transform)
        {
            BoxCollider2D col = filho.GetComponent<BoxCollider2D>();
            if (col == null || col.isTrigger) continue;

            Vector2 pos = filho.localPosition;

            if (!Mathf.Approximately(deltaInset, 0f) && pos != Vector2.zero)
                filho.localPosition += (Vector3)(-pos.normalized * deltaInset);

            if (!Mathf.Approximately(deltaOutset, 0f) && pos != Vector2.zero)
                filho.localPosition += (Vector3)(pos.normalized * deltaOutset);

            filho.localPosition += (Vector3)deltaOffset;
            count++;
        }

        insetAplicado  = insetParedes;
        outsetAplicado = outsetParedes;
        offsetAplicado = offsetParedes;
        Debug.Log($"{name}: inset {deltaInset:F2} + outset {deltaOutset:F2} + offset {deltaOffset} aplicados em {count} paredes.");
    }

    [ContextMenu("Resetar Inset/Outset/Offset das Paredes")]
    public void ResetarNasParedes()
    {
        insetParedes  = 0f;
        outsetParedes = 0f;
        offsetParedes = Vector2.zero;
        AplicarNasParedes();
    }

    [ContextMenu("Gerar Chão")]
    public void GerarChao()
    {
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col == null) { Debug.LogWarning($"{name}: precisa de BoxCollider2D."); return; }

        foreach (string nome in new[] { "Chao", "Square" })
        {
            Transform t = transform.Find(nome);
            if (t != null) DestroyImmediate(t.gameObject);
        }

        GameObject chao = new GameObject("Chao");
        chao.transform.SetParent(transform);
        chao.transform.localPosition = Vector3.zero;

        SpriteRenderer sr = chao.AddComponent<SpriteRenderer>();
        sr.sprite       = spriteChao != null ? spriteChao : CriarSpriteWhite();
        sr.color        = corChao;
        sr.sortingOrder = ordemChao;

        chao.transform.localScale = spriteChao != null
            ? Vector3.one
            : new Vector3(col.size.x, col.size.y, 1f);

        Debug.Log($"{name}: chão gerado.");
    }

    [ContextMenu("Gerar Paredes")]
    public void GerarParedes()
    {
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col == null) { Debug.LogWarning($"{name}: precisa de BoxCollider2D."); return; }

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform filho = transform.GetChild(i);
            if (filho.name.StartsWith("Parede_"))
                DestroyImmediate(filho.gameObject);
        }

        float w = col.size.x, h = col.size.y, esp = 0.3f;

        insetAplicado  = 0f;
        outsetAplicado = 0f;
        offsetAplicado = Vector2.zero;

        GerarLado("Norte", Vector2.up    * h / 2f, new Vector2(w,   esp), aberturaNoRte,  true);
        GerarLado("Sul",   Vector2.down  * h / 2f, new Vector2(w,   esp), aberturaSul,    true);
        GerarLado("Leste", Vector2.right * w / 2f, new Vector2(esp, h  ), aberturaLeste,  false);
        GerarLado("Oeste", Vector2.left  * w / 2f, new Vector2(esp, h  ), aberturaOeste,  false);

        if (!Mathf.Approximately(insetParedes, 0f) || offsetParedes != Vector2.zero)
            AplicarNasParedes();

        Debug.Log($"{name}: paredes geradas.");
    }

    // ── Internos ──────────────────────────────────

    void GerarLado(string nome, Vector2 centro, Vector2 tamanho, bool temAbertura, bool horizontal)
    {
        if (!temAbertura)
        {
            CriarParede($"Parede_{nome}", centro, tamanho);
            return;
        }

        float comprimento  = horizontal ? tamanho.x : tamanho.y;
        float segmento     = (comprimento - larguraCorredor) / 2f;
        float desl         = segmento / 2f + larguraCorredor / 2f;

        if (horizontal)
        {
            CriarParede($"Parede_{nome}_Dir", centro + Vector2.right * desl, new Vector2(segmento, tamanho.y));
            CriarParede($"Parede_{nome}_Esq", centro + Vector2.left  * desl, new Vector2(segmento, tamanho.y));
        }
        else
        {
            CriarParede($"Parede_{nome}_Sup", centro + Vector2.up   * desl, new Vector2(tamanho.x, segmento));
            CriarParede($"Parede_{nome}_Inf", centro + Vector2.down * desl, new Vector2(tamanho.x, segmento));
        }
    }

    void CriarParede(string nome, Vector2 posLocal, Vector2 tamanho)
    {
        GameObject go = new GameObject(nome);
        go.transform.SetParent(transform);
        go.transform.localPosition = new Vector3(posLocal.x, posLocal.y, 0f);
        go.transform.localScale    = Vector3.one;
        go.AddComponent<BoxCollider2D>().size = tamanho;
    }

    static Sprite CriarSpriteWhite()
    {
        Texture2D tex = new Texture2D(4, 4);
        Color[] pixels = new Color[16];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
    }
}
