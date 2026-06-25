using System.Collections;
using UnityEngine;

/// <summary>
/// Inimigo kamikaze: corre em direção ao jogador e explode ao chegar perto.
/// Também explode ao morrer por dano — o jogador deve mantê-lo à distância.
/// Precisa de: Rigidbody2D + HealthSystem.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(HealthSystem))]
public class EnemyExploder : MonoBehaviour
{
    [Header("Movimento")]
    public float velocidade = 4.5f;

    [Header("Explosão")]
    [Tooltip("Distância do jogador que dispara a explosão de proximidade")]
    public float raioAtivacao  = 1.2f;
    [Tooltip("Raio de dano da explosão")]
    public float raioExplosao  = 2.5f;
    public float danoExplosao  = 60f;

    [Header("Visual")]
    [Tooltip("Partícula ou efeito instantiado no centro da explosão (opcional)")]
    public GameObject prefabVFXExplosao;
    [Tooltip("Pisca em vermelho antes de explodir por proximidade")]
    public bool avisarAntesDeExplodir = true;

    private Rigidbody2D  rb;
    private HealthSystem vida;
    private Transform    player;
    private SpriteRenderer sr;
    private bool explodiu = false; // true depois que a explosão ocorreu
    private bool emAviso  = false; // true durante a animação de aviso (pisca)

    void Awake()
    {
        rb              = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.angularDamping = 1000f;
        vida = GetComponent<HealthSystem>();
        sr   = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        // Explode também quando morto por dano
        vida.aoMorrer.AddListener(Explodir);

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
        else
            Debug.LogWarning($"{name}: jogador não encontrado na cena.");
    }

    void FixedUpdate()
    {
        if (explodiu || emAviso || player == null) return;

        Vector2 dir  = (Vector2)player.position - (Vector2)transform.position;
        float   dist = dir.magnitude;
        dir.Normalize();

        // Gira para o jogador
        float angulo = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        rb.MoveRotation(angulo);

        if (dist <= raioAtivacao)
        {
            // Chegou perto: avisa e explode
            if (avisarAntesDeExplodir)
                StartCoroutine(AvisoEExplodir());
            else
                Explodir();
            return;
        }

        rb.linearVelocity = dir * velocidade;
    }

    private IEnumerator AvisoEExplodir()
    {
        emAviso = true;
        rb.linearVelocity = Vector2.zero;

        // Pisca 3 vezes em 0.4 s
        Color corOriginal = sr != null ? sr.color : Color.white;
        for (int i = 0; i < 3; i++)
        {
            if (sr != null) sr.color = Color.red;
            yield return new WaitForSeconds(0.07f);
            if (sr != null) sr.color = corOriginal;
            yield return new WaitForSeconds(0.07f);
        }

        emAviso = false;
        // Se o inimigo já foi destruído por dano durante o aviso, explodiu=true e Explodir() retorna cedo
        if (!explodiu) Explodir();
    }

    private void Explodir()
    {
        if (explodiu) return;
        explodiu = true;

        rb.linearVelocity = Vector2.zero;

        // Dano em todos os Colliders no raio que pertencem ao jogador
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, raioExplosao);
        foreach (Collider2D hit in hits)
        {
            if (!hit.CompareTag("Player")) continue;
            HealthSystem hs = hit.GetComponent<HealthSystem>();
            if (hs != null) hs.ReceberDano(danoExplosao);
        }

        // VFX de explosão
        if (prefabVFXExplosao != null)
            Instantiate(prefabVFXExplosao, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        // Raio de ativação (amarelo)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, raioAtivacao);

        // Raio de dano da explosão (laranja)
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.35f);
        Gizmos.DrawSphere(transform.position, raioExplosao);
        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawWireSphere(transform.position, raioExplosao);
    }
}
