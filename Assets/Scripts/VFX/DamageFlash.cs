using System.Collections;
using UnityEngine;

/// <summary>
/// Faz o sprite piscar vermelho ao receber dano.
/// Adicione a qualquer inimigo ou ao Player com SpriteRenderer + HealthSystem.
/// </summary>
[RequireComponent(typeof(SpriteRenderer), typeof(HealthSystem))]
public class DamageFlash : MonoBehaviour
{
    [Header("Flash Visual")]
    public Color   corFlash       = new Color(1f, 0.18f, 0.18f);
    public float   duracaoFlash   = 0.10f;
    public int     numeroFlashes  = 2;

    [Header("Áudio (opcional)")]
    public AudioClip somDano;

    private SpriteRenderer sr;
    private HealthSystem   vida;
    private Color          corOriginal;
    private Coroutine      flashAtivo;

    void Awake()
    {
        sr           = GetComponent<SpriteRenderer>();
        vida         = GetComponent<HealthSystem>();
        corOriginal  = sr.color;
    }

    void Start()
    {
        vida.aoReceberDano.AddListener(AoReceberDano);
    }

    void OnDestroy()
    {
        if (vida != null) vida.aoReceberDano.RemoveListener(AoReceberDano);
    }

    public void CancelarFlash()
    {
        if (flashAtivo != null) { StopCoroutine(flashAtivo); flashAtivo = null; }
        if (sr != null) sr.color = corOriginal;
    }

    private void AoReceberDano(float atual, float maximo)
    {
        AudioManager.Instance?.PlaySFX(somDano);

        if (flashAtivo != null) StopCoroutine(flashAtivo);
        flashAtivo = StartCoroutine(Piscar());
    }

    private IEnumerator Piscar()
    {
        for (int i = 0; i < numeroFlashes; i++)
        {
            sr.color = corFlash;
            yield return new WaitForSeconds(duracaoFlash);
            sr.color = corOriginal;
            if (i < numeroFlashes - 1)
                yield return new WaitForSeconds(duracaoFlash * 0.4f);
        }
        flashAtivo = null;
    }
}
