using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Transform _cam;

    private Quaternion _originalRotation;

    void Start()
    {
        _cam = Camera.main.transform;
        
        _originalRotation = transform.rotation;
    }

    void Update()
    {
        transform.rotation = _cam.rotation * _originalRotation;   
    }
}
