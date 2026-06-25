using UnityEngine;

/// <summary>
/// Controla o disparo da arma atual do jogador.
/// Adicione ao GameObject do Player junto com PlayerController.
/// A arma é trocada automaticamente pelo GameManager ao mudar de fase.
/// </summary>
public class WeaponController : MonoBehaviour
{
    [Header("Arma Atual")]
    public WeaponData armaAtual;

    [Header("Referências")]
    [Tooltip("Transform filho que marca de onde os projéteis saem")]
    public Transform pontoDeDisparo;
    [Tooltip("Necessário apenas quando tipoAtaque = LancaChamas")]
    public FlamethrowerAttack lancaChamas;

    [Header("Áudio (opcional)")]
    public AudioClip somDisparo;

    private float timerDisparo = 0f;

    void Update()
    {
        if (armaAtual == null) return;
        timerDisparo -= Time.deltaTime;

        switch (armaAtual.tipoAtaque)
        {
            case TipoAtaque.Normal:
                if (Input.GetMouseButtonDown(0) && timerDisparo <= 0f)
                    Disparar();
                break;

            case TipoAtaque.Automatico:
                if (Input.GetMouseButton(0) && timerDisparo <= 0f)
                    Disparar();
                break;

            case TipoAtaque.Shotgun:
                if (Input.GetMouseButtonDown(0) && timerDisparo <= 0f)
                    DispararShotgun();
                break;

            case TipoAtaque.LancaChamas:
                if (lancaChamas == null) break;
                if (Input.GetMouseButtonDown(0)) lancaChamas.Ativar();
                if (Input.GetMouseButtonUp(0))   lancaChamas.Desativar();
                break;
        }
    }

    // ── Disparo Normal / Automático ─────────────────────────────────

    private void Disparar()
    {
        if (!VerificarReferencias()) return;

        float angulo = AnguloParaMouse();
        GameObject obj = Instantiate(
            armaAtual.prefabProjetil,
            pontoDeDisparo.position,
            Quaternion.Euler(0f, 0f, angulo));

        ConfigurarProjetil(obj);
        AudioManager.Instance?.PlaySFX(somDisparo);
        timerDisparo = armaAtual.intervaloDisparo;
    }

    // ── Disparo em Cone (Shotgun) ───────────────────────────────────

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

        AudioManager.Instance?.PlaySFX(somDisparo);
        timerDisparo = armaAtual.intervaloDisparo;
    }

    // ── Auxiliares ──────────────────────────────────────────────────

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
        if (armaAtual.prefabProjetil == null)
        {
            Debug.LogWarning($"WeaponController: '{armaAtual.nomeArma}' não tem Prefab de Projétil!");
            return false;
        }
        if (pontoDeDisparo == null)
        {
            Debug.LogWarning("WeaponController: Ponto de Disparo não atribuído no Inspector!");
            return false;
        }
        return true;
    }

    // ── API pública (usada pelo GameManager) ────────────────────────

    /// <summary>
    /// Troca a arma ativa. Chamado pelo GameManager ao carregar nova fase.
    /// </summary>
    public void TrocarArma(WeaponData novaArma)
    {
        if (lancaChamas != null && lancaChamas.Ativo)
            lancaChamas.Desativar();

        armaAtual    = novaArma;
        timerDisparo = 0f;
    }
}
