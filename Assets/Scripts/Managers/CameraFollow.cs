using UnityEngine;

/// <summary>
/// Adicione na Main Camera.
/// Arraste o Transform do Player no campo Alvo.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("Alvo")]
    public Transform alvo;

    [Header("Suavização")]
    [Tooltip("Quanto menor, mais rápido a câmera acompanha (0.05 = suave, 0.01 = colado)")]
    public float suavizacao = 0.08f;

    [Header("Offset")]
    [Tooltip("Deslocamento da câmera em relação ao player (deixe Z = -10)")]
    public Vector3 offset = new Vector3(0f, 0f, -10f);

    private Vector3 velocidade = Vector3.zero;

    void LateUpdate()
    {
        if (alvo == null) return;

        Vector3 posAlvo = alvo.position + offset;
        transform.position = Vector3.SmoothDamp(
            transform.position,
            posAlvo,
            ref velocidade,
            suavizacao
        );
    }
}
