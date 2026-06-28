using UnityEngine;

public enum TipoAtaque
{
    Normal,      // Um tiro por clique
    Automatico,  // Atira enquanto o botão é pressionado
    Shotgun,     // Múltiplos projéteis em cone por clique
    LancaChamas, // Delega ao FlamethrowerAttack
    Katana       // Ataque melee em arco, botão direito
}

/// <summary>
/// ScriptableObject que define os atributos de uma arma.
/// Crie via: Assets > Create > Jogo > Arma
/// </summary>
[CreateAssetMenu(fileName = "NovaArma", menuName = "Jogo/Arma")]
public class WeaponData : ScriptableObject
{
    [Header("Identificação")]
    public string    nomeArma   = "Nova Arma";
    public Sprite    sprite;
    [Tooltip("Tamanho do sprite da arma na mão do player")]
    public float     escalaNaMao = 0.3f;
    [Tooltip("Som tocado ao disparar")]
    public AudioClip somDisparo;
    [Tooltip("Delay em segundos antes de tocar o som de disparo (útil para sincronizar com animações)")]
    public float delayAudio = 0f;

    [Header("Tipo de Ataque")]
    public TipoAtaque tipoAtaque = TipoAtaque.Normal;

    [Header("Projétil")]
    public GameObject prefabProjetil;
    [Tooltip("Sprite visual do projétil (deixe vazio para usar o sprite do prefab)")]
    public Sprite spriteProjetil;
    public float dano              = 25f;
    public float velocidadeProjetil = 12f;
    [Tooltip("Distância máxima percorrida antes de sumir")]
    public float alcance           = 8f;
    [Tooltip("O projétil atravessa inimigos (ex: plasma)")]
    public bool  atravessaInimigos = false;

    [Header("Cadência")]
    [Tooltip("Segundos mínimos entre disparos")]
    public float intervaloDisparo = 0.25f;

    [Header("Shotgun (ignorado em outros tipos)")]
    public int   quantidadeProjeteis = 8;
    [Tooltip("Abertura total do cone em graus")]
    public float dispersao           = 40f;
    [Tooltip("Variação aleatória extra por pellet em graus")]
    public float dispersaoAleatoria  = 5f;
    [Tooltip("Força de recuo aplicada no player ao atirar")]
    public float forcaRecuo          = 6f;
    [Tooltip("Intensidade do shake de câmera")]
    public float shakeIntensidade    = 0.3f;
    [Tooltip("Duração do shake de câmera em segundos")]
    public float shakeDuracao        = 0.15f;

    [Header("Rastro do Projétil (Trail)")]
    [Tooltip("Ativa o rastro visual no projétil desta arma")]
    public bool  usarTrail      = false;
    [Tooltip("Cor no início do rastro (perto do projétil)")]
    public Color corTrailInicio = new Color(0.4f, 0.8f, 1f, 1f);
    [Tooltip("Cor no fim do rastro (cauda)")]
    public Color corTrailFim    = new Color(0.1f, 0.4f, 1f, 0f);
    [Tooltip("Largura do rastro na origem")]
    public float larguraTrail   = 0.12f;
    [Tooltip("Duração do rastro em segundos")]
    public float tempoTrail     = 0.12f;

    [Header("Partículas Orbitais (Buraco Negro)")]
    [Tooltip("Ativa partículas que orbitam ao redor do projétil")]
    public bool  usarParticulasBuracoNegro = false;

    [Tooltip("Cor no início da vida da partícula")]
    public Color corParticula        = new Color(0.55f, 0f, 1f, 1f);
    [Tooltip("Cor no meio/fim da vida da partícula (gradiente entre as duas)")]
    public Color corParticula2       = new Color(0f, 0.2f, 1f, 1f);
    [Tooltip("Velocidade de rotação individual de cada partícula em graus/segundo (independente da órbita)")]
    public float velocidadeRotacaoParticula = 90f;
    [Tooltip("Rotação fixa do sprite de cada partícula em graus — gire até a meia-lua ficar tangente ao anel (tente 90)")]
    public float rotacaoParticula = 0f;

    [Tooltip("Raio do anel de órbita das partículas")]
    public float raioBuracoNegro     = 0.25f;

    [Tooltip("Velocidade de rotação das partículas em graus/segundo")]
    public float velocidadeOrbital   = 270f;

    [Tooltip("Partículas emitidas por segundo")]
    public float emissaoParticulas   = 10f;

    [Tooltip("Limite máximo de partículas simultâneas")]
    public int   maxParticulas       = 40;

    [Tooltip("Tamanho mínimo de cada partícula")]
    public float tamanhoMinParticula = 0.07f;
    [Tooltip("Tamanho máximo de cada partícula")]
    public float tamanhoMaxParticula = 0.12f;

    [Tooltip("Tempo de vida mínimo de cada partícula em segundos")]
    public float vidaMinParticula    = 0.7f;
    [Tooltip("Tempo de vida máximo de cada partícula em segundos")]
    public float vidaMaxParticula    = 1.1f;

    [Tooltip("Comprimento do arco — quanto a partícula se estica na direção da órbita")]
    public float comprimentoArco     = 0.18f;
    [Tooltip("Escala extra de comprimento do arco")]
    public float escalaArco          = 1f;
    [Tooltip("Força com que o fim do arco é puxado para o centro (0 = sem curva, maior = arco mais fechado)")]
    public float forcaCentro         = 1.5f;

    [Header("Sucção no Impacto")]
    [Tooltip("Ao colidir, cria uma zona que puxa inimigos e objetos para o ponto de impacto")]
    public bool  usarSucaoImpacto = false;
    [Tooltip("Raio da zona de sucção em unidades")]
    public float raioSucao        = 3f;
    [Tooltip("Força de atração aplicada por segundo")]
    public float forcaSucao       = 18f;
    [Tooltip("Duração da sucção em segundos")]
    public float duracaoSucao     = 1.2f;

    [Header("Katana / Melee (ignorado em outros tipos)")]
    [Tooltip("Alcance do ataque em unidades")]
    public float raioAtaqueMelee  = 2f;
    [Tooltip("Arco de ataque em graus na direção do mouse")]
    public float anguloAtaqueMelee = 120f;
    [Tooltip("Força de knockback aplicada nos inimigos atingidos")]
    public float forcaKnockback   = 12f;
}
