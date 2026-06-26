using UnityEngine;
using UnityEngine.Events;

public class WeaponController : MonoBehaviour
{
    [Header("Arma Atual")]
    public WeaponData armaAtual;

    [Header("Referências")]
    [Tooltip("Transform filho que representa a arma (orbita ao redor do player)")]
    public Transform pontoDeDisparo;
    [Tooltip("Necessário apenas quando tipoAtaque = LancaChamas")]
    public FlamethrowerAttack lancaChamas;

    [Header("Órbita da Arma")]
    [Tooltip("Distância da arma ao centro do player")]
    public float raioOrbita = 1f;

    [Header("Visual da Arma na Mão")]
    [Tooltip("SpriteRenderer filho do PontoDeDisparo que exibe o sprite da arma")]
    public SpriteRenderer spriteNaMao;

    // HUDManager escuta para atualizar o ícone
    [HideInInspector]
    public UnityEvent<Sprite> onSpriteArmaAlterado = new UnityEvent<Sprite>();

    private float timerDisparo = 0f;

    void Start()
    {
        AtualizarVisual();
    }

    // Atualiza o visual quando armaAtual é trocada no Inspector (Editor e Play mode)
    void OnValidate()
    {
        if (spriteNaMao == null || armaAtual == null) return;
        spriteNaMao.sprite = armaAtual.sprite;
        spriteNaMao.transform.localScale = Vector3.one * armaAtual.escalaNaMao;
    }

    void Update()
    {
        AtualizarOrbita();

        if (armaAtual == null) return;
        timerDisparo -= Time.deltaTime;

        switch (armaAtual.tipoAtaque)
        {
            case TipoAtaque.Normal:
                if (Input.GetMouseButtonDown(0) && timerDisparo <= 0f) Disparar();
                break;

            case TipoAtaque.Automatico:
                if (Input.GetMouseButton(0) && timerDisparo <= 0f) Disparar();
                break;

            case TipoAtaque.Shotgun:
                if (Input.GetMouseButtonDown(0) && timerDisparo <= 0f) DispararShotgun();
                break;

            case TipoAtaque.LancaChamas:
                if (lancaChamas == null) break;
                if (Input.GetMouseButtonDown(0)) lancaChamas.Ativar();
                if (Input.GetMouseButtonUp(0))   lancaChamas.Desativar();
                break;
        }
    }

    // ── Órbita ──────────────────────────────────────────────────────

    private void AtualizarOrbita()
    {
        if (Camera.main == null || pontoDeDisparo == null) return;

        Vector3 posRato = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        posRato.z = 0f;

        Vector2 direcao = ((Vector2)posRato - (Vector2)transform.position).normalized;

        // Posiciona a arma no raio ao redor do player
        pontoDeDisparo.position = (Vector2)transform.position + direcao * raioOrbita;

        // Rotaciona a arma para apontar em direção ao mouse
        float angulo = Mathf.Atan2(direcao.y, direcao.x) * Mathf.Rad2Deg;
        pontoDeDisparo.rotation = Quaternion.Euler(0f, 0f, angulo);

        // Vira o sprite verticalmente quando a arma aponta para a esquerda
        // evita que o sprite apareça de cabeça pra baixo
        if (spriteNaMao != null)
            spriteNaMao.flipY = direcao.x < 0f;
    }

    // ── Disparo Normal / Automático ──────────────────────────────────

    private void Disparar()
    {
        if (!VerificarReferencias()) return;

        float angulo = AnguloParaMouse();
        GameObject obj = Instantiate(
            armaAtual.prefabProjetil,
            pontoDeDisparo.position,
            Quaternion.Euler(0f, 0f, angulo));

        ConfigurarProjetil(obj);
        AudioManager.Instance?.PlaySFX(armaAtual.somDisparo);
        timerDisparo = armaAtual.intervaloDisparo;
    }

    // ── Disparo em Cone (Shotgun) ────────────────────────────────────

    private void DispararShotgun()
    {
        if (!VerificarReferencias()) return;

        float anguloBase = AnguloParaMouse();
        int   qtd        = Mathf.Max(1, armaAtual.quantidadeProjeteis);

        for (int i = 0; i < qtd; i++)
        {
            float offset = qtd > 1
                ? Mathf.Lerp(-armaAtual.dispersao * 0.5f,
                              armaAtual.dispersao * 0.5f,
                              (float)i / (qtd - 1))
                : 0f;

            GameObject obj = Instantiate(
                armaAtual.prefabProjetil,
                pontoDeDisparo.position,
                Quaternion.Euler(0f, 0f, anguloBase + offset));

            ConfigurarProjetil(obj);
        }

        AudioManager.Instance?.PlaySFX(armaAtual.somDisparo);
        timerDisparo = armaAtual.intervaloDisparo;
    }

    // ── Auxiliares ───────────────────────────────────────────────────

    private void ConfigurarProjetil(GameObject obj)
    {
        Projectile proj = obj.GetComponent<Projectile>();
        if (proj != null)
            proj.Configurar(
                armaAtual.dano,
                armaAtual.velocidadeProjetil,
                armaAtual.alcance,
                armaAtual.atravessaInimigos);
    }

    private float AnguloParaMouse()
    {
        if (Camera.main == null || pontoDeDisparo == null) return 0f;
        Vector3 posRato = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dir = (Vector2)posRato - (Vector2)pontoDeDisparo.position;
        return Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
    }

    private bool VerificarReferencias()
    {
        if (armaAtual == null || armaAtual.prefabProjetil == null)
        {
            Debug.LogWarning($"WeaponController: sem prefab de projétil!");
            return false;
        }
        if (pontoDeDisparo == null)
        {
            Debug.LogWarning("WeaponController: PontoDeDisparo não atribuído!");
            return false;
        }
        return true;
    }

    private void AtualizarVisual()
    {
        Sprite s = armaAtual != null ? armaAtual.sprite : null;

        if (spriteNaMao != null)
        {
            spriteNaMao.sprite = s;

            // Aplica o tamanho configurado no ScriptableObject da arma
            if (armaAtual != null)
                spriteNaMao.transform.localScale = Vector3.one * armaAtual.escalaNaMao;
        }

        onSpriteArmaAlterado?.Invoke(s);
    }

    // ── API pública ──────────────────────────────────────────────────

    public void TrocarArma(WeaponData novaArma)
    {
        if (lancaChamas != null && lancaChamas.Ativo)
            lancaChamas.Desativar();

        armaAtual    = novaArma;
        timerDisparo = 0f;
        AtualizarVisual();
    }
}
