using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimento")]
    public float velocidade = 5f;

    [Header("Dash")]
    [Tooltip("Tecla para ativar o dash")]
    public KeyCode teclaDash = KeyCode.Space;
    [Tooltip("Velocidade do dash")]
    public float forcaDash = 18f;
    [Tooltip("Duração do dash em segundos")]
    public float duracaoDash = 0.15f;
    [Tooltip("Tempo de espera entre dashes")]
    public float cooldownDash = 0.7f;

    [Header("Dash — Sprite")]
    [Tooltip("Ativa a troca de sprite durante o dash")]
    public bool  usarSpriteDash = true;
    [Tooltip("Sprite exibido enquanto o dash está ativo (deixe vazio para manter o atual)")]
    public Sprite spriteDash;

    [Header("Dash — Trail (Sandevistan)")]
    [Tooltip("Ativa o efeito de rastro")]
    public bool  usarTrail = true;
    [Tooltip("Intervalo entre cada fantasma")]
    public float intervaloTrail     = 0.04f;
    [Tooltip("Quanto tempo cada fantasma demora para sumir")]
    public float duracaoTrail       = 0.3f;
    [Tooltip("Quantos fantasmas sobrepostos por ponto (1 = efeito limpo)")]
    public int   fantasmasPorPonto  = 1;
    [Tooltip("Espalhamento aleatório entre fantasmas sobrepostos")]
    public float offsetAleatorio    = 0f;
    [Tooltip("Cores dos fantasmas em sequência")]
    public Color[] coresTrail = new Color[]
    {
        new Color(0.5f, 0.85f, 1.0f, 0.65f),  // azul ciano suave
    };

    private Rigidbody2D  rb;
    private bool         emDash        = false;
    private float        timerCooldown = 0f;
    private Vector2      direcaoAtual  = Vector2.right;
    private SpriteRenderer sr;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale   = 0f;
        rb.freezeRotation = true;
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        timerCooldown -= Time.deltaTime;

        if (Input.GetKeyDown(teclaDash) && !emDash && timerCooldown <= 0f)
            StartCoroutine(ExecutarDash());
    }

    void FixedUpdate()
    {
        if (emDash) return;
        Mover();
    }

    private void Mover()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        Vector2 direcao = new Vector2(x, y).normalized;

        if (direcao != Vector2.zero)
            direcaoAtual = direcao;

        rb.linearVelocity = direcao * velocidade;
    }

    private IEnumerator ExecutarDash()
    {
        emDash        = true;
        timerCooldown = cooldownDash;

        // Garante referência válida ao sr (pode ter sido perdida)
        if (sr == null) sr = GetComponent<SpriteRenderer>();

        // Invencível antes de cancelar o flash para evitar race condition
        HealthSystem vida = GetComponent<HealthSystem>();
        if (vida != null) vida.SetInvencivel(true);

        // Cancela o flash de dano para não conflitar com o blink
        GetComponent<DamageFlash>()?.CancelarFlash();

        rb.linearVelocity = direcaoAtual * forcaDash;

        // Troca sprite
        Sprite spriteOriginal = sr != null ? sr.sprite : null;
        if (usarSpriteDash && spriteDash != null && sr != null)
            sr.sprite = spriteDash;

        // Trail + pisca em paralelo
        if (usarTrail) StartCoroutine(ExecutarTrail());

        // Bool dedicado — não depende de ler sr.color.a
        bool blinkLigado = false;
        float tempo = 0f;
        while (tempo < duracaoDash)
        {
            if (sr != null)
            {
                blinkLigado = !blinkLigado;
                sr.color = new Color(1f, 1f, 1f, blinkLigado ? 0.25f : 1f);
            }
            yield return new WaitForSeconds(0.05f);
            tempo += 0.05f;
        }

        // Restaura
        if (sr != null)
        {
            sr.color  = Color.white;
            if (usarSpriteDash && spriteDash != null)
                sr.sprite = spriteOriginal;
        }

        if (vida != null) vida.SetInvencivel(false);
        emDash = false;
    }

    private IEnumerator ExecutarTrail()
    {
        float tempo = 0f;
        while (tempo < duracaoDash)
        {
            CriarFantasma();
            yield return new WaitForSeconds(intervaloTrail);
            tempo += intervaloTrail;
        }
    }

    private void CriarFantasma()
    {
        if (sr == null || coresTrail == null || coresTrail.Length == 0) return;

        int qtd = Mathf.Max(1, fantasmasPorPonto);
        for (int i = 0; i < qtd; i++)
        {
            Vector3 offset = new Vector3(
                Random.Range(-offsetAleatorio, offsetAleatorio),
                Random.Range(-offsetAleatorio, offsetAleatorio), 0f);

            GameObject fantasma = new GameObject("DashTrail");
            fantasma.transform.position   = transform.position + offset;
            fantasma.transform.rotation   = transform.rotation;
            fantasma.transform.localScale = transform.localScale;

            SpriteRenderer srF = fantasma.AddComponent<SpriteRenderer>();
            srF.sprite         = sr.sprite;
            srF.flipX          = sr.flipX;
            srF.flipY          = sr.flipY;
            srF.sortingLayerID = sr.sortingLayerID;
            srF.sortingOrder   = sr.sortingOrder + 1;
            srF.color          = coresTrail[i % coresTrail.Length];

            StartCoroutine(FadeFantasma(srF));
        }
    }

    private IEnumerator FadeFantasma(SpriteRenderer srF)
    {
        float tempo = 0f;
        Color corInicial = srF.color;

        while (tempo < duracaoTrail)
        {
            if (srF == null) yield break;
            float t = tempo / duracaoTrail;
            srF.color = new Color(corInicial.r, corInicial.g, corInicial.b, Mathf.Lerp(corInicial.a, 0f, t));
            tempo += Time.deltaTime;
            yield return null;
        }

        if (srF != null) Destroy(srF.gameObject);
    }
}
