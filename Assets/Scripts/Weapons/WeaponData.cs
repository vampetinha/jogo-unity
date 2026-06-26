using UnityEngine;

public enum TipoAtaque
{
    Normal,      // Um tiro por clique
    Automatico,  // Atira enquanto o botão é pressionado
    Shotgun,     // Múltiplos projéteis em cone por clique
    LancaChamas  // Delega ao FlamethrowerAttack
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
    public int   quantidadeProjeteis = 5;
    [Tooltip("Abertura total do cone em graus")]
    public float dispersao           = 30f;
}
