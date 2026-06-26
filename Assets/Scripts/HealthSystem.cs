using UnityEngine;
using UnityEngine.Events;

public class HealthSystem : MonoBehaviour
{
    [Header("Vida")]
    public float vidaMaxima = 100f;

    [Tooltip("Se marcado, Destroy NÃO é chamado automaticamente ao morrer. Use apenas no Boss.")]
    public bool morteManual = false;

    private float vidaAtual;

    // Disparado ao receber dano: (vidaAtual, vidaMaxima)
    public UnityEvent<float, float> aoReceberDano;
    public UnityEvent aoMorrer;

    void Awake()
    {
        vidaAtual = vidaMaxima;
    }

    private bool invencivel = false;

    /// <summary>Enquanto invencível, todo dano é ignorado.</summary>
    public void SetInvencivel(bool valor) => invencivel = valor;
    public bool EstaInvencivel => invencivel;

    public void ReceberDano(float dano)
    {
        if (invencivel) return;

        vidaAtual = Mathf.Max(0f, vidaAtual - dano);
        aoReceberDano?.Invoke(vidaAtual, vidaMaxima);

        if (vidaAtual <= 0f)
            Morrer();
    }

    private void Morrer()
    {
        aoMorrer?.Invoke();
        if (!morteManual) Destroy(gameObject);
    }

    public float VidaAtual  => vidaAtual;
    public float VidaMaxima => vidaMaxima;

    /// <summary>Recupera vida sem ultrapassar o máximo.</summary>
    public void Curar(float quantidade)
    {
        vidaAtual = Mathf.Min(vidaMaxima, vidaAtual + quantidade);
        aoReceberDano?.Invoke(vidaAtual, vidaMaxima);
    }

    /// <summary>
    /// Define vida e vidaMaxima diretamente.
    /// Usado pelo GameManager para restaurar a vida entre cenas.
    /// </summary>
    public void DefinirVida(float novaVida, float novaVidaMaxima)
    {
        vidaMaxima = novaVidaMaxima;
        vidaAtual  = Mathf.Clamp(novaVida, 0f, vidaMaxima);
        aoReceberDano?.Invoke(vidaAtual, vidaMaxima);
    }
}
