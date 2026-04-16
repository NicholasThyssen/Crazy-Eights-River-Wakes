using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [System.Serializable]
    public class Sound
    {
        public SoundName name;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
    }

    [Header("Sounds")]
    public List<Sound> sounds = new List<Sound>();

    private Dictionary<SoundName, Sound> soundDict;
    private AudioSource audioSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();


        soundDict = new Dictionary<SoundName, Sound>();
        foreach (var sound in sounds)
        {
            if (!soundDict.ContainsKey(sound.name))
            {
                soundDict.Add(sound.name, sound);
            }
        }
    }

    public void Play(SoundName soundName)
    {
        if (!soundDict.TryGetValue(soundName, out Sound sound))
        {
            Debug.LogWarning($"Sound not found: {soundName}");
            return;
        }

        audioSource.PlayOneShot(sound.clip, sound.volume);
    }

    public void Stop()
    {
        audioSource.Stop();
    }

}

public enum SoundName
{
    PlaceCard,
    Skip,
    CardSwap,
}