using UnityEngine;

public class MusicaTrigger : MonoBehaviour
{
    [Tooltip("Música tocada ao entrar nesta fase")]
    public AudioClip musica;

    void Start()
    {
        if (musica != null)
            AudioManager.Instance?.PlayMusica(musica);
    }
}
