using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(HealthSystem))]
public class EnemyController : MonoBehaviour
{
    [Header("Movimento")]
    public float velocidade = 2f;
    public float distanciaDeAtaque = 1.2f;

    [Header("Ataque")]
    public float danoDeAtaque = 10f;
    public float intervaloDeAtaque = 1f;

    [Header("Elemento")]
    public ElementoMagico elemento = ElementoMagico.Fogo;

    private Transform player;
    private Rigidbody2D rb;
    private float timerAtaque = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    void Update()
    {
        if (player == null) return;

        timerAtaque -= Time.deltaTime;

        if (DistanciaAoPlayer() <= distanciaDeAtaque && timerAtaque <= 0f)
        {
            Atacar();
            timerAtaque = intervaloDeAtaque;
        }
    }

    void FixedUpdate()
    {
        if (player == null) return;

        if (DistanciaAoPlayer() > distanciaDeAtaque)
        {
            Vector2 direcao = ((Vector2)player.position - rb.position).normalized;
            rb.MovePosition(rb.position + direcao * velocidade * Time.fixedDeltaTime);

            float angulo = Mathf.Atan2(direcao.y, direcao.x) * Mathf.Rad2Deg;
            rb.rotation = angulo - 90f;
        }
    }

    private void Atacar()
    {
        HealthSystem vidaPlayer = player.GetComponent<HealthSystem>();
        if (vidaPlayer != null)
            vidaPlayer.ReceberDano(danoDeAtaque);

        Debug.Log($"{gameObject.name} atacou o player! Dano: {danoDeAtaque}");
    }

    private float DistanciaAoPlayer()
    {
        return Vector2.Distance(rb.position, (Vector2)player.position);
    }
}
