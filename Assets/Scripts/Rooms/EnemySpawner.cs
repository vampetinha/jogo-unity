using UnityEngine;

/// <summary>
/// Coloque em um GameObject vazio dentro da sala.
/// Configure os prefabs e pontos de spawn no Inspector.
/// Chamado pelo RoomController — não spawna sozinho.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Inimigos")]
    [Tooltip("Um ou mais prefabs. Cada ponto de spawn sorteia um aleatoriamente.")]
    public GameObject[] prefabsInimigos;

    [Header("Pontos de Surgimento")]
    [Tooltip("Arraste aqui GameObjects vazios posicionados onde os inimigos vão aparecer.")]
    public Transform[] pontosDeSpawn;

    /// <summary>
    /// Instancia os inimigos e os registra na sala.
    /// Retorna a quantidade de inimigos criados com HealthSystem.
    /// </summary>
    public int Spawnar(RoomController sala)
    {
        if (prefabsInimigos == null || prefabsInimigos.Length == 0)
        {
            Debug.LogWarning($"{name}: nenhum prefab de inimigo configurado.");
            return 0;
        }

        int contagem = 0;

        foreach (Transform ponto in pontosDeSpawn)
        {
            if (ponto == null) continue;

            // Sorteia um tipo de inimigo do pool
            GameObject prefab = prefabsInimigos[Random.Range(0, prefabsInimigos.Length)];
            if (prefab == null) continue;

            GameObject obj = Instantiate(prefab, ponto.position, Quaternion.identity);

            // Assina o evento de morte para notificar a sala
            HealthSystem vida = obj.GetComponent<HealthSystem>();
            if (vida != null)
            {
                vida.aoMorrer.AddListener(() => sala.NotificarInimigoDerrotado());
                contagem++;
            }
            else
            {
                Debug.LogWarning($"{prefab.name} não tem HealthSystem — não será contado pela sala.");
            }
        }

        return contagem;
    }

    // Esferas vermelhas nos pontos de spawn para visualizar no Editor
    void OnDrawGizmos()
    {
        if (pontosDeSpawn == null) return;
        Gizmos.color = Color.red;
        foreach (Transform p in pontosDeSpawn)
            if (p != null) Gizmos.DrawWireSphere(p.position, 0.3f);
    }
}
