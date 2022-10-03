using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    [SerializeField] private Slider[] volumeSlider;

    [SerializeField] private AudioMixer mixer;

    private static readonly string[] VolumeName = new[]
    {
        "MasterVolume", "MusicVolume", "SoundVolume"
    };
    
    
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < VolumeName.Length; i++)
        {
            mixer.GetFloat(VolumeName[i], out var value);

            volumeSlider[i].value = VolumeToLinear(value);
        }
    }

    public void SetMasterVolume(float value) => SetVolume(value, 0);

    public void SetMusicVolume(float value) => SetVolume(value, 1);

    public void SetSoundVolume(float value) => SetVolume(value, 2);

    private void SetVolume(float value, int index)
    {
        mixer.SetFloat(VolumeName[index], LinearToVolume(value));
    }

    private static float LinearToVolume(float value)
    {
        return value == 0 ? -80 : Mathf.Log10(value) * 20;
    }
    
    private static float VolumeToLinear(float value)
    {
        return value == -80 ? 0 : Mathf.Pow(10, value / 20);
    }
}
