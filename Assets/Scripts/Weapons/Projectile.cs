using UnityEngine;

/// <summary>
/// Projétil usado pelo WeaponController.
/// Prefab precisa de: Rigidbody2D (gravityScale = 0) + CircleCollider2D (isTrigger = true).
/// Coloque o prefab no Layer "Projectile" e configure a Collision Matrix para
/// ignorar colisão entre "Projectile" e "Player".
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    private float      dano;
    private bool       atravessaInimigos;
    private Rigidbody2D rb;
    private WeaponData  armaData;

    void Awake()
    {
        rb              = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.angularDamping = 0f;
    }

    /// <summary>Chamado pelo WeaponController imediatamente após Instantiate.</summary>
    public void Configurar(float novoDano, float velocidade, float alcance, bool atravessa, Sprite sprite = null)
    {
        dano              = novoDano;
        atravessaInimigos = atravessa;

        if (sprite != null)
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null) sr.sprite = sprite;
        }

        rb.linearVelocity = transform.right * velocidade;

        Destroy(gameObject, alcance / Mathf.Max(0.1f, velocidade));
    }

    public void ConfigurarImpacto(WeaponData arma)
    {
        armaData = arma;
    }

    public void ConfigurarTrail(WeaponData arma)
    {
        if (arma == null || !arma.usarTrail) return;

        TrailRenderer trail   = gameObject.AddComponent<TrailRenderer>();
        trail.time            = arma.tempoTrail;
        trail.startWidth      = arma.larguraTrail;
        trail.endWidth        = 0f;
        trail.autodestruct    = false;
        trail.material        = new Material(Shader.Find("Sprites/Default"));

        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(arma.corTrailInicio, 0f),
                new GradientColorKey(arma.corTrailFim,    1f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(arma.corTrailInicio.a, 0f),
                new GradientAlphaKey(0f,                    1f)
            }
        );
        trail.colorGradient = grad;
    }

    public void ConfigurarParticulas(WeaponData arma)
    {
        if (arma == null || !arma.usarParticulasBuracoNegro) return;

        GameObject psObj = new GameObject("BuracoNegroPS");
        psObj.transform.SetParent(transform);
        psObj.transform.localPosition = Vector3.zero;

        ParticleSystem ps = psObj.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.loop            = true;
        main.playOnAwake     = true;
        main.startLifetime   = new ParticleSystem.MinMaxCurve(arma.vidaMinParticula, arma.vidaMaxParticula);
        main.startSpeed      = 0f;
        main.startSize       = new ParticleSystem.MinMaxCurve(arma.tamanhoMinParticula, arma.tamanhoMaxParticula);
        // startColor alterna entre as duas cores para partículas variadas no nascimento
        main.startColor = new ParticleSystem.MinMaxGradient(arma.corParticula, arma.corParticula2);
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.maxParticles    = arma.maxParticulas;

        var emission = ps.emission;
        emission.enabled      = true;
        emission.rateOverTime = arma.emissaoParticulas;

        var shape = ps.shape;
        shape.enabled         = true;
        shape.shapeType       = ParticleSystemShapeType.Circle;
        shape.radius          = arma.raioBuracoNegro;
        shape.radiusThickness = 0f; // emite só na borda do anel

        // Orbita em torno do Z + pull radial crescente no fim da vida
        var vel = ps.velocityOverLifetime;
        vel.enabled  = true;
        vel.space    = ParticleSystemSimulationSpace.Local;
        vel.orbitalZ = new ParticleSystem.MinMaxCurve(arma.velocidadeOrbital);

        // Curva radial: fica em 0 até metade da vida, depois puxa para o centro
        AnimationCurve curvaRadial = new AnimationCurve(
            new Keyframe(0f,   0f,  0f, 0f),
            new Keyframe(0.5f, 0f,  0f, 0f),
            new Keyframe(1f,  -1f,  0f, 0f));
        vel.radial = new ParticleSystem.MinMaxCurve(-arma.forcaCentro, curvaRadial);

        // Gradiente duplo: nasce na cor1, transiciona para cor2 no meio, some no fim
        var col = ps.colorOverLifetime;
        col.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(arma.corParticula,  0f),
                new GradientColorKey(arma.corParticula,  0.2f),
                new GradientColorKey(arma.corParticula2, 0.6f),
                new GradientColorKey(arma.corParticula2, 1f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(0f,   0f),
                new GradientAlphaKey(1f,   0.15f),
                new GradientAlphaKey(0.8f, 0.7f),
                new GradientAlphaKey(0f,   1f)
            }
        );
        col.color = grad;

        // Rotação fixa de todos os sprites — ajuste no Inspector até ficarem tangentes ao anel
        main.startRotation = new ParticleSystem.MinMaxCurve(arma.rotacaoParticula * Mathf.Deg2Rad);

        var rotOL = ps.rotationOverLifetime;
        rotOL.enabled = true;
        // orbitalZ gira o conjunto; velocidadeRotacaoParticula gira cada sprite individualmente
        rotOL.z = new ParticleSystem.MinMaxCurve(
            arma.velocidadeOrbital * Mathf.Deg2Rad + arma.velocidadeRotacaoParticula * Mathf.Deg2Rad,
            arma.velocidadeOrbital * Mathf.Deg2Rad - arma.velocidadeRotacaoParticula * Mathf.Deg2Rad);

        // Billboard: mostra a textura como ela é, sem esticar → formato redondo preservado
        var psRenderer = psObj.GetComponent<ParticleSystemRenderer>();
        psRenderer.renderMode   = ParticleSystemRenderMode.Billboard;
        psRenderer.sortingOrder = 5;

        Material mat = new Material(Shader.Find("Legacy Shaders/Particles/Additive"));
        mat.mainTexture = CriarTexturaMeiaLua(64);
        psRenderer.material = mat;
    }

    private static Texture2D CriarTexturaMeiaLua(int tam)
    {
        Texture2D tex = new Texture2D(tam, tam, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;

        float centro    = tam * 0.5f;
        float raioExt   = tam * 0.44f;
        float raioInt   = tam * 0.28f;
        float desloc    = tam * 0.16f;
        float suavExt   = tam * 0.10f; // borda externa bem suave → arredondada
        float suavInt   = tam * 0.08f; // borda interna suave

        for (int y = 0; y < tam; y++)
        {
            for (int x = 0; x < tam; x++)
            {
                float dx = x - centro;
                float dy = y - centro;

                float distExt = Mathf.Sqrt(dx * dx + dy * dy);
                float distInt = Mathf.Sqrt((dx - desloc) * (dx - desloc) + dy * dy);

                // Suavidade Gaussiana nas bordas — sem corte abrupto
                float aExt = Mathf.Clamp01((raioExt - distExt) / suavExt);
                float aInt = Mathf.Clamp01((distInt - raioInt) / suavInt);
                float alpha = aExt * aExt * aInt * aInt; // quadrático = mais suave nas pontas

                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        tex.Apply();
        return tex;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) return;

        if (other.CompareTag("Enemy"))
        {
            HealthSystem vida = other.GetComponent<HealthSystem>();
            if (vida != null) vida.ReceberDano(dano);

            if (!atravessaInimigos)
            {
                SpawnarSucao();
                Destroy(gameObject);
            }
            return;
        }

        if (!other.isTrigger)
        {
            SpawnarSucao();
            Destroy(gameObject);
        }
    }

    private void SpawnarSucao()
    {
        if (armaData != null && armaData.usarSucaoImpacto)
            SucaoBuracoNegro.Spawnar(transform.position, armaData);
    }
}
