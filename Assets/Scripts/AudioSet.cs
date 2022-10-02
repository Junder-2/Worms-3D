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
        BatImpact, Explosion, FuseLit,

        HghWuh1, HghWuh2, Hiya1, Hiya2, Hmmrgh, Oww, Ow, UghhYa1, UghhYa2, Waah
    };

    public enum MusicID
    {
        Menu, Game
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
