using UnityEngine;

public class RoomController : MonoBehaviour
{
    [Header("Inimigos")]
    [Tooltip("Prefabs dos inimigos que podem aparecer nesta sala.")]
    public GameObject[] prefabsInimigos;
    [Tooltip("Quantos inimigos spawnar ao entrar na sala.")]
    public int quantidadeInimigos = 3;
    [Tooltip("Margem interna para não spawnar rente às paredes.")]
    public float margemParedes = 0.8f;
    [Tooltip("Distância mínima do player para spawnar (em unidades). 0 = sem restrição.")]
    public float distanciaMinimaDaPlayer = 5f;
    [Tooltip("Tentativas máximas para achar posição longe o suficiente do player.")]
    public int tentativasMaximas = 30;

    [Header("Portas")]
    public DoorController[] portas;

    [Header("Portal (somente na última sala da fase)")]
    public PortalController portal;

    [Header("Boss (somente na sala do boss)")]
    public BossController boss;

    [Header("Configuração")]
    [Tooltip("Ative SOMENTE na sala onde o player começa. Nas demais deixe desativado.")]
    public bool ativarSeJaDentro = false;

    private bool ativada = false;
    private int inimigosVivos = 0;

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[{name}] TriggerEnter: {other.name} | tag={other.tag} | ativada={ativada}");
        if (!ativada && other.CompareTag("Player"))
            AtivarSala();
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (ativarSeJaDentro && !ativada && other.CompareTag("Player"))
            AtivarSala();
    }

    private void AtivarSala()
    {
        ativada = true;
        Debug.Log($"[{name}] AtivarSala — prefabs={prefabsInimigos?.Length} qtd={quantidadeInimigos}");

        foreach (DoorController porta in portas)
            porta?.FecharComDelay();

        inimigosVivos = 0;
        inimigosVivos += SpawnarInimigos();
        Debug.Log($"[{name}] inimigosVivos={inimigosVivos}");

        if (boss != null)
        {
            inimigosVivos++;
            boss.AtivarBoss();
        }

        if (inimigosVivos <= 0)
            SalaLimpa();
    }

    private int SpawnarInimigos()
    {
        if (prefabsInimigos == null || prefabsInimigos.Length == 0 || quantidadeInimigos <= 0)
            return 0;

        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col == null)
        {
            Debug.LogWarning($"{name}: sem BoxCollider2D para spawnar inimigos.");
            return 0;
        }

        Bounds b = col.bounds;
        float xMin = b.min.x + margemParedes;
        float xMax = b.max.x - margemParedes;
        float yMin = b.min.y + margemParedes;
        float yMax = b.max.y - margemParedes;

        Transform playerTransform = null;
        if (distanciaMinimaDaPlayer > 0f)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) playerTransform = playerObj.transform;
        }

        int contagem = 0;
        for (int i = 0; i < quantidadeInimigos; i++)
        {
            GameObject prefab = prefabsInimigos[Random.Range(0, prefabsInimigos.Length)];
            if (prefab == null) continue;

            Vector2 pos = Vector2.zero;
            bool posicaoValida = false;
            for (int tentativa = 0; tentativa < tentativasMaximas; tentativa++)
            {
                pos = new Vector2(Random.Range(xMin, xMax), Random.Range(yMin, yMax));
                if (playerTransform == null || Vector2.Distance(pos, playerTransform.position) >= distanciaMinimaDaPlayer)
                {
                    posicaoValida = true;
                    break;
                }
            }
            if (!posicaoValida)
                Debug.LogWarning($"{name}: não achou posição longe do player após {tentativasMaximas} tentativas.");

            GameObject obj = Instantiate(prefab, pos, Quaternion.identity);

            HealthSystem vida = obj.GetComponent<HealthSystem>();
            if (vida != null)
            {
                vida.aoMorrer.AddListener(() => NotificarInimigoDerrotado());
                contagem++;
            }
        }
        return contagem;
    }

    public void NotificarInimigoDerrotado()
    {
        inimigosVivos = Mathf.Max(0, inimigosVivos - 1);
        if (inimigosVivos <= 0)
            SalaLimpa();
    }

    private void SalaLimpa()
    {
        foreach (DoorController porta in portas)
            porta?.Abrir();

        portal?.Ativar();
        Debug.Log($"Sala '{gameObject.name}' limpa!");
    }

    void OnDrawGizmos()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col == null) return;
        Gizmos.color = new Color(0f, 1f, 0f, 0.08f);
        Gizmos.DrawCube(col.bounds.center, col.bounds.size);
        Gizmos.color = new Color(0f, 1f, 0f, 0.4f);
        Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
    }
}
