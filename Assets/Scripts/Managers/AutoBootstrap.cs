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
        // Só aplica a arma inicial se o GameManager não tiver uma arma definida
        // (ou seja, o player entrou direto nessa cena sem passar pelo portal)
        if (armaInicial != null && GameManager.Instance != null && GameManager.Instance.armaAtual == null)
        {
            GameManager.Instance.armaAtual = armaInicial;
            GameManager.Instance.AplicarArmaPublico();
        }
    }
}
