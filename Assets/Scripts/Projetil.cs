using UnityEngine;

public class Projetil : MonoBehaviour
{
    public float velocidade = 10f;
    public float tempoDeVida = 3f;
    public float dano = 25f;

    [HideInInspector]
    public ElementoMagico elemento;

    void Start()
    {
        Destroy(gameObject, tempoDeVida);
    }

    void Update()
    {
        transform.Translate(Vector2.right * velocidade * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;

        EnemyController inimigo = other.GetComponent<EnemyController>();
        HealthSystem vida = other.GetComponent<HealthSystem>();

        if (vida != null)
        {
            float multiplicador = inimigo != null ? CalcularMultiplicador(inimigo.elemento) : 1f;
            vida.ReceberDano(dano * multiplicador);

            if (multiplicador > 1f)
                Debug.Log($"Golpe eficaz! {elemento} vs {inimigo?.elemento} → x{multiplicador}");
        }

        Destroy(gameObject);
    }

    // Fraquezas: Fogo > Vento > Terra > Água > Fogo
    private float CalcularMultiplicador(ElementoMagico elementoInimigo)
    {
        bool eFraqueza =
            (elemento == ElementoMagico.Fogo  && elementoInimigo == ElementoMagico.Vento) ||
            (elemento == ElementoMagico.Vento && elementoInimigo == ElementoMagico.Terra) ||
            (elemento == ElementoMagico.Terra && elementoInimigo == ElementoMagico.Agua)  ||
            (elemento == ElementoMagico.Agua  && elementoInimigo == ElementoMagico.Fogo);

        return eFraqueza ? 1.5f : 1f;
    }
}