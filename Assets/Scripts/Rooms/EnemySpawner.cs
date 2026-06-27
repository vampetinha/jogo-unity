using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Inimigos")]
    [Tooltip("Um ou mais prefabs — sorteia aleatoriamente.")]
    public GameObject[] prefabsInimigos;

    [Header("Modo Aleatório (sem pontos manuais)")]
    [Tooltip("Quantidade de inimigos a spawnar aleatoriamente dentro do BoxCollider2D da sala.")]
    public int quantidadeAleatoria = 3;
    [Tooltip("Margem interna para evitar spawnar rente às paredes (em unidades).")]
    public float margemParedes = 0.8f;

    [Header("Pontos Manuais (opcional)")]
    [Tooltip("Se preenchido, usa esses pontos em vez de posições aleatórias.")]
    public Transform[] pontosDeSpawn;

    public int Spawnar(RoomController sala)
    {
        if (prefabsInimigos == null || prefabsInimigos.Length == 0)
        {
            Debug.LogWarning($"{name}: nenhum prefab de inimigo configurado.");
            return 0;
        }

        return pontosDeSpawn != null && pontosDeSpawn.Length > 0
            ? SpawnarNasPosicoes(sala)
            : SpawnarAleatorio(sala);
    }

    private int SpawnarNasPosicoes(RoomController sala)
    {
        int contagem = 0;
        foreach (Transform ponto in pontosDeSpawn)
        {
            if (ponto == null) continue;
            contagem += InstanciarInimigo(sala, ponto.position);
        }
        return contagem;
    }

    private int SpawnarAleatorio(RoomController sala)
    {
        BoxCollider2D col = sala.GetComponent<BoxCollider2D>();
        if (col == null)
        {
            Debug.LogWarning($"{sala.name}: sem BoxCollider2D para spawnar aleatoriamente.");
            return 0;
        }

        Bounds bounds = col.bounds;
        float xMin = bounds.min.x + margemParedes;
        float xMax = bounds.max.x - margemParedes;
        float yMin = bounds.min.y + margemParedes;
        float yMax = bounds.max.y - margemParedes;

        int contagem = 0;
        for (int i = 0; i < quantidadeAleatoria; i++)
        {
            Vector2 pos = new Vector2(
                Random.Range(xMin, xMax),
                Random.Range(yMin, yMax)
            );
            contagem += InstanciarInimigo(sala, pos);
        }
        return contagem;
    }

    private int InstanciarInimigo(RoomController sala, Vector2 pos)
    {
        GameObject prefab = prefabsInimigos[Random.Range(0, prefabsInimigos.Length)];
        if (prefab == null) return 0;

        GameObject obj = Instantiate(prefab, pos, Quaternion.identity);

        HealthSystem vida = obj.GetComponent<HealthSystem>();
        if (vida != null)
        {
            vida.aoMorrer.AddListener(() => sala.NotificarInimigoDerrotado());
            return 1;
        }

        Debug.LogWarning($"{prefab.name} não tem HealthSystem — não será contado pela sala.");
        return 0;
    }

    void OnDrawGizmos()
    {
        if (pontosDeSpawn != null)
        {
            Gizmos.color = Color.red;
            foreach (Transform p in pontosDeSpawn)
                if (p != null) Gizmos.DrawWireSphere(p.position, 0.3f);
        }
    }
}
