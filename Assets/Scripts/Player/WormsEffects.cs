using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WormsEffects : MonoBehaviour
{
    [SerializeField] private LineRenderer aimLine;

    [SerializeField] private byte lineResolution = 16;

    [SerializeField] private ParticleSystem smokeParticles;

    private void Start()
    {
        aimLine.positionCount = lineResolution;
        DisableAimLine();
        
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
}
