using System.Collections;
using UnityEngine;

/// <summary>
/// Boss com duas fases:
///   Fase 1 (HP > 50%): perseguição agressiva + avanço (dash)
///   Fase 2 (HP ≤ 50%): mantém distância + rajada de 3 projéteis + círculo de 8
/// O RoomController chama AtivarBoss() ao ativar a sala.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(HealthSystem))]
public class BossController : MonoBehaviour
{
    public enum FaseBoss { Inativo, Fase1, Transicao, Fase2, Morto }

    [Header("Identificação")]
    public string nomeBoss = "Senhor da Fortaleza";

    [Header("Fase 1 — Corpo a Corpo")]
    public float velocidadeFase1  = 3.5f;
    public float alcanceMelee     = 1.5f;
    public float danoMelee        = 35f;
    public float intervaloMelee   = 1.2f;

    [Header("Fase 1 — Avanço (Dash)")]
    public float velocidadeAvanco = 14f;
    public float duracaoAvanco    = 0.25f;
    public float intervaloAvanco  = 5f;

    [Header("Fase 2 — Ataque à Distância")]
    public float velocidadeFase2   = 2.5f;
    public float distanciaIdeal    = 5f;
    public GameObject prefabProjetil;
    public float danoProjetil      = 20f;
    public float velocidadeProjetil = 9f;
    public float alcanceProjetil   = 10f;
    public float intervaloSpread   = 1.5f;
    public float intervaloRafaga   = 6f;
    [Tooltip("Projéteis no círculo de 360°")]
    public int   quantidadeRafaga  = 8;

    [Header("Morte")]
    public GameObject prefabVFXMorte;
    public float delayVitoria = 1f;

    // ── Referências ──────────────────────────────────────────────────
    private Rigidbody2D  rb;
    private HealthSystem vida;
    private SpriteRenderer sr;
    private Transform    player;
    private RoomController sala;

    // ── Estado ───────────────────────────────────────────────────────
    public FaseBoss Estado { get; private set; } = FaseBoss.Inativo;

    private float timerMelee;
    private float timerAvanco;
    private float timerSpread;
    private float timerRafaga;
    private bool  emAvanco;

    // ────────────────────────────────────────────────────────────────

    void Awake()
    {
        rb              = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.angularDamping = 1000f;
        vida = GetComponent<HealthSystem>();
        sr   = GetComponent<SpriteRenderer>();
        sala = GetComponentInParent<RoomController>();
    }

    void Start()
    {
        // Morte manual: o boss controla o próprio Destroy (precisa da animação)
        vida.morteManual = true;
        vida.aoMorrer.AddListener(IniciarMorte);
        vida.aoReceberDano.AddListener(VerificarTransicao);

        HUDManager.Instance?.MostrarBarraBoss(nomeBoss, vida);
    }

    // ── API pública ──────────────────────────────────────────────────

    /// <summary>Chamado pelo RoomController ao ativar a sala do boss.</summary>
    public void AtivarBoss()
    {
        Estado = FaseBoss.Fase1;
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        timerMelee  = intervaloMelee;
        timerAvanco = intervaloAvanco * 0.5f; // primeiro dash mais cedo
        timerSpread = intervaloSpread;
        timerRafaga = intervaloRafaga;
    }

    // ── Update / FixedUpdate ─────────────────────────────────────────

    void Update()
    {
        if (player == null || Estado == FaseBoss.Inativo ||
            Estado == FaseBoss.Transicao || Estado == FaseBoss.Morto) return;

        timerMelee  -= Time.deltaTime;
        timerAvanco -= Time.deltaTime;

        float dist = Vector2.Distance(transform.position, player.position);

        if (Estado == FaseBoss.Fase1)
        {
            if (dist <= alcanceMelee && timerMelee <= 0f)
            {
                Atacar();
                timerMelee = intervaloMelee;
            }
            if (!emAvanco && timerAvanco <= 0f)
                StartCoroutine(ExecutarAvanco());
        }
        else if (Estado == FaseBoss.Fase2)
        {
            timerSpread -= Time.deltaTime;
            timerRafaga -= Time.deltaTime;

            if (timerSpread <= 0f)
            {
                DispararSpread();
                timerSpread = intervaloSpread;
            }
            if (timerRafaga <= 0f)
            {
                DispararRafaga();
                timerRafaga = intervaloRafaga;
            }
            if (dist <= alcanceMelee && timerMelee <= 0f)
            {
                Atacar();
                timerMelee = intervaloMelee;
            }
        }
    }

    void FixedUpdate()
    {
        if (emAvanco || player == null || Estado == FaseBoss.Inativo ||
            Estado == FaseBoss.Transicao || Estado == FaseBoss.Morto) return;

        Vector2 dir  = (Vector2)player.position - (Vector2)transform.position;
        float   dist = dir.magnitude;
        dir.Normalize();

        // Gira para o jogador
        float angulo = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        rb.MoveRotation(angulo);

        if (Estado == FaseBoss.Fase1)
        {
            rb.linearVelocity = dist > alcanceMelee
                ? dir * velocidadeFase1
                : Vector2.Lerp(rb.linearVelocity, Vector2.zero, 0.3f);
        }
        else if (Estado == FaseBoss.Fase2)
        {
            if (dist < distanciaIdeal * 0.7f)
                rb.linearVelocity = -dir * velocidadeFase2;  // recua
            else if (dist > distanciaIdeal * 1.3f)
                rb.linearVelocity =  dir * velocidadeFase2;  // avança
            else
                rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, 0.2f);
        }
    }

    // ── Ataques ──────────────────────────────────────────────────────

    private void Atacar()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, alcanceMelee + 0.3f);
        foreach (Collider2D hit in hits)
        {
            if (!hit.CompareTag("Player")) continue;
            HealthSystem hs = hit.GetComponent<HealthSystem>();
            if (hs != null) hs.ReceberDano(danoMelee);
        }
    }

    private IEnumerator ExecutarAvanco()
    {
        if (player == null) yield break;
        emAvanco = true;

        Vector2 direcao = ((Vector2)player.position - (Vector2)transform.position).normalized;
        bool    acertou = false;
        float   tempo   = 0f;

        while (tempo < duracaoAvanco)
        {
            rb.linearVelocity = direcao * velocidadeAvanco;

            // Causa dano apenas uma vez por avanço
            if (!acertou)
            {
                Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, alcanceMelee);
                foreach (Collider2D hit in hits)
                {
                    if (!hit.CompareTag("Player")) continue;
                    HealthSystem hs = hit.GetComponent<HealthSystem>();
                    if (hs == null) continue;
                    hs.ReceberDano(danoMelee * 1.5f); // dano extra no dash
                    acertou = true;
                    break;
                }
            }

            tempo += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        rb.linearVelocity = Vector2.zero;
        emAvanco           = false;
        timerAvanco        = intervaloAvanco;
    }

    // Fase 2: 3 projéteis em leque direcionados ao jogador
    private void DispararSpread()
    {
        if (prefabProjetil == null || player == null) return;

        Vector2 dir        = ((Vector2)player.position - (Vector2)transform.position).normalized;
        float   anguloBase = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        float[] offsets = { -25f, 0f, 25f };
        foreach (float off in offsets)
            InstanciarProjetil(anguloBase + off);
    }

    // Fase 2: círculo de projéteis em todas as direções
    private void DispararRafaga()
    {
        if (prefabProjetil == null) return;
        for (int i = 0; i < quantidadeRafaga; i++)
            InstanciarProjetil(i * (360f / quantidadeRafaga));
    }

    private void InstanciarProjetil(float angulo)
    {
        GameObject obj = Instantiate(prefabProjetil, transform.position,
                                     Quaternion.Euler(0f, 0f, angulo));
        EnemyProjectile proj = obj.GetComponent<EnemyProjectile>();
        if (proj != null) proj.Configurar(danoProjetil, velocidadeProjetil, alcanceProjetil);
    }

    // ── Transição e Morte ────────────────────────────────────────────

    private void VerificarTransicao(float vidaAtual, float vidaMaxima)
    {
        if (Estado == FaseBoss.Fase1 && vidaAtual / vidaMaxima <= 0.5f)
            StartCoroutine(TransicaoParaFase2());
    }

    private IEnumerator TransicaoParaFase2()
    {
        Estado = FaseBoss.Transicao;
        rb.linearVelocity = Vector2.zero;

        // Pisca vermelho 5 vezes — sinal de enraivecimento
        Color original = sr != null ? sr.color : Color.white;
        for (int i = 0; i < 5; i++)
        {
            if (sr != null) sr.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            if (sr != null) sr.color = original;
            yield return new WaitForSeconds(0.1f);
        }

        // Cresce levemente para indicar power-up
        transform.localScale *= 1.2f;

        Estado      = FaseBoss.Fase2;
        timerSpread = 0.4f; // atira logo após a transição
        timerRafaga = 3f;
    }

    private void IniciarMorte()
    {
        StartCoroutine(SequenciaMorte());
    }

    private IEnumerator SequenciaMorte()
    {
        Estado = FaseBoss.Morto;
        rb.linearVelocity = Vector2.zero;

        // Pisca branco rapidamente
        Color original = sr != null ? sr.color : Color.white;
        for (int i = 0; i < 8; i++)
        {
            if (sr != null) sr.color = Color.white;
            yield return new WaitForSeconds(0.07f);
            if (sr != null) sr.color = original;
            yield return new WaitForSeconds(0.07f);
        }

        if (prefabVFXMorte != null)
            Instantiate(prefabVFXMorte, transform.position, Quaternion.identity);

        // Notifica a sala (abre portas se houver)
        sala?.NotificarInimigoDerrotado();

        yield return new WaitForSeconds(delayVitoria);

        HUDManager.Instance?.OcultarBarraBoss();
        HUDManager.Instance?.MostrarVitoria();

        Destroy(gameObject);
    }

    // ── Gizmos ──────────────────────────────────────────────────────
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, alcanceMelee);
        Gizmos.color = new Color(0.4f, 0.4f, 1f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, distanciaIdeal);
    }
}
