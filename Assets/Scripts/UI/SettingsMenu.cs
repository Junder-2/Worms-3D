using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace UI
{
    public class SettingsMenu : MonoBehaviour
    {
        [SerializeField] private Slider[] volumeSlider;

        [SerializeField] private AudioMixer mixer;

        private static readonly string[] VolumeName =
        {
            "MasterVolume", "MusicVolume", "SoundVolume"
        };
    
        public void Setup()
        {
            if (PlayerPrefs.HasKey(VolumeName[0]))
            {
                for (int i = 0; i < VolumeName.Length; i++)
                {
                    float value = VolumeToLinear(PlayerPrefs.GetFloat(VolumeName[i]));

                    volumeSlider[i].value = value;
                
                    SetVolume(value, i);
                }
            }
            else
            {
                for (int i = 0; i < VolumeName.Length; i++)
                {
                    mixer.GetFloat(VolumeName[i], out var value);

                    volumeSlider[i].value = VolumeToLinear(value);
                }
            }
        }
    
        public void SaveSettings()
        {
            foreach (var t in VolumeName)
            {
                mixer.GetFloat(t, out var value);
            
                PlayerPrefs.SetFloat(t, value);
            }

            PlayerPrefs.Save();
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
}
