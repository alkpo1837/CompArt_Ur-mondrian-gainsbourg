using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioSourcePlayer : MonoBehaviour
{
    [Header("Values")]
    public KeyCode Key;

    private AudioSource _audioSource;

    void Start()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (Input.GetKeyUp(Key))
        {
            _audioSource.Play();
        }
    }
}
