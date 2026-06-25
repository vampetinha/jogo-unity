using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Coloque no GameObject do portal.
/// Precisa de: SpriteRenderer + Collider2D (Is Trigger) + opcional ParticleSystem.
/// O RoomController chama Ativar() quando a sala for limpa.
/// </summary>
public class PortalController : MonoBehaviour
{
    [Header("Próxima Fase")]
    [Tooltip("Nome EXATO da Scene a carregar (igual ao nome no Build Settings).")]
    public string proximaScene = "";

    [Tooltip("Nome da dimensão exibido na tela ao entrar (ex: 'MARTE'). Deixe vazio para não exibir.")]
    public string nomeDaDimensao = "";

    [Tooltip("Arma que o jogador receberá ao entrar nessa fase. Deixe vazio para manter a arma atual.")]
    public WeaponData armaProximaFase;

    [Header("Referências")]
    public ParticleSystem particulas;

    private bool ativo = false;
    private SpriteRenderer sr;
    private Collider2D col;

    void Awake()
    {
        sr  = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        DefinirEstado(false);
    }

    /// <summary>Chamado pelo RoomController quando todos os inimigos morreram.</summary>
    public void Ativar()
    {
        ativo = true;
        DefinirEstado(true);
        Debug.Log($"Portal ativado! Próxima cena: {proximaScene}");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (ativo && other.CompareTag("Player"))
            UsarPortal();
    }

    private void UsarPortal()
    {
        if (string.IsNullOrEmpty(proximaScene))
        {
            Debug.LogWarning("PortalController: campo 'Proxima Scene' está vazio!");
            return;
        }

        if (GameManager.Instance != null)
        {
            // Registra a arma da próxima fase antes de trocar a cena
            if (armaProximaFase != null)
                GameManager.Instance.armaAtual = armaProximaFase;

            GameManager.Instance.IrParaFase(proximaScene, nomeDaDimensao);
        }
        else
        {
            SceneManager.LoadScene(proximaScene); // fallback sem fade
        }
    }

    private void DefinirEstado(bool visivel)
    {
        if (sr  != null) sr.enabled  = visivel;
        if (col != null) col.enabled = visivel;

        if (particulas != null)
        {
            if (visivel) particulas.Play();
            else         particulas.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }
}
