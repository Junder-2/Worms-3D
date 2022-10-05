using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class AudioSet : ScriptableObject
{
    [SerializeField] public AudioClip[] audioClips;
    [SerializeField] public AudioClip[] songClips;

    public enum AudioID
    {
        batImpact, explosion, fuseLit,
        
        uiClickA, uiClickB,

        hghWuh1, hghWuh2, hiya1, hiya2, hmmrgh, oww, ow, ughhYa1, ughhYa2, waah
    };

    public enum MusicID
    {
        menu, game
    };

    public AudioClip GetSound(int index)
    {
        return audioClips[index];
    }

    public AudioClip GetMusic(int index)
    {
        return songClips[index];
    }

}
