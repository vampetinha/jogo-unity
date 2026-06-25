using UnityEngine;

/// <summary>
/// Instancia um efeito visual (partícula, sprite, etc.) quando o personagem morre.
/// Adicione a qualquer inimigo com HealthSystem.
/// Funciona tanto com morteManual=false (inimigos) quanto com morteManual=true (boss).
/// </summary>
[RequireComponent(typeof(HealthSystem))]
public class DeathVFX : MonoBehaviour
{
    [Header("Efeito de Morte")]
    public GameObject prefabVFX;

    [Header("Áudio (opcional)")]
    public AudioClip somMorte;

    void Start()
    {
        HealthSystem vida = GetComponent<HealthSystem>();
        if (vida != null) vida.aoMorrer.AddListener(SpawnarVFX);
    }

    private void SpawnarVFX()
    {
        AudioManager.Instance?.PlaySFX(somMorte);

        if (prefabVFX != null)
            Instantiate(prefabVFX, transform.position, Quaternion.identity);
    }
}
