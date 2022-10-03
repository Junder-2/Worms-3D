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

    private void Start()
    {
        byte playerAmount = GameRules.PlayerAmount;
        byte worms = GameRules.WormsPerPlayer;
        float health = GameRules.WormsMaxHealth;
        float roundTimer = GameRules.RoundTimer;
        
        playerAmountDisplay.text = playerAmount.ToString();
        wormPerPlayerDisplay.text = worms.ToString();
        playerHealthDisplay.text = health.ToString();
        turnTimeDisplay.text = roundTimer.ToString();
    }

    public void ChangePlayerAmount(int dir)
    {
        byte playerAmount = GameRules.PlayerAmount;

        switch (dir)
        {
            case 0 when playerAmount > GameRules.MinPlayers:
                playerAmount--;
                AudioManager.Instance.PlayGlobalSound((int)AudioSet.AudioID.UIClickB);
                break;
            case 1 when playerAmount < GameRules.MaxPlayers:
                playerAmount++;
                AudioManager.Instance.PlayGlobalSound((int)AudioSet.AudioID.UIClickA);
                break;
        }

        playerAmountDisplay.text = playerAmount.ToString();
        GameRules.PlayerAmount = playerAmount;
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
