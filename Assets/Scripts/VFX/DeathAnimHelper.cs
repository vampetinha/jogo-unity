using System.Collections;
using UnityEngine;

public class DeathAnimHelper : MonoBehaviour
{
    public void Iniciar(float duracao) => StartCoroutine(Animar(duracao));

    private IEnumerator Animar(float duracao)
    {
        SpriteRenderer sr     = GetComponent<SpriteRenderer>();
        Vector3        escala = transform.localScale;
        float          t      = 0f;

        while (t < duracao)
        {
            float p = t / duracao;
            transform.Rotate(0f, 0f, 400f * Time.deltaTime);
            transform.localScale = escala * (1f - p * 0.6f);
            if (sr != null) sr.color = new Color(1f, 0.4f, 0.4f, 1f - p);
            t += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}
