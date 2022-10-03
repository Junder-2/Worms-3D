using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterSplash : MonoBehaviour
{
    [SerializeField] private Renderer waterRipples;

    [SerializeField] private float lifeTime = 1;

    private float timer = 0;
    private float startStep;

    private Material rippleMat;
    private static readonly int Step = Shader.PropertyToID("_Step");

    private void Awake()
    {
        rippleMat = waterRipples.material;

        startStep = rippleMat.GetFloat(Step);
        rippleMat.SetFloat(Step, startStep);
    }

    private void Update()
    {
        timer += Time.deltaTime;
        
        rippleMat.SetFloat(Step, Mathf.Lerp(startStep, 1, timer/lifeTime));
        
        if(timer/lifeTime > 1)
            Destroy(gameObject);
    }
}