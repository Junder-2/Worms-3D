using System;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(AudioSet))]
    public class AudioSetEditor : UnityEditor.Editor
    {
        private readonly int audioEffectSize = Enum.GetValues(typeof(AudioSet.AudioID)).Length;
        private readonly int songSize = Enum.GetValues(typeof(AudioSet.MusicID)).Length;

        private SerializedProperty _audioClips;
        private SerializedProperty _songClips;

        private void OnEnable()
        {
            AudioSet audioSet = (AudioSet)target;
            if (audioSet.audioClips.Length != audioEffectSize)
                audioSet.audioClips = new AudioClip[audioEffectSize];
        
            _audioClips = serializedObject.FindProperty("audioClips");

            if (audioSet.songClips.Length != songSize)
                audioSet.songClips = new AudioClip[songSize];
        
            _songClips = serializedObject.FindProperty("songClips");
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            for (int i = 0; i < audioEffectSize; i++)
            {
                var clip = _audioClips.GetArrayElementAtIndex(i);

                EditorGUILayout.LabelField(((AudioSet.AudioID)i).ToString());

                EditorGUILayout.PropertyField(clip, GUIContent.none);
            
                EditorGUILayout.Space(5);
            }

            EditorGUILayout.Space(15);

            for (int i = 0; i < songSize; i++)
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
