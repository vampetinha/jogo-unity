using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PortalController : MonoBehaviour
{
    [Header("Próxima Fase")]
#if UNITY_EDITOR
    [Tooltip("Arraste a Scene aqui.")]
    public SceneAsset proximaSceneAsset;
#endif
    [HideInInspector]
    public string proximaScene = "";

    [Tooltip("Nome da dimensão exibido na tela ao entrar (ex: 'MARTE'). Deixe vazio para não exibir.")]
    public string nomeDaDimensao = "";

    [Tooltip("Arma que o jogador receberá ao entrar nessa fase. Deixe vazio para manter a arma atual.")]
    public WeaponData armaProximaFase;

    [Header("Detecção")]
    [Tooltip("Raio em unidades para detectar o player")]
    public float raioDeteccao = 1.2f;

    [Header("Referências")]
    public ParticleSystem particulas;

    private bool ativo = false;
    private SpriteRenderer sr;
    private Collider2D col;

    void OnValidate()
    {
#if UNITY_EDITOR
        if (proximaSceneAsset != null)
            proximaScene = proximaSceneAsset.name;
#endif
    }

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

    void Update()
    {
        if (!ativo) return;

        Collider2D hit = Physics2D.OverlapCircle(transform.position, raioDeteccao);
        if (hit != null && hit.CompareTag("Player"))
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
