using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WormsEffects : MonoBehaviour
{
    [SerializeField] private LineRenderer aimLine;

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
        aimLine.positionCount = lineResolution;
        DisableAimLine();
        SetHighlight(false);
    }

    public void SetLine(Vector3 startPos, Vector3 vel, float timeRange)
    {
        aimLine.gameObject.SetActive(true);
        
        Vector3[] pos = new Vector3[lineResolution];
        
        for (int i = 0; i < lineResolution; i++)
        {
            float timePos = i * timeRange/lineResolution;

            pos[i] = startPos + (vel * timePos);
            pos[i].y += Physics.gravity.y * timePos *timePos*.5f;
            //Debug.Log(pos[i].y);
        }
        
        aimLine.SetPositions(pos);
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
        _healthMat.SetColor("_Color", GameRules.playerUIColors[playerIndex]);
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
