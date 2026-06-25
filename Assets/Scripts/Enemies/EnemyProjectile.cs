using UnityEngine;

/// <summary>
/// Projétil disparado pelos inimigos — causa dano ao Player.
/// Prefab: Rigidbody2D (gravityScale=0) + CircleCollider2D (isTrigger=true).
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyProjectile : MonoBehaviour
{
    private float dano;
    private Rigidbody2D rb;

    void Awake()
    {
        rb              = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.angularDamping = 0f;
    }

    /// <summary>Chamado pelo EnemyShooter logo após Instantiate.</summary>
    public void Configurar(float novoDano, float velocidade, float alcance)
    {
        dano = novoDano;
        rb.linearVelocity = transform.right * velocidade;
        Destroy(gameObject, alcance / Mathf.Max(0.1f, velocidade));
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy")) return; // não acerta inimigos

        if (other.CompareTag("Player"))
        {
            HealthSystem vida = other.GetComponent<HealthSystem>();
            if (vida != null) vida.ReceberDano(dano);
            Destroy(gameObject);
            return;
        }

        if (!other.isTrigger)
            Destroy(gameObject); // parede ou obstáculo
    }
}
