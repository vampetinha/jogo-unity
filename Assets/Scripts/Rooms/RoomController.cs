using UnityEngine;

/// <summary>
/// Coloque no GameObject raiz da sala.
/// Precisa de um BoxCollider2D com Is Trigger = true cobrindo o interior da sala.
/// </summary>
public class RoomController : MonoBehaviour
{
    [Header("Portas")]
    [Tooltip("Arraste todas as portas desta sala.")]
    public DoorController[] portas;

    [Header("Spawners")]
    [Tooltip("Arraste todos os EnemySpawners desta sala.")]
    public EnemySpawner[] spawners;

    [Header("Portal (somente na última sala da fase)")]
    [Tooltip("Deixe vazio se não for a última sala.")]
    public PortalController portal;

    [Header("Boss (somente na sala do boss)")]
    [Tooltip("Arraste o BossController desta sala. Ele será ativado ao entrar.")]
    public BossController boss;

    private bool ativada = false;
    private int inimigosVivos = 0;

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[Sala] OnTriggerEnter2D: {other.name} | tag={other.tag} | ativada={ativada}");
        if (!ativada && other.CompareTag("Player"))
            AtivarSala();
    }

    // Fallback: captura o player que já estava dentro ao apertar Play
    void OnTriggerStay2D(Collider2D other)
    {
        if (!ativada && other.CompareTag("Player"))
        {
            Debug.LogWarning("[Sala] Player já estava dentro ao iniciar — ativando pela sala.");
            AtivarSala();
        }
    }

    private void AtivarSala()
    {
        ativada = true;

        // Fecha todas as portas com delay para o player ter tempo de entrar
        foreach (DoorController porta in portas)
            porta?.FecharComDelay();

        // Spawna todos os inimigos e conta quantos têm HealthSystem
        inimigosVivos = 0;
        foreach (EnemySpawner spawner in spawners)
            inimigosVivos += spawner.Spawnar(this);

        // Boss conta como um inimigo
        if (boss != null)
        {
            inimigosVivos++;
            boss.AtivarBoss();
        }

        // Sala sem inimigos configurados: abre imediatamente
        if (inimigosVivos <= 0)
            SalaLimpa();
    }

    /// <summary>
    /// Chamado pelo EnemySpawner via evento quando um inimigo morre.
    /// </summary>
    public void NotificarInimigoDerrotado()
    {
        inimigosVivos = Mathf.Max(0, inimigosVivos - 1);

        if (inimigosVivos <= 0)
            SalaLimpa();
    }

    private void SalaLimpa()
    {
        // Abre todas as portas
        foreach (DoorController porta in portas)
            porta?.Abrir();

        // Ativa o portal se esta for a última sala
        portal?.Ativar();

        Debug.Log($"Sala '{gameObject.name}' limpa!");
    }

    // Retângulo verde no Editor para visualizar a área de trigger
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
