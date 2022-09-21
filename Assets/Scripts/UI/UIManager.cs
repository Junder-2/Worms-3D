using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.UI;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [SerializeField]
        private Image[] playerHealth;

    private Material[] healthUIMat;
    private static readonly int HealthValuesA = Shader.PropertyToID("_HealthValuesA");
    private static readonly int HealthValuesB = Shader.PropertyToID("_HealthValuesB");

    private void Awake()
    {
        Instance = this;
    }

    public void SetPlayersHealth(byte playerAmount, byte wormAmount, float maxHealth)
    {
        healthUIMat = new Material[playerAmount];

        Material UIMatCopy = playerHealth[0].material;

        Vector4 aValues = Vector4.zero;
        Vector4 bValues = Vector4.zero;

        for (int i = 0; i < wormAmount; i++)
        {
            int index = i % 4;

            if (i < 4)
                aValues[index] = 1;
            else
                bValues[index] = 1;

        }
        
        UIMatCopy.SetVector(HealthValuesA, aValues);
        UIMatCopy.SetVector(HealthValuesB, bValues);
        UIMatCopy.SetFloat("_MaximumHealth", wormAmount);

        for (int i = 0; i < playerHealth.Length; i++)
        {
            playerHealth[i].transform.parent.gameObject.SetActive(false);
        }

        for (int i = 0; i < playerAmount; i++)
        {
            healthUIMat[i] = new Material(UIMatCopy);
            healthUIMat[i].SetColor("_Color", GameRules.playerColors[i]);

            playerHealth[i].material = healthUIMat[i];
            playerHealth[i].transform.parent.gameObject.SetActive(true);
        }
    }

    public void UpdatePlayerHealth(byte player, byte worm, float health)
    {
        int index = worm % 4;

        if (worm < 4)
        {
            Vector4 values = healthUIMat[player].GetVector(HealthValuesA);
            values[index] = health;
            healthUIMat[player].SetVector(HealthValuesA, values);
        }
        else
        {
            Vector4 values = healthUIMat[player].GetVector(HealthValuesB);
            values[index] = health;
            healthUIMat[player].SetVector(HealthValuesB, values);
        }
    }
}
