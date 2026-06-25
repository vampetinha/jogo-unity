using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Número flutuante que sobe e desvanece ao colidir.
/// O prefab precisa de: TextMeshPro (componente 3D, não UI) e este script.
/// Tamanho de fonte recomendado: 1.2 (unidades de mundo).
/// </summary>
[RequireComponent(typeof(TextMeshPro))]
public class DamageNumber : MonoBehaviour
{
    [Header("Animação")]
    public float velocidadeSubida = 1.8f;
    public float duracaoVida      = 0.75f;

    private TextMeshPro tmp;

    public void Configurar(float dano, Color cor)
    {
        tmp       = GetComponent<TextMeshPro>();
        tmp.text  = Mathf.RoundToInt(dano).ToString();
        tmp.color = cor;

        // Garante que o texto sempre fique de frente para a câmera
        transform.rotation = Camera.main != null
            ? Camera.main.transform.rotation
            : Quaternion.identity;

        StartCoroutine(Animar());
    }

    private IEnumerator Animar()
    {
        Vector3 posInicial  = transform.position;
        Color   corInicial  = tmp.color;
        float   t           = 0f;

        while (t < duracaoVida)
        {
            t += Time.deltaTime;
            float prog = t / duracaoVida;

            // Sobe mais rápido no início, desacelera no final
            transform.position = posInicial + Vector3.up * velocidadeSubida * prog * (1f - prog * 0.5f);

            // Fade out na segunda metade
            Color c = corInicial;
            c.a = Mathf.Clamp01(1f - (prog - 0.4f) / 0.6f);
            tmp.color = c;

            yield return null;
        }

        Destroy(gameObject);
    }
}
