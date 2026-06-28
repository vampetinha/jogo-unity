using UnityEngine;

public class SucaoBuracoNegro : MonoBehaviour
{
    public static void Spawnar(Vector2 pos, WeaponData arma)
    {
        GameObject obj = new GameObject("SucaoBuracoNegro");
        obj.transform.position = pos;
        var s          = obj.AddComponent<SucaoBuracoNegro>();
        s.raio         = arma.raioSucao;
        s.forca        = arma.forcaSucao;
        s.duracao      = arma.duracaoSucao;
        s.cor          = arma.corParticula;
    }

    private float raio, forca, duracao, timer;
    private Color cor;
    private SpriteRenderer anel;

    void Start()
    {
        timer = duracao;
        CriarVisual();
    }

    void CriarVisual()
    {
        anel                 = gameObject.AddComponent<SpriteRenderer>();
        anel.sortingOrder    = 20;
        anel.material        = new Material(Shader.Find("Legacy Shaders/Particles/Additive"));
        anel.material.mainTexture = CriarTexturaAnel(128);
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f) { Destroy(gameObject); return; }

        float t = 1f - (timer / duracao);

        // Anel expande até o raio e some
        float escala = raio * 2f * t;
        transform.localScale = Vector3.one * Mathf.Max(0.01f, escala);

        if (anel != null)
            anel.color = new Color(cor.r, cor.g, cor.b, 0.7f * (1f - t));
    }

    void FixedUpdate()
    {
        if (timer <= 0f) return;

        float t = 1f - (timer / duracao);

        Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position, raio);
        foreach (var col in cols)
        {
            if (col.CompareTag("Player")) continue;
            if (col.isTrigger) continue;

            Rigidbody2D rb = col.GetComponent<Rigidbody2D>();
            if (rb == null) continue;

            Vector2 dir  = (Vector2)transform.position - (Vector2)col.transform.position;
            float   dist = dir.magnitude;
            if (dist < 0.05f) continue;

            // Força maior quanto mais perto do centro e mais no começo (t pequeno = mais força)
            float intensidade = forca * Mathf.Clamp01(1f - dist / raio) * (1f - t * 0.5f);
            rb.AddForce(dir.normalized * intensidade, ForceMode2D.Force);
        }
    }

    private static Texture2D CriarTexturaAnel(int tam)
    {
        Texture2D tex = new Texture2D(tam, tam, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;

        float centro  = tam * 0.5f;
        float raioExt = tam * 0.48f;
        float raioInt = tam * 0.36f;
        float suav    = tam * 0.06f;

        for (int y = 0; y < tam; y++)
            for (int x = 0; x < tam; x++)
            {
                float dist = Mathf.Sqrt((x - centro) * (x - centro) + (y - centro) * (y - centro));
                float aExt = Mathf.Clamp01((raioExt - dist) / suav);
                float aInt = Mathf.Clamp01((dist - raioInt) / suav);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, aExt * aInt));
            }

        tex.Apply();
        return tex;
    }
}
