using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEffects : MonoBehaviour
{
    [SerializeField] private GameObject waterSplashPrefab;

    public static LevelEffects Instance;

    private void Awake()
    {
        Instance = this;
    }
    
    LayerMask waterLayer => LayerMask.NameToLayer("Water");

    public void SpawnWaterSplash(Vector3 pos)
    {
        RaycastHit hit;

        if (Physics.Raycast(pos + 2f * Vector3.up, Vector3.down, out hit, 10f, waterLayer))
        {
            pos.y = hit.point.y;
        }

        Instantiate(waterSplashPrefab, pos, Quaternion.identity);
    }
}
