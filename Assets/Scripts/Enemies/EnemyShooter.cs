using UnityEngine;

/// <summary>
/// Inimigo à distância: mantém range ideal e atira projéteis no jogador.
/// Recua se o jogador chegar perto demais, avança se o jogador se afastar.
/// Precisa de: Rigidbody2D + HealthSystem + prefab de EnemyProjectile.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(HealthSystem))]
public class EnemyShooter : MonoBehaviour
{
    [Header("Movimento")]
    public float velocidade      = 2.5f;
    [Tooltip("Recua se o jogador entrar nesse raio")]
    public float distanciaMinima = 4f;
    [Tooltip("Avança se o jogador sair desse raio")]
    public float distanciaMaxima = 7f;

    [Header("Ataque")]
    public GameObject prefabProjetil;
    public float danoProjetil        = 15f;
    public float velocidadeProjetil  = 8f;
    public float alcanceProjetil     = 9f;
    public float intervaloDisparo    = 1.5f;

    private Rigidbody2D rb;
    private Transform   player;
    private float       timerDisparo;

    void Awake()
    {
        rb              = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.angularDamping = 1000f;
    }

    void Start()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
        else
            Debug.LogWarning($"{name}: jogador não encontrado na cena.");

        // Dispersa os tiros iniciais para inimigos múltiplos não atirarem juntos
        timerDisparo = Random.Range(0f, intervaloDisparo);
    }

    void Update()
    {
        if (player == null) return;
        timerDisparo -= Time.deltaTime;

        float dist = Vector2.Distance(transform.position, player.position);

        // Atira se dentro do range ideal
        if (dist >= distanciaMinima && dist <= distanciaMaxima && timerDisparo <= 0f)
            Atirar();
    }

    void FixedUpdate()
    {
        if (player == null) return;

        Vector2 dir  = (Vector2)player.position - (Vector2)transform.position;
        float   dist = dir.magnitude;
        dir.Normalize();

        // Gira para o jogador
        float angulo = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        rb.MoveRotation(angulo);

        // Movimenta conforme a distância
        if (dist < distanciaMinima)
            rb.linearVelocity = -dir * velocidade;          // recua
        else if (dist > distanciaMaxima)
            rb.linearVelocity = dir * velocidade;           // avança
        else
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, 0.25f); // freia
    }

    private void Atirar()
    {
        if (prefabProjetil == null)
        {
            Debug.LogWarning($"{name}: Prefab Projetil não atribuído!");
            return;
        }

        Vector2 dir    = ((Vector2)player.position - (Vector2)transform.position).normalized;
        float   angulo = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        GameObject obj = Instantiate(prefabProjetil, transform.position,
                                     Quaternion.Euler(0f, 0f, angulo));

        EnemyProjectile proj = obj.GetComponent<EnemyProjectile>();
        if (proj != null)
            proj.Configurar(danoProjetil, velocidadeProjetil, alcanceProjetil);

        timerDisparo = intervaloDisparo;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, distanciaMinima);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, distanciaMaxima);
    }
}
