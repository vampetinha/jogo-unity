using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimento")]
    public float velocidade = 5f;

    private Rigidbody2D rb;
    private float anguloAlvo;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;

        // Impede que colisões girem o personagem involuntariamente
        rb.angularDamping = 1000f;
    }

    void Update()
    {
        // Calcula o ângulo para o mouse a cada frame (alta taxa de atualização)
        AtualizarAnguloMouse();
    }

    void FixedUpdate()
    {
        Mover();

        // Aplica a rotação via Rigidbody para ser consistente com a física
        rb.MoveRotation(anguloAlvo);
    }

    private void Mover()
    {
        // Lê WASD e setas — diagonal normalizada evita velocidade extra na diagonal
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        Vector2 direcao = new Vector2(x, y).normalized;
        rb.linearVelocity = direcao * velocidade;
    }

    private void AtualizarAnguloMouse()
    {
        if (Camera.main == null) return;

        Vector3 posRato = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direcao = (Vector2)posRato - (Vector2)transform.position;

        // -90f ajusta para o sprite que aponta para cima por padrão
        anguloAlvo = Mathf.Atan2(direcao.y, direcao.x) * Mathf.Rad2Deg - 90f;
    }
}
