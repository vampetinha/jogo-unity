using UnityEngine;

/// <summary>
/// Adicione em um GameObject vazio em cada cena que precisa dos managers.
/// Garante que GameManager, HUDManager e AudioManager existam mesmo
/// quando a cena é aberta diretamente no editor.
/// </summary>
public class AutoBootstrap : MonoBehaviour
{
    [Header("Configuração do HUD (usado só se HUDManager não existir)")]
    public string nomeCenaPrincipal = "FaseTerra";
    public string nomeSceneMenu     = "MenuNovo";

    [Header("Arma desta fase (aplicada quando a cena é aberta diretamente)")]
    [Tooltip("Arraste o WeaponData da arma que o player deve usar nesta fase.")]
    public WeaponData armaInicial;

    [Header("Música desta cena")]
    [Tooltip("Música tocada enquanto o player estiver nesta cena.")]
    public AudioClip musicaDaCena;

    void Awake()
    {
        if (GameManager.Instance == null)
            new GameObject("GameManager").AddComponent<GameManager>();

        if (AudioManager.Instance == null)
            new GameObject("AudioManager").AddComponent<AudioManager>();

        if (HUDManager.Instance == null)
        {
            GameObject obj = new GameObject("HUDManager");
            HUDManager hud = obj.AddComponent<HUDManager>();
            hud.nomeCenaPrincipal = nomeCenaPrincipal;
            hud.nomeSceneMenu     = nomeSceneMenu;
        }
    }

    void Start()
    {
        if (armaInicial != null && GameManager.Instance != null && GameManager.Instance.armaAtual == null)
        {
            GameManager.Instance.armaAtual = armaInicial;
            GameManager.Instance.AplicarArmaPublico();
        }

        if (musicaDaCena != null)
            AudioManager.Instance?.PlayMusica(musicaDaCena);
    }
}
