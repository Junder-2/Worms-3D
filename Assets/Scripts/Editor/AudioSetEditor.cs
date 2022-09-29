using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AudioSet))]
public class AudioSetEditor : Editor
{

    private readonly int size = Enum.GetValues(typeof(AudioSet.AudioID)).Length;

    private SerializedProperty _audioClips;

    private void OnEnable()
    {
        AudioSet audioSet = (AudioSet)target;
        if (audioSet.audioClips.Length != size)
            audioSet.audioClips = new AudioClip[size];
        
        _audioClips = serializedObject.FindProperty("audioClips");
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();

        for (int i = 0; i < size; i++)
        {
            /*_audioSet.audioClips[i] = (AudioClip)EditorGUILayout.ObjectField(((AudioSet.AudioID)i).ToString(),
                _audioSet.audioClips[i], typeof(AudioClip), false);*/

            var clip = _audioClips.GetArrayElementAtIndex(i);

            EditorGUILayout.LabelField(((AudioSet.AudioID)i).ToString());

            EditorGUILayout.PropertyField(clip, GUIContent.none);
            
            EditorGUILayout.Space(5);
        }

        if (EditorGUI.EndChangeCheck())
            serializedObject.ApplyModifiedProperties();
    }
}
