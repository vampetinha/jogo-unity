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

    [Header("Katana / Melee (ignorado em outros tipos)")]
    [Tooltip("Alcance do ataque em unidades")]
    public float raioAtaqueMelee  = 2f;
    [Tooltip("Arco de ataque em graus na direção do mouse")]
    public float anguloAtaqueMelee = 120f;
    [Tooltip("Força de knockback aplicada nos inimigos atingidos")]
    public float forcaKnockback   = 12f;
}
