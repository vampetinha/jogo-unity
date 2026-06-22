using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Atributos do Mago")]
    public float velocidadeMovimento = 5f;

    private Rigidbody2D rb;
    private Vector2 destino;
    private bool estaSeMovendo = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;

        // O personagem começa parado na sua posição inicial
        destino = rb.position;
    }

    void Update()
    {
        // 1. CAPTURAR O CLIQUE (Botão Direito do Mouse)
        if (Input.GetMouseButton(1))
        {
            // Converte a posição do mouse no ecrã para o mundo 2D
            Vector3 posicaoClique = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            destino = new Vector2(posicaoClique.x, posicaoClique.y);
            estaSeMovendo = true;
        }
    }

    void FixedUpdate()
    {
        // 2. MOVER O PERSONAGEM
        if (estaSeMovendo)
        {
            // MoveTowards move o mago do ponto A ao B de forma suave
            rb.position = Vector2.MoveTowards(rb.position, destino, velocidadeMovimento * Time.fixedDeltaTime);

            // 3. ROTACIONAR PARA ONDE ESTÁ A ANDAR
            Vector2 direcaoOlhar = destino - rb.position;

            // Só roda se ainda estiver longe do destino (evita que fique a tremer)
            if (direcaoOlhar.sqrMagnitude > 0.01f)
            {
                float angulo = Mathf.Atan2(direcaoOlhar.y, direcaoOlhar.x) * Mathf.Rad2Deg;
                rb.rotation = angulo - 90f; // Ajuste para o sprite ficar de frente
            }

            // 4. PARAR AO CHEGAR
            if (Vector2.Distance(rb.position, destino) < 0.1f)
            {
                estaSeMovendo = false;
            }
        }
    }
}