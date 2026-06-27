using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteAnimator : MonoBehaviour
{
    [Header("Idle — Pulso de Escala")]
    public bool  idleAtivo       = true;
    public float amplitudePulso  = 0.03f;
    public float velocidadePulso = 1.2f;

    [Header("Movimento — Flip Horizontal")]
    public bool flipAtivo = true;

    [Header("Morte — Giro e Fade")]
    public float duracaoMorte = 0.45f;

    private SpriteRenderer sr;
    private Rigidbody2D    rb;
    private Vector3        escalaOriginal;

    void Awake()
    {
        sr             = GetComponent<SpriteRenderer>();
        rb             = GetComponent<Rigidbody2D>();
        escalaOriginal = transform.localScale;
    }

    void Start()
    {
        HealthSystem vida = GetComponent<HealthSystem>();
        if (vida != null) vida.aoMorrer.AddListener(AnimarMorte);

        if (idleAtivo) StartCoroutine(PulsarIdle());
    }

    void Update()
    {
        if (!flipAtivo || rb == null) return;
        if      (rb.linearVelocity.x >  0.1f) sr.flipX = false;
        else if (rb.linearVelocity.x < -0.1f) sr.flipX = true;
    }

    private IEnumerator PulsarIdle()
    {
        // Offset aleatório para que mobs próximos não pulsem sincronizados
        float t = Random.Range(0f, Mathf.PI * 2f);
        while (true)
        {
            t += Time.deltaTime * velocidadePulso;
            float s = 1f + Mathf.Sin(t) * amplitudePulso;
            transform.localScale = escalaOriginal * s;
            yield return null;
        }
    }

    private void AnimarMorte()
    {
        // Cria um "fantasma" para animar independente do Destroy do objeto original
        GameObject ghost = new GameObject("DeathAnim");
        ghost.transform.SetPositionAndRotation(transform.position, transform.rotation);
        ghost.transform.localScale = transform.localScale;

        SpriteRenderer srG = ghost.AddComponent<SpriteRenderer>();
        srG.sprite         = sr.sprite;
        srG.flipX          = sr.flipX;
        srG.sortingLayerID = sr.sortingLayerID;
        srG.sortingOrder   = sr.sortingOrder;
        srG.color          = sr.color;

        ghost.AddComponent<DeathAnimHelper>().Iniciar(duracaoMorte);
    }
}
