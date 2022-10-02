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

    public void SpawnWaterSplash(Vector3 pos)
    {
        pos.y = 0;

        Instantiate(waterSplashPrefab, pos, Quaternion.identity);
    }
}
