using UnityEngine;

public class Projetil : MonoBehaviour
{
    public float velocidade = 10f;
    public float tempoDeVida = 3f; // Destrói a magia após 3 segundos para não pesar o jogo

    void Start()
    {
        Destroy(gameObject, tempoDeVida);
    }

    void Update()
    {
        // Move a magia sempre "para a frente"
        // Como o player roda para olhar para o mouse, a "frente" será sempre a direção certa!
        transform.Translate(Vector2.right * velocidade * Time.deltaTime);
    }
}