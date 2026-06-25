using UnityEngine;

public class SpellSystem : MonoBehaviour
{
    [Header("Configurações de Disparo")]
    public GameObject projetilPrefab;

    [Tooltip("Local de onde a magia será criada")]
    public Transform pontoDeDisparo;

    [Header("Sistema de Elementos")]
    public ElementManager elementManager;

    [Header("Lança-Chamas (Fogo)")]
    [SerializeField] private FlamethrowerAttack lancaChamas;

    private void Update()
    {
        bool fogoSelecionado = elementManager != null &&
                               elementManager.filaDeMagia.Count > 0 &&
                               elementManager.filaDeMagia[0] == ElementoMagico.Fogo;

        // Verdadeiro somente no frame em que o jogador pressiona 2
        bool pressionouFogo = Input.GetKeyDown(KeyCode.Alpha2);

        // Tecla 2: ativa o lança-chamas se a barra estiver pronta
        if (pressionouFogo)
        {
            if (lancaChamas == null)
                Debug.LogWarning("FlamethrowerAttack não atribuído no SpellSystem.");
            else if (lancaChamas.PodeAtivar())
                lancaChamas.Ativar();
            else if (lancaChamas.EmRecarga)
                Debug.Log("Superaquecido! Aguarde a barra recarregar.");
        }

        // Desativa ao trocar de elemento — mas NUNCA no mesmo frame que ativou
        // (fogoSelecionado ainda é false nesse frame porque o ElementManager
        //  processa a tecla na sua própria ordem de execução)
        if (!pressionouFogo && !fogoSelecionado && lancaChamas != null && lancaChamas.Ativo)
            lancaChamas.Desativar();

        // Projétil normal para Água, Terra e Vento (LMB)
        if (!pressionouFogo && !fogoSelecionado && Input.GetMouseButtonDown(0))
            AtirarMagia();
    }

    private void AtirarMagia()
    {
        if (elementManager == null)
        {
            Debug.LogError("Associe o ElementManager no SpellSystem.");
            return;
        }

        if (projetilPrefab == null)
        {
            Debug.LogError("Associe o Projetil Prefab no SpellSystem.");
            return;
        }

        if (pontoDeDisparo == null)
        {
            Debug.LogError("Associe o Ponto de Disparo no SpellSystem.");
            return;
        }

        if (Camera.main == null)
        {
            Debug.LogError("A câmera principal precisa estar com a tag MainCamera.");
            return;
        }

        // Nenhuma magia selecionada
        if (elementManager.filaDeMagia.Count == 0)
        {
            Debug.Log("Selecione uma magia antes de atirar.");
            return;
        }

        ElementoMagico magiaUsada =
            elementManager.filaDeMagia[0];

        Vector3 posicaoRato =
            Camera.main.ScreenToWorldPoint(Input.mousePosition);

        posicaoRato.z = 0f;

        Vector2 direcao =
            (posicaoRato - pontoDeDisparo.position).normalized;

        float angulo =
            Mathf.Atan2(direcao.y, direcao.x) * Mathf.Rad2Deg;

        Quaternion rotacao =
            Quaternion.Euler(0f, 0f, angulo);

        GameObject obj = Instantiate(projetilPrefab, pontoDeDisparo.position, rotacao);

        Projetil projetil = obj.GetComponent<Projetil>();
        if (projetil != null)
            projetil.elemento = magiaUsada;

        Debug.Log("Magia lançada: " + magiaUsada);

        elementManager.LimparFila();
    }
}