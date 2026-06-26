using System.Collections;
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

    [Header("Comportamento")]
    [Tooltip("Tempo em segundos antes de fechar após a sala ser ativada.")]
    public float delayParaFechar = 0.6f;

    [Tooltip("Marque se a porta deve começar fechada.")]
    public bool comecarFechada = false;

    private Collider2D    col;
    private SpriteRenderer sr;
    private Coroutine      rotinaDeFechamento;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        sr  = GetComponent<SpriteRenderer>();

        if (comecarFechada) Fechar();
        else                Abrir();
    }

    // ── API pública ───────────────────────────────

    public void Abrir()
    {
        // Cancela qualquer fechamento pendente
        if (rotinaDeFechamento != null)
        {
            StopCoroutine(rotinaDeFechamento);
            rotinaDeFechamento = null;
        }

        if (col != null) col.enabled = false;
        if (sr  != null) sr.color    = corAberta;
    }

    public void Fechar()
    {
        if (col != null) col.enabled = true;
        if (sr  != null) sr.color    = corFechada;
    }

    /// <summary>
    /// Fecha a porta após um pequeno delay, dando tempo do player entrar.
    /// </summary>
    public void FecharComDelay()
    {
        if (rotinaDeFechamento != null)
            StopCoroutine(rotinaDeFechamento);

        rotinaDeFechamento = StartCoroutine(RotinaFechar());
    }

    IEnumerator RotinaFechar()
    {
        yield return new WaitForSeconds(delayParaFechar);
        Fechar();
        rotinaDeFechamento = null;
    }
}
