using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clouds : MonoBehaviour
{
    private void Awake() 
    {
        float randomValue = Random.Range(0, 10f);

        GetComponent<Renderer>().sharedMaterial.SetFloat("_Random", randomValue);
    }
}
