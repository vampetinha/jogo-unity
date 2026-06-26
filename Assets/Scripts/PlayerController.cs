using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimento")]
    public float velocidade = 5f;

    [Header("Dash")]
    [Tooltip("Tecla para ativar o dash")]
    public KeyCode teclaDash = KeyCode.Space;
    [Tooltip("Velocidade do dash")]
    public float forcaDash = 18f;
    [Tooltip("Duração do dash em segundos")]
    public float duracaoDash = 0.15f;
    [Tooltip("Tempo de espera entre dashes")]
    public float cooldownDash = 0.7f;

    private Rigidbody2D rb;
    private bool  emDash       = false;
    private float timerCooldown = 0f;
    private Vector2 direcaoAtual = Vector2.right;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale   = 0f;
        rb.freezeRotation = true;
    }

    void Update()
    {
        timerCooldown -= Time.deltaTime;

        if (Input.GetKeyDown(teclaDash) && !emDash && timerCooldown <= 0f)
            StartCoroutine(ExecutarDash());
    }

    void FixedUpdate()
    {
        if (emDash) return; // dash controla a velocidade, não o movimento normal

        Mover();
    }

    private void Mover()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        Vector2 direcao = new Vector2(x, y).normalized;

        // Guarda a última direção para o dash funcionar mesmo parado
        if (direcao != Vector2.zero)
            direcaoAtual = direcao;

        rb.linearVelocity = direcao * velocidade;
    }

    private IEnumerator ExecutarDash()
    {
        emDash        = true;
        timerCooldown = cooldownDash;

        // Ativa invencibilidade
        HealthSystem vida = GetComponent<HealthSystem>();
        if (vida != null) vida.SetInvencivel(true);

        rb.linearVelocity = direcaoAtual * forcaDash;

        // Pisca o sprite durante o dash para indicar invencibilidade
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        float tempo = 0f;
        while (tempo < duracaoDash)
        {
            if (sr != null)
                sr.color = new Color(1f, 1f, 1f, sr.color.a > 0.5f ? 0.25f : 1f);
            yield return new WaitForSeconds(0.05f);
            tempo += 0.05f;
        }

        // Restaura sprite e desativa invencibilidade
        if (sr != null) sr.color = Color.white;
        if (vida != null) vida.SetInvencivel(false);

        emDash = false;
    }
}
