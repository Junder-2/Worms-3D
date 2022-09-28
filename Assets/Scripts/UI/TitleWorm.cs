using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class TitleWorm : MonoBehaviour
{
    [SerializeField]
        private byte animIndex;

    [SerializeField] private byte wormPreset;

    [SerializeField] private Renderer renderer;
    
    private Material _skinMat, _eyeMat;
    
    [SerializeField]
        private float eyeTilt;
    [SerializeField]
        private float eyeLidDefault;
        
    private static readonly int EyeLidTilt = Shader.PropertyToID("_EyeLidTilt");

    private float _timer;
    private static readonly int Blink = Shader.PropertyToID("_Blink");

    private void Start() 
    {
        GetComponent<Animator>().SetInteger("Index", animIndex);
        
        _skinMat = renderer.materials[0];
        _eyeMat = renderer.materials[1];
        
        Color32 color = GameRules.PlayerPresetColors[wormPreset];

        _skinMat.color = color;
        _eyeMat.color = color;
        _eyeMat.SetFloat(Blink, eyeLidDefault);
        _eyeMat.SetFloat(EyeLidTilt, eyeTilt);
        _timer = Random.Range(0f, 1f);
    }

    private void LateUpdate()
    {
        _timer += Time.deltaTime;
        if (!(_timer < 2))
        {
            if (_timer < 2.25f)
            {
                _eyeMat.SetFloat(Blink, eyeLidDefault - (_timer - 2) * 4);
            }
            else
            {
                _eyeMat.SetFloat(Blink, Mathf.Min((_timer - 2.25f)*4f, eyeLidDefault));

                if (!(_timer > 2.5f)) return;
                _eyeMat.SetFloat(Blink, eyeLidDefault);
                _timer = 0;
            }
        }
    }
}
