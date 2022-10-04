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
        private UIHealthBar[] healthBar;

    [SerializeField] private UIWeaponIcon[] weaponIcon;

    [SerializeField] private UITurnTimer turnTimer;

    [SerializeField]
        private UIWinDisplay _winDisplay;

    private void Awake()
    {
        Instance = this;
    }

    public void SetUpPlayersHealth(byte playerAmount, byte wormAmount)
    {
        for (int i = 0; i < healthBar.Length; i++)
        {
            if (i < playerAmount)
            {
                healthBar[i].SetupHealth(wormAmount, i);
                healthBar[i].Display(true);
            }
            else
                healthBar[i].Display(false);
        }
    }

    public void UpdatePlayerHealth(byte player, byte worm, float health)
    {
        healthBar[player].UpdateHealth(worm, health);
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

    public void SetWinText(byte playerIndex) => _winDisplay.SetWinner(playerIndex);
}
