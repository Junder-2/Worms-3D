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

    private Material[] _healthUIMat;
    private static readonly int HealthValuesA = Shader.PropertyToID("_HealthValuesA");
    private static readonly int HealthValuesB = Shader.PropertyToID("_HealthValuesB");

    [SerializeField] private UIWeaponIcon[] weaponIcon;

    [SerializeField] private UITurnTimer turnTimer;

    private void Awake()
    {
        Instance = this;
    }

    public void SetPlayersHealth(byte playerAmount, byte wormAmount, float maxHealth)
    {
        _healthUIMat = new Material[playerAmount];

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

        foreach (var t in playerHealth)
        {
            t.transform.parent.gameObject.SetActive(false);
        }

        for (int i = 0; i < playerAmount; i++)
        {
            UIMatCopy.SetColor("_Color", GameRules.playerUIColors[i]);
            _healthUIMat[i] = new Material(UIMatCopy);

            playerHealth[i].material = _healthUIMat[i];
            playerHealth[i].transform.parent.gameObject.SetActive(true);
        }
    }

    public void UpdatePlayerHealth(byte player, byte worm, float health)
    {
        int index = worm % 4;

        if (worm < 4)
        {
            Vector4 values = _healthUIMat[player].GetVector(HealthValuesA);
            values[index] = health;
            _healthUIMat[player].SetVector(HealthValuesA, values);
        }
        else
        {
            Vector4 values = _healthUIMat[player].GetVector(HealthValuesB);
            values[index] = health;
            _healthUIMat[player].SetVector(HealthValuesB, values);
        }
    }

    public void InstanceWeaponUI(int[] amount, int selected)
    {
        for (int i = 0; i < weaponIcon.Length; i++)
        {
            if(selected >= 0 && selected == i)weaponIcon[i].SetSelector(true);
            else weaponIcon[i].SetSelector(false);
            weaponIcon[i].SetAmount(amount[i]);
        }
    }

    public void UpdateWeaponUI(int selected, int amount)
    {
        foreach (var t in weaponIcon)
        {
            t.SetSelector(false);
        }

        if (selected < 0) return;
        weaponIcon[selected].SetSelector(true);
        weaponIcon[selected].SetAmount(amount);
    }

    public void StartTimerUI(float time, bool start)
    {
        turnTimer.SetTime((int)time);
        turnTimer.StartTime(start);
    }
}
