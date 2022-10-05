using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterSplash : MonoBehaviour
{
    [SerializeField] private Renderer waterRipples;

    [SerializeField] private float lifeTime = 1;

    private float _timer = 0;
    private float _startStep;

    private Material _rippleMat;
    private static readonly int Step = Shader.PropertyToID("_Step");

    private void Awake()
    {
        _rippleMat = waterRipples.material;

        _startStep = _rippleMat.GetFloat(Step);
        _rippleMat.SetFloat(Step, _startStep);
    }

    private void Update()
    {
        _timer += Time.deltaTime;
        
        _rippleMat.SetFloat(Step, Mathf.Lerp(_startStep, 1, _timer/lifeTime));
        
        if(_timer/lifeTime > 1)
            Destroy(gameObject);
    }
}