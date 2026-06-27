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
    [Tooltip("Distância mínima do player para spawnar (em unidades). 0 = sem restrição.")]
    public float distanciaMinimaDaPlayer = 5f;
    [Tooltip("Tentativas máximas para achar posição longe o suficiente do player.")]
    public int tentativasMaximas = 30;

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

        Transform playerTransform = null;
        if (distanciaMinimaDaPlayer > 0f)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) playerTransform = playerObj.transform;
        }

        int contagem = 0;
        for (int i = 0; i < quantidadeAleatoria; i++)
        {
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
                Debug.LogWarning($"{name}: não encontrou posição longe o suficiente do player após {tentativasMaximas} tentativas — spawnando mesmo assim.");

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
