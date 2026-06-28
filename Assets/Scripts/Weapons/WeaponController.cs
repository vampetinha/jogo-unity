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

    [Header("Katana (Botão Direito)")]
    [Tooltip("WeaponData da katana — deixe vazio para desativar")]
    public WeaponData katana;

    [Header("Katana — Visual")]
    [Tooltip("Sprite do slash que aparece na direção do ataque")]
    public Sprite spriteSlash;
    [Tooltip("Cor da ponta do slash (borda que chega por último — mais viva)")]
    public Color corSlashLidando = new Color(1f, 0.95f, 0.4f, 1f);
    [Tooltip("Cor do rastro do slash (borda inicial — mais apagada)")]
    public Color corSlashRastro  = new Color(0.3f, 0.75f, 1f, 0.2f);
    [Tooltip("Duração do efeito de slash em segundos")]
    public float duracaoSlash    = 0.18f;
    [Tooltip("Quantidade de sprites que formam o arco do slash")]
    public int   fatiasSlash     = 7;
    [Tooltip("Escala do sprite de slash (1 = tamanho original)")]
    public float escalaSlash     = 1f;
    [Tooltip("Distância do centro do player até os sprites do slash")]
    public float raioSlash       = 1.1f;

    // HUDManager escuta para atualizar o ícone
    [HideInInspector]
    public UnityEvent<Sprite> onSpriteArmaAlterado = new UnityEvent<Sprite>();

    private float timerDisparo = 0f;
    private float timerKatana  = 0f;

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

        timerKatana -= Time.deltaTime;
        if (katana != null && Input.GetMouseButtonDown(1) && timerKatana <= 0f)
            AtacarKatana();

        if (armaAtual == null) return;
        timerDisparo -= Time.deltaTime;

        switch (armaAtual.tipoAtaque)
        {
            case TipoAtaque.Normal:
                if (Input.GetMouseButtonDown(0) && timerDisparo <= 0f) Disparar();
                break;

            case TipoAtaque.Automatico:
                if (Input.GetMouseButtonDown(0))
                    StartCoroutine(TocarSomLoop(armaAtual.somDisparo, armaAtual.delayAudio));
                if (Input.GetMouseButtonUp(0))
                    AudioManager.Instance?.StopSFXLoop();
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
        if (armaAtual.tipoAtaque != TipoAtaque.Automatico)
            StartCoroutine(TocarSomDisparo(armaAtual.somDisparo, armaAtual.delayAudio));
        timerDisparo = armaAtual.intervaloDisparo;
    }

    // ── Disparo em Cone (Shotgun) ────────────────────────────────────

    private void DispararShotgun()
    {
        if (!VerificarReferencias()) return;

        float anguloBase = AnguloParaMouse();
        int   qtd        = Mathf.Max(1, armaAtual.quantidadeProjeteis);
        float meioLeque  = armaAtual.dispersao * 0.5f;

        for (int i = 0; i < qtd; i++)
        {
            float posNoLeque = qtd > 1 ? Mathf.Lerp(-meioLeque, meioLeque, (float)i / (qtd - 1)) : 0f;
            float ruido      = Random.Range(-armaAtual.dispersaoAleatoria, armaAtual.dispersaoAleatoria);

            GameObject obj = Instantiate(
                armaAtual.prefabProjetil,
                pontoDeDisparo.position,
                Quaternion.Euler(0f, 0f, anguloBase + posNoLeque + ruido));

            ConfigurarProjetil(obj);
        }

        // Recuo no player (empurra pra direção oposta ao tiro)
        if (armaAtual.forcaRecuo > 0f)
        {
            Rigidbody2D rbPlayer = GetComponent<Rigidbody2D>();
            if (rbPlayer != null)
            {
                Vector2 direcaoTiro = new Vector2(
                    Mathf.Cos(anguloBase * Mathf.Deg2Rad),
                    Mathf.Sin(anguloBase * Mathf.Deg2Rad));
                rbPlayer.AddForce(-direcaoTiro * armaAtual.forcaRecuo, ForceMode2D.Impulse);
            }
        }

        CameraFollow.Instance?.Shake(armaAtual.shakeIntensidade, armaAtual.shakeDuracao);
        StartCoroutine(TocarSomDisparo(armaAtual.somDisparo, armaAtual.delayAudio));
        timerDisparo = armaAtual.intervaloDisparo;
    }

    // ── Katana ───────────────────────────────────────────────────────

    private void AtacarKatana()
    {
        if (Camera.main == null) return;

        Vector3 posRato = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direcaoMouse = ((Vector2)posRato - (Vector2)transform.position).normalized;

        Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position, katana.raioAtaqueMelee);
        foreach (Collider2D col in cols)
        {
            if (col.CompareTag("Player")) continue;

            Vector2 direcaoInimigo = ((Vector2)col.transform.position - (Vector2)transform.position).normalized;
            if (Vector2.Angle(direcaoMouse, direcaoInimigo) > katana.anguloAtaqueMelee * 0.5f) continue;

            HealthSystem vida = col.GetComponent<HealthSystem>();
            if (vida != null) vida.ReceberDano(katana.dano);

            EnemyController inimigo = col.GetComponent<EnemyController>();
            if (inimigo != null) inimigo.AplicarKnockback(direcaoInimigo * katana.forcaKnockback);
        }

        AudioManager.Instance?.PlaySFX(katana.somDisparo);
        timerKatana = katana.intervaloDisparo;

        StartCoroutine(AnimacaoKatana(direcaoMouse));
    }

    private System.Collections.IEnumerator AnimacaoKatana(Vector2 direcao)
    {
        SpriteRenderer srPlayer = GetComponent<SpriteRenderer>();
        if (srPlayer != null) srPlayer.color = new Color(1.6f, 1.6f, 1.6f, 1f);
        if (spriteNaMao != null) spriteNaMao.enabled = false;

        if (spriteSlash != null)
        {
            float anguloBase = Mathf.Atan2(direcao.y, direcao.x) * Mathf.Rad2Deg;
            float meioArco   = katana.anguloAtaqueMelee * 0.5f;
            int   sortLayer  = srPlayer?.sortingLayerID ?? 0;
            int   sortOrder  = (srPlayer?.sortingOrder  ?? 0) + 1;

            // Sprite da lâmina que varre o arco
            GameObject laminaObj = new GameObject("SlashLamina");
            SpriteRenderer srLamina = laminaObj.AddComponent<SpriteRenderer>();
            srLamina.sprite         = spriteSlash;
            srLamina.color          = corSlashLidando;
            srLamina.sortingLayerID = sortLayer;
            srLamina.sortingOrder   = sortOrder + 1;

            float tempo = 0f;
            while (tempo < duracaoSlash)
            {
                float t      = tempo / duracaoSlash;
                float angulo = Mathf.Lerp(anguloBase + meioArco, anguloBase - meioArco, t);
                float rad    = angulo * Mathf.Deg2Rad;
                float escala = escalaSlash * Mathf.Lerp(0.8f, 1.3f, t);
                Vector2 pos  = (Vector2)transform.position +
                               new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * raioSlash;

                laminaObj.transform.position   = pos;
                laminaObj.transform.rotation   = Quaternion.Euler(0f, 0f, angulo);
                laminaObj.transform.localScale = Vector3.one * escala;

                // Rastro: cor vai da lâmina até corSlashRastro conforme o t avança
                Color corRastro = Color.Lerp(corSlashLidando, corSlashRastro, t);
                SpawnRastroSlash(pos, angulo, escala, corRastro, sortLayer, sortOrder);

                tempo += Time.deltaTime;
                yield return null;
            }

            Destroy(laminaObj);
        }

        if (spriteNaMao != null) spriteNaMao.enabled = true;
        if (srPlayer != null) srPlayer.color = Color.white;
    }

    private void SpawnRastroSlash(Vector2 pos, float angulo, float escala, Color cor, int sortLayer, int sortOrder)
    {
        GameObject rastro = new GameObject("SlashRastro");
        rastro.transform.position   = pos;
        rastro.transform.rotation   = Quaternion.Euler(0f, 0f, angulo);
        rastro.transform.localScale = Vector3.one * escala;

        SpriteRenderer srR = rastro.AddComponent<SpriteRenderer>();
        srR.sprite         = spriteSlash;
        srR.color          = cor;
        srR.sortingLayerID = sortLayer;
        srR.sortingOrder   = sortOrder;

        StartCoroutine(FadeSlashFatia(srR, cor, duracaoSlash * 0.4f));
    }

    private System.Collections.IEnumerator FadeSlashFatia(SpriteRenderer sr, Color corInicial, float duracao)
    {
        float tempo = 0f;
        while (tempo < duracao)
        {
            if (sr == null) yield break;
            float t = tempo / duracao;
            sr.color = new Color(corInicial.r, corInicial.g, corInicial.b, Mathf.Lerp(corInicial.a, 0f, t));
            tempo += Time.deltaTime;
            yield return null;
        }
        if (sr != null) Destroy(sr.gameObject);
    }

    // ── Auxiliares ───────────────────────────────────────────────────

    private void ConfigurarProjetil(GameObject obj)
    {
        Projectile proj = obj.GetComponent<Projectile>();
        if (proj != null)
        {
            proj.Configurar(
                armaAtual.dano,
                armaAtual.velocidadeProjetil,
                armaAtual.alcance,
                armaAtual.atravessaInimigos,
                armaAtual.spriteProjetil);
            proj.ConfigurarTrail(armaAtual);
            proj.ConfigurarParticulas(armaAtual);
            proj.ConfigurarImpacto(armaAtual);
        }
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
        AudioManager.Instance?.StopSFXLoop();

        armaAtual    = novaArma;
        timerDisparo = 0f;
        AtualizarVisual();
    }

    private System.Collections.IEnumerator TocarSomDisparo(AudioClip clip, float delay)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);
        AudioManager.Instance?.PlaySFXDisparo(clip);
    }

    private System.Collections.IEnumerator TocarSomLoop(AudioClip clip, float delay)
    {
        AudioManager.Instance?.PlaySFXLoop(clip, delay);
        yield break;
    }
}
