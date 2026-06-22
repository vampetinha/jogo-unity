using UnityEngine;

public class SpellSystem : MonoBehaviour
{
    [Header("Configurações de Disparo")]
    public GameObject projetilPrefab;

    private ElementManager elementManager;

    void Start()
    {
        elementManager = GetComponent<ElementManager>();
    }

    void Update()
    {
        // 0 = Botão Esquerdo do Rato
        if (Input.GetMouseButtonDown(0))
        {
            AtirarMagia();
        }
    }

    void AtirarMagia()
    {
        if (elementManager.filaDeMagia.Count == 0) return;

        // 1. DESCOBRIR A POSIÇÃO DO RATO
        Vector3 posicaoRato = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // 2. CALCULAR A DIREÇÃO (Do Mago até ao Rato)
        Vector2 direcao = (posicaoRato - transform.position).normalized;

        // 3. CALCULAR O ÂNGULO EXATO
        float angulo = Mathf.Atan2(direcao.y, direcao.x) * Mathf.Rad2Deg;
        Quaternion rotacaoParaORato = Quaternion.Euler(0, 0, angulo);

        // 4. CRIAR A MAGIA JÁ VIRADA PARA O RATO
        GameObject novaMagia = Instantiate(projetilPrefab, transform.position, rotacaoParaORato);

        // Limpa a fila após atirar
        elementManager.LimparFila();
    }
}