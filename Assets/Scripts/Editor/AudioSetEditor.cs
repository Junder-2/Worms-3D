using System;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(AudioSet))]
    public class AudioSetEditor : UnityEditor.Editor
    {
        private readonly int _audioEffectSize = Enum.GetValues(typeof(AudioSet.AudioID)).Length;
        private readonly int _songSize = Enum.GetValues(typeof(AudioSet.MusicID)).Length;

        private SerializedProperty _audioClips;
        private SerializedProperty _songClips;

        private void OnEnable()
        {
            AudioSet audioSet = (AudioSet)target;
            if (audioSet.audioClips.Length != _audioEffectSize)
                audioSet.audioClips = new AudioClip[_audioEffectSize];
        
            _audioClips = serializedObject.FindProperty("audioClips");

            if (audioSet.songClips.Length != _songSize)
                audioSet.songClips = new AudioClip[_songSize];
        
            _songClips = serializedObject.FindProperty("songClips");
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            for (int i = 0; i < _audioEffectSize; i++)
            {
                var clip = _audioClips.GetArrayElementAtIndex(i);

                EditorGUILayout.LabelField(((AudioSet.AudioID)i).ToString());

                EditorGUILayout.PropertyField(clip, GUIContent.none);
            
                EditorGUILayout.Space(5);
            }

            EditorGUILayout.Space(15);

            for (int i = 0; i < _songSize; i++)
            {
                var clip = _songClips.GetArrayElementAtIndex(i);

                EditorGUILayout.LabelField(((AudioSet.MusicID)i).ToString());

                EditorGUILayout.PropertyField(clip, GUIContent.none);
            
                EditorGUILayout.Space(5);
            }

            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        }
    }
}
