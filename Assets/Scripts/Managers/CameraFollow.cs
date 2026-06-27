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

    private Vector3 velocidade  = Vector3.zero;
    private float   shakeTimer  = 0f;
    private float   shakeForce  = 0f;

    public static CameraFollow Instance { get; private set; }

    void Awake() => Instance = this;

    void LateUpdate()
    {
        if (alvo == null) return;

        Vector3 posAlvo = alvo.position + offset;

        if (shakeTimer > 0f)
        {
            posAlvo += (Vector3)Random.insideUnitCircle * shakeForce;
            shakeTimer -= Time.deltaTime;
        }

        transform.position = Vector3.SmoothDamp(
            transform.position,
            posAlvo,
            ref velocidade,
            suavizacao
        );
    }

    public void Shake(float intensidade, float duracao)
    {
        shakeForce = intensidade;
        shakeTimer = duracao;
    }
}
