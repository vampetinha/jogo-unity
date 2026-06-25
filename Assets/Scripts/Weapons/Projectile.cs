using UnityEngine;

/// <summary>
/// Projétil usado pelo WeaponController.
/// Prefab precisa de: Rigidbody2D (gravityScale = 0) + CircleCollider2D (isTrigger = true).
/// Coloque o prefab no Layer "Projectile" e configure a Collision Matrix para
/// ignorar colisão entre "Projectile" e "Player".
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    private float dano;
    private bool  atravessaInimigos;
    private Rigidbody2D rb;

    void Awake()
    {
        rb              = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.angularDamping = 0f;
    }

    /// <summary>Chamado pelo WeaponController imediatamente após Instantiate.</summary>
    public void Configurar(float novoDano, float velocidade, float alcance, bool atravessa)
    {
        dano              = novoDano;
        atravessaInimigos = atravessa;

        rb.linearVelocity = transform.right * velocidade;

        // Destrói automaticamente ao atingir o alcance máximo
        Destroy(gameObject, alcance / Mathf.Max(0.1f, velocidade));
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) return;

        if (other.CompareTag("Enemy"))
        {
            HealthSystem vida = other.GetComponent<HealthSystem>();
            if (vida != null) vida.ReceberDano(dano);

            if (!atravessaInimigos) Destroy(gameObject);
            return;
        }

        // Colliders sólidos (paredes, obstáculos) destroem o projétil
        // Triggers (salas, portais) são ignorados
        if (!other.isTrigger)
            Destroy(gameObject);
    }
}
