using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class AudioSet : ScriptableObject
{
    [SerializeField] public AudioClip[] audioClips;

    public enum AudioID
    {
        BatImpact, Explosion, FuseLit
    };

    public AudioClip GetSound(int index)
    {
        return audioClips[index];
    }
}
