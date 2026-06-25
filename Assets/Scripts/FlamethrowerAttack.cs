using UnityEngine;
using UnityEngine.Events;

public class FlamethrowerAttack : MonoBehaviour
{
    [Header("Referências")]
    [Tooltip("Ponto de onde as chamas saem")]
    public Transform pontoDeDisparo;

    [Tooltip("ParticleSystem filho do pontoDeDisparo")]
    public ParticleSystem particulasChamas;

    // --- Dano ---
    [Header("Dano")]
    public float danoPorSegundo = 40f;
    public float intervaloEntreDanos = 0.2f;
    public LayerMask layerInimigos;

    // --- Alcance ---
    [Header("Alcance e Forma")]
    public float alcance = 3f;
    public float larguraDoJato = 0.8f;

    // --- Superaquecimento ---
    [Header("Superaquecimento")]
    [Tooltip("Calor máximo (barra cheia)")]
    public float calorMaximo = 100f;

    [Tooltip("Calor consumido por segundo enquanto ativo")]
    public float taxaDrenagem = 20f;

    [Tooltip("Calor recuperado por segundo enquanto inativo")]
    public float taxaRecarga = 10f;

    // Evento disparado sempre que o calor muda: (calorAtual, calorMaximo)
    // A HeatBarUI escuta este evento para atualizar a barra na tela.
    public UnityEvent<float, float> onCalorAlterado;

    // --- Propriedades públicas de leitura ---
    public bool Ativo => ativo;
    public bool EmRecarga => emRecarga;
    public float CalorAtual => calorAtual;
    public float CalorMaximo => calorMaximo;

    // --- Estado interno ---
    private bool ativo = false;

    // Verdadeiro quando a barra esvaziou: impede reativação até recarregar 100%
    private bool emRecarga = false;

    private float calorAtual;
    private float timerDano = 0f;
    private Vector2 direcaoAtual = Vector2.right;

    void Awake()
    {
        calorAtual = calorMaximo;

        if (particulasChamas != null)
        {
            var main = particulasChamas.main;
            main.playOnAwake = false;
            main.loop = true;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            // Força parada imediata, mesmo que Play On Awake esteja marcado no Editor
            particulasChamas.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    void Update()
    {
        if (ativo)
            AtualizarAtivo();
        else
            AtualizarRecarga();
    }

    // Lógica executada enquanto o lança-chamas está disparando
    private void AtualizarAtivo()
    {
        AtualizarDirecao();

        // Drena o calor
        calorAtual -= taxaDrenagem * Time.deltaTime;
        calorAtual = Mathf.Max(0f, calorAtual);
        onCalorAlterado?.Invoke(calorAtual, calorMaximo);

        // Aplica dano em intervalos fixos
        timerDano -= Time.deltaTime;
        if (timerDano <= 0f)
        {
            AplicarDano();
            timerDano = intervaloEntreDanos;
        }

        // Calor esgotado: desliga e entra em modo recarga
        if (calorAtual <= 0f)
            DesligarPorEsgotamento();
    }

    // Lógica executada enquanto inativo: recarrega o calor
    private void AtualizarRecarga()
    {
        // Libera o travamento mesmo que a barra já estivesse cheia ao entrar aqui
        if (emRecarga && calorAtual >= calorMaximo)
        {
            emRecarga = false;
            onCalorAlterado?.Invoke(calorAtual, calorMaximo);
            return;
        }

        if (calorAtual >= calorMaximo) return;

        calorAtual += taxaRecarga * Time.deltaTime;
        calorAtual = Mathf.Min(calorMaximo, calorAtual);
        onCalorAlterado?.Invoke(calorAtual, calorMaximo);

        if (emRecarga && calorAtual >= calorMaximo)
        {
            emRecarga = false;
            onCalorAlterado?.Invoke(calorAtual, calorMaximo);
        }
    }

    // Retorna true se o lança-chamas pode ser ativado agora
    // emRecarga só volta a false quando a barra está 100% cheia
    public bool PodeAtivar()
    {
        return !emRecarga;
    }

    public void Ativar()
    {
        if (ativo) return;
        if (!PodeAtivar()) return;

        ativo = true;
        timerDano = 0f;

        if (particulasChamas != null && !particulasChamas.isPlaying)
            particulasChamas.Play();
    }

    public void Desativar()
    {
        if (!ativo) return;

        ativo = false;

        if (particulasChamas != null && particulasChamas.isPlaying)
            particulasChamas.Stop();
    }

    // Desliga porque o calor acabou (diferente de trocar de elemento)
    private void DesligarPorEsgotamento()
    {
        ativo = false;
        emRecarga = true;

        if (particulasChamas != null && particulasChamas.isPlaying)
            particulasChamas.Stop();

        Debug.Log("Lança-chamas superaquecido! Aguarde a barra recarregar.");
    }

    private void AtualizarDirecao()
    {
        if (Camera.main == null || pontoDeDisparo == null) return;

        Vector3 posRato = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        posRato.z = 0f;

        direcaoAtual = ((Vector2)posRato - (Vector2)pontoDeDisparo.position).normalized;

        if (particulasChamas != null)
        {
            Vector3 dir = new Vector3(direcaoAtual.x, direcaoAtual.y, 0f);
            particulasChamas.transform.rotation = Quaternion.FromToRotation(Vector3.forward, dir);
        }
    }

    private void AplicarDano()
    {
        if (pontoDeDisparo == null) return;

        Vector2 centro = (Vector2)pontoDeDisparo.position + direcaoAtual * (alcance * 0.5f);
        float anguloCapsula = Vector2.SignedAngle(Vector2.up, direcaoAtual);

        Collider2D[] atingidos = Physics2D.OverlapCapsuleAll(
            centro,
            new Vector2(larguraDoJato, alcance),
            CapsuleDirection2D.Vertical,
            anguloCapsula,
            layerInimigos
        );

        float danoAplicado = danoPorSegundo * intervaloEntreDanos;

        foreach (Collider2D col in atingidos)
        {
            HealthSystem vida = col.GetComponent<HealthSystem>();
            if (vida != null)
                vida.ReceberDano(danoAplicado);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (pontoDeDisparo == null) return;

        Gizmos.color = new Color(1f, 0.35f, 0f, 0.5f);
        Vector2 origem = pontoDeDisparo.position;
        Vector2 fim = origem + direcaoAtual * alcance;

        Gizmos.DrawLine(origem, fim);
        Gizmos.DrawWireSphere(origem, larguraDoJato * 0.5f);
        Gizmos.DrawWireSphere(fim, larguraDoJato * 0.5f);
    }
}
