using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class WormsEffects : MonoBehaviour
{
    [SerializeField] private AimLine aimLine;

    [SerializeField] private byte lineResolution = 16;

    [SerializeField] private ParticleSystem smokeParticles;

    [SerializeField]
        private Image healthUI;

    [SerializeField] 
        private GameObject highlight;

    private Material _healthMat;
    
    private static readonly int HealthValuesA = Shader.PropertyToID("_HealthValuesA");
    private static readonly int HealthValuesB = Shader.PropertyToID("_HealthValuesB");

    private void Start()
    {
        DisableAimLine();
        SetHighlight(false);
    }
    
    [SerializeField] 
    private GameObject[] playerHats;
    
    [SerializeField] 
    private Renderer renderer;
    private Material _skinMat, _eyeMat;
    
    private static readonly int EyeLidTilt = Shader.PropertyToID("_EyeLidTilt");
    private static readonly int Blink = Shader.PropertyToID("_Blink");
    
    private float _eyeTilt;
    private float _eyeLidDefault;
    
    public void SetPresetLook(int num)
    {
        _skinMat = renderer.materials[0];
        _eyeMat = renderer.materials[1];
        
        Color32 color = GameRules.PlayerPresetColors[num];

        _skinMat.color = color;
        _eyeMat.color = color;

        for (int i = 0; i < playerHats.Length; i++)
        {
            playerHats[i].SetActive(i == num);
        }

        _eyeTilt = GameRules.EyeLidTilt[num];
        _eyeLidDefault = GameRules.EyeLidDefault[num];
        
        _eyeMat.SetFloat(Blink, _eyeLidDefault);
        _eyeMat.SetFloat(EyeLidTilt, _eyeTilt);
        _blinkTimer = Random.Range(0f, 1f);
    }

    private float _blinkTimer = 0;
    public void BlinkLoop()
    {
        _blinkTimer += Time.deltaTime;
        if (!(_blinkTimer < 2))
        {
            if (_blinkTimer < 2.25f)
            {
                _eyeMat.SetFloat(Blink, _eyeLidDefault - (_blinkTimer - 2) * 4);
            }
            else
            {
                _eyeMat.SetFloat(Blink, Mathf.Min((_blinkTimer - 2.25f)*4f, _eyeLidDefault));

                if (!(_blinkTimer > 2.5f)) return;
                _eyeMat.SetFloat(Blink, _eyeLidDefault);
                _blinkTimer = 0;
            }
        }
    }

    public void SetLine(Vector3 startPos, Vector3 vel, float timeRange)
    {
        aimLine.gameObject.SetActive(true);

        int maxDots = Mathf.FloorToInt(vel.magnitude * timeRange) + 2;
        
        Matrix4x4[] trans = new Matrix4x4[maxDots];

        float scale = aimLine.GetScale();
        
        for (int i = 0; i < maxDots; i++)
        {
            float timePos = i * timeRange/maxDots;
            
            Vector3 pos = startPos + (vel * timePos);
            pos.y += Physics.gravity.y * timePos *timePos*.5f;

            trans[i] = Matrix4x4.TRS(pos, Quaternion.identity, Vector3.one*scale);
            //Debug.Log(pos[i].y);
        }
        
        aimLine.DrawLine(trans);
    }

    public void DisableAimLine() => aimLine.gameObject.SetActive(false);

    public void SetSmokeParticles(bool value)
    {
        if (value)
            smokeParticles.Play();
        else
            smokeParticles.Stop();
    }

    public void InstanceHealthUI(byte playerIndex)
    {
        _healthMat = new Material(healthUI.material);
        
        _healthMat.SetFloat("_MaximumHealth", 1);
        _healthMat.SetColor("_Color", GameRules.PlayerUIColors[playerIndex]);
        _healthMat.SetVector(HealthValuesA, new Vector4(1,0,0,0));
        _healthMat.SetVector(HealthValuesB, Vector4.zero);

        healthUI.material = _healthMat;
    }
    
    public void SetHealthUI(float value)
    {
        _healthMat.SetVector(HealthValuesA, new Vector4(value,0,0,0));
    }

    public void SetHighlight(bool value) => highlight.SetActive(value);
}
