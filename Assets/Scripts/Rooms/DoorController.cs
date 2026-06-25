using UnityEngine;

/// <summary>
/// Coloque em cada porta da sala.
/// Precisa de: SpriteRenderer + Collider2D.
/// </summary>
public class DoorController : MonoBehaviour
{
    [Header("Visual")]
    public Color corFechada = new Color(0.15f, 0.15f, 0.15f);
    public Color corAberta  = new Color(0.2f, 0.8f, 0.2f, 0f); // transparente

    [Tooltip("Marque se a porta deve começar fechada (ex: portão inicial)")]
    public bool comecarFechada = false;

    private Collider2D col;
    private SpriteRenderer sr;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        sr  = GetComponent<SpriteRenderer>();

        if (comecarFechada) Fechar();
        else                Abrir();
    }

    public void Abrir()
    {
        if (col != null) col.enabled = false;
        if (sr  != null) sr.color    = corAberta;
    }

    public void Fechar()
    {
        if (col != null) col.enabled = true;
        if (sr  != null) sr.color    = corFechada;
    }
}
