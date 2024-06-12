using UnityEngine;

public class SFXPlayer : MonoBehaviour
{
    [SerializeField]
    private AudioSource _audioSource;

    public static SFXPlayer Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            _audioSource = GetComponent<AudioSource>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlaySFX(AudioClip clip, float volume = 1f, Vector2 pos = default)
    {
        _audioSource.transform.position = pos;
        _audioSource.PlayOneShot(clip, volume);
    }
}