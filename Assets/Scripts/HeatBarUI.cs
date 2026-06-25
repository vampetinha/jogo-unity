using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HeatBarUI : MonoBehaviour
{
    [Header("Referências")]
    public FlamethrowerAttack lancaChamas;
    public Slider slider;
    public Image imagemFill;

    [Tooltip("Texto de status (opcional — TMP_Text). Exibe PRONTO / ATIVO / SUPERAQUECIDO")]
    public TMP_Text textoStatus;

    [Header("Cores")]
    public Color corPronto    = new Color(1.00f, 0.55f, 0.00f); // laranja
    public Color corAtivo     = new Color(1.00f, 0.25f, 0.00f); // laranja-vivo
    public Color corCritica   = new Color(1.00f, 0.05f, 0.05f); // vermelho
    public Color corRecarga   = new Color(0.40f, 0.40f, 0.40f); // cinza

    [Range(0f, 1f)]
    [Tooltip("Porcentagem de calor onde a barra vira vermelha")]
    public float limiarCritico = 0.3f;

    [Header("Suavização")]
    [Tooltip("Velocidade com que a barra acompanha o valor real")]
    public float velocidadeLerp = 8f;

    private float valorAlvo;

    void Start()
    {
        if (lancaChamas == null)
        {
            Debug.LogError("HeatBarUI: arraste o FlamethrowerAttack no Inspector.");
            return;
        }

        if (slider != null)
        {
            slider.minValue = 0f;
            slider.maxValue = lancaChamas.CalorMaximo;
            slider.value    = lancaChamas.CalorMaximo;
            slider.interactable = false; // jogador não arrasta a barra
        }

        valorAlvo = lancaChamas.CalorMaximo;

        lancaChamas.onCalorAlterado.AddListener(OnCalorAlterado);
        AtualizarVisual(lancaChamas.CalorAtual, lancaChamas.CalorMaximo);
    }

    void OnDestroy()
    {
        if (lancaChamas != null)
            lancaChamas.onCalorAlterado.RemoveListener(OnCalorAlterado);
    }

    void Update()
    {
        // Suaviza o preenchimento da barra para não pular abruptamente
        if (slider != null)
            slider.value = Mathf.Lerp(slider.value, valorAlvo, Time.deltaTime * velocidadeLerp);
    }

    private void OnCalorAlterado(float atual, float maximo)
    {
        valorAlvo = atual;
        AtualizarVisual(atual, maximo);
    }

    private void AtualizarVisual(float atual, float maximo)
    {
        // --- Cor do preenchimento ---
        if (imagemFill != null)
        {
            Color alvo;

            if (lancaChamas.EmRecarga)
                alvo = corRecarga;
            else if (lancaChamas.Ativo)
                alvo = corAtivo;
            else if (maximo > 0f && atual / maximo <= limiarCritico)
                alvo = corCritica;
            else
                alvo = corPronto;

            // Transição suave de cor
            imagemFill.color = Color.Lerp(imagemFill.color, alvo, Time.deltaTime * velocidadeLerp);
        }

        // --- Texto de status ---
        if (textoStatus == null) return;

        if (lancaChamas.EmRecarga)
            textoStatus.text = "SUPERAQUECIDO";
        else if (lancaChamas.Ativo)
            textoStatus.text = "ATIVO";
        else if (maximo > 0f && atual >= maximo)
            textoStatus.text = "PRONTO";
        else
            textoStatus.text = "RECARREGANDO";
    }
}
