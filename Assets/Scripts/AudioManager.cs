using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [SerializeField]
        private AudioSource soundSource;

    [SerializeField] 
        private AudioSource musicSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        soundSource = GetComponent<AudioSource>();
    }

    [SerializeField]
    private AudioSet audioSet;
    
    public AudioClip GetAudioClip(int index)
    {
        return audioSet.GetSound(index);
    }

    public void PlayGlobalSound(int index)
    {
        soundSource.PlayOneShot(audioSet.GetSound(index));
    }
}
