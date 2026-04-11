using UnityEngine;
using UnityEngine.Audio;

public class SettingsMenu : MonoBehaviour
{
    public AudioMixer mixer;
    public void OnCardSFXVolumeChanged(float value)
    {
        SettingsData.SetCardSfxVolume(value);
        float calcedVolume = (value > 0) ? Mathf.Lerp(-80.0f, 0.0f, value/100.0f) : -80.0f;
        mixer.SetFloat("CardVolume", calcedVolume);
    }

    public void OnEnvSFXVolumeChanged(float value)
    {
        SettingsData.SetEnvSfxVolume(value);
        float calcedVolume = (value > 0) ? Mathf.Lerp(-80.0f, 0.0f, value/100.0f) : -80.0f;
        mixer.SetFloat("EnvVolume", calcedVolume);        
    }
}