using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class PlaySettingsController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerAmountDisplay;
    [SerializeField] private TextMeshProUGUI wormPerPlayerDisplay;
    [SerializeField] private TextMeshProUGUI playerHealthDisplay;
    [SerializeField] private TextMeshProUGUI turnTimeDisplay;

    [SerializeField] private UIPlayerPreset[] playerPreset;
    private int[] _currentSelectedPreset;
    [SerializeField] private Sprite[] presetIcon;

    public void Setup()
    {
        byte playerAmount = GameRules.PlayerAmount;
        byte worms = GameRules.WormsPerPlayer;
        float health = GameRules.WormsMaxHealth;
        float roundTimer = GameRules.RoundTimer;
        
        playerAmountDisplay.text = playerAmount.ToString();
        wormPerPlayerDisplay.text = worms.ToString();
        playerHealthDisplay.text = health.ToString();
        turnTimeDisplay.text = roundTimer.ToString();

        _currentSelectedPreset = GameRules.playerAssignedPreset;

        for (int i = 0; i < playerPreset.Length; i++)
        {
            if (i < playerAmount)
            {
                playerPreset[i].SetIcon(presetIcon[_currentSelectedPreset[i]]);
                playerPreset[i].Active = true;
            }
            else
            {
                _currentSelectedPreset[i] = -1;
                playerPreset[i].Active = false;
            }
        }
    }

    public void ChangePlayerAmount(int dir)
    {
        byte playerAmount = GameRules.PlayerAmount;

        switch (dir)
        {
            case 0 when playerAmount > GameRules.MinPlayers:
                playerAmount--;
                AudioManager.Instance.PlayGlobalSound((int)AudioSet.AudioID.UIClickB);

                for (int i = GameRules.MaxPlayers-1; i > playerAmount-1; i--)
                {
                    _currentSelectedPreset[i] = -1;
                    playerPreset[i].Active = false;
                }
                
                break;
            case 1 when playerAmount < GameRules.MaxPlayers:
                playerAmount++;
                AudioManager.Instance.PlayGlobalSound((int)AudioSet.AudioID.UIClickA);

                playerPreset[playerAmount-1].Active = true;
                IncrementPlayerPreset(playerAmount-1);

                break;
        }

        playerAmountDisplay.text = playerAmount.ToString();
        GameRules.PlayerAmount = playerAmount;
    }
    
    public void DecrementPlayerPreset(int index) 
    {
        AudioManager.Instance.PlayGlobalSound((int)AudioSet.AudioID.UIClickB);
        ChangePlayerPreset(-1, index);
    } 
    public void IncrementPlayerPreset(int index)
    { 
        AudioManager.Instance.PlayGlobalSound((int)AudioSet.AudioID.UIClickA);
        ChangePlayerPreset(1, index);
    }
    
    private void ChangePlayerPreset(int dir, int index)
    {
        int potentialPreset = _currentSelectedPreset[index] + dir;

        if (potentialPreset >= 0)
            potentialPreset %= _currentSelectedPreset.Length;
        else 
            potentialPreset += _currentSelectedPreset.Length;

        bool failed = false;
        
        do
        {
            failed = false;
            
            for (int i = 0; i < _currentSelectedPreset.Length; i++)
            {
                if (i == index) continue;
                if (potentialPreset == _currentSelectedPreset[i])
                {
                    potentialPreset += dir;
                    if (potentialPreset >= 0)
                        potentialPreset %= _currentSelectedPreset.Length;
                    else
                        potentialPreset += _currentSelectedPreset.Length;

                    failed = true;
                    
                    break;
                }
            }
        } while (failed);

        _currentSelectedPreset[index] = potentialPreset;
        playerPreset[index].SetIcon(presetIcon[potentialPreset]);

        GameRules.playerAssignedPreset = _currentSelectedPreset;
    }

    public void ChangeWormsPerPlayer(int dir)
    {
        byte worms = GameRules.WormsPerPlayer;
        
        switch (dir)
        {
            case 0 when worms > GameRules.MinWorms:
                worms--;
                AudioManager.Instance.PlayGlobalSound((int)AudioSet.AudioID.UIClickB);
                break;
            case 1 when worms < GameRules.MaxWorms:
                worms++;
                AudioManager.Instance.PlayGlobalSound((int)AudioSet.AudioID.UIClickA);
                break;
        }

        wormPerPlayerDisplay.text = worms.ToString();
        GameRules.WormsPerPlayer = worms;
    }

    public void ChangePlayerHealth(int dir)
    {
        float health = GameRules.WormsMaxHealth;
        
        switch (dir)
        {
            case 0 when health > GameRules.MinHealth:
                health -= 5;
                AudioManager.Instance.PlayGlobalSound((int)AudioSet.AudioID.UIClickB);
                break;
            case 1 when health < GameRules.MaxHealth:
                health += 5;
                AudioManager.Instance.PlayGlobalSound((int)AudioSet.AudioID.UIClickA);
                break;
        }

        playerHealthDisplay.text = health.ToString();
        GameRules.WormsMaxHealth = (half)health;
    }
    
    public void ChangeTurnTime(int dir)
    {
        float roundTimer = GameRules.RoundTimer;
        
        switch (dir)
        {
            case 0 when roundTimer > GameRules.MinRoundTime:
                roundTimer -= 5;
                AudioManager.Instance.PlayGlobalSound((int)AudioSet.AudioID.UIClickB);
                break;
            case 1 when roundTimer < GameRules.MaxRoundTime:
                roundTimer += 5;
                AudioManager.Instance.PlayGlobalSound((int)AudioSet.AudioID.UIClickA);
                break;
        }

        turnTimeDisplay.text = roundTimer.ToString();
        GameRules.RoundTimer = (half)roundTimer;
    }
}
