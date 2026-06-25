using UnityEngine;

/// <summary>
/// Instancia um número flutuante sempre que o objeto recebe dano.
/// Adicione a qualquer personagem com HealthSystem.
/// Crie o prefab com TextMeshPro (3D) + DamageNumber.
/// </summary>
[RequireComponent(typeof(HealthSystem))]
public class DamageNumberSpawner : MonoBehaviour
{
    [Header("Prefab")]
    public GameObject prefabNumero;

    [Header("Aparência")]
    public Color corDano   = new Color(1f, 0.25f, 0.25f);
    public Color corCura   = new Color(0.3f, 1f, 0.4f);
    public float offsetY   = 0.6f;

    private HealthSystem vida;
    private float        vidaAnterior;

    void Start()
    {
        vida         = GetComponent<HealthSystem>();
        vidaAnterior = vida.VidaAtual;
        vida.aoReceberDano.AddListener(AoReceberDano);
    }

    void OnDestroy()
    {
        if (vida != null) vida.aoReceberDano.RemoveListener(AoReceberDano);
    }

    private void AoReceberDano(float vidaAtual, float vidaMaxima)
    {
        if (prefabNumero == null) return;

        float delta = vidaAnterior - vidaAtual;
        vidaAnterior = vidaAtual;

        if (Mathf.Abs(delta) < 0.5f) return; // ignora valores irrisórios

        Vector3 pos  = transform.position + Vector3.up * offsetY;
        pos.x       += Random.Range(-0.25f, 0.25f); // evita sobreposição

        Color cor    = delta > 0f ? corDano : corCura; // dano=vermelho, cura=verde

        GameObject obj = Instantiate(prefabNumero, pos, Quaternion.identity);
        DamageNumber num = obj.GetComponent<DamageNumber>();
        if (num != null) num.Configurar(Mathf.Abs(delta), cor);
    }
}
