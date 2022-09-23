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
        byte playerAmount = GameRules.playerAmount;
        byte worms = GameRules.wormsPerPlayer;
        float health = GameRules.wormsMaxHealth;
        float roundTimer = GameRules.roundTimer;
        
        playerAmountDisplay.text = playerAmount.ToString();
        wormPerPlayerDisplay.text = worms.ToString();
        playerHealthDisplay.text = health.ToString();
        turnTimeDisplay.text = roundTimer.ToString();
    }

    public void ChangePlayerAmount(int dir)
    {
        byte playerAmount = GameRules.playerAmount;

        switch (dir)
        {
            case 0 when playerAmount > GameRules.MinPlayers:
                playerAmount--;
                break;
            case 1 when playerAmount < GameRules.MaxPlayers:
                playerAmount++;
                break;
        }

        playerAmountDisplay.text = playerAmount.ToString();
        GameRules.playerAmount = playerAmount;
    }

    public void ChangeWormsPerPlayer(int dir)
    {
        byte worms = GameRules.wormsPerPlayer;
        
        switch (dir)
        {
            case 0 when worms > GameRules.MinWorms:
                worms--;
                break;
            case 1 when worms < GameRules.MaxWorms:
                worms++;
                break;
        }

        wormPerPlayerDisplay.text = worms.ToString();
        GameRules.wormsPerPlayer = worms;
    }

    public void ChangePlayerHealth(int dir)
    {
        float health = GameRules.wormsMaxHealth;
        
        switch (dir)
        {
            case 0 when health > GameRules.MinHealth:
                health -= 5;
                break;
            case 1 when health < GameRules.MaxHealth:
                health += 5;
                break;
        }

        playerHealthDisplay.text = health.ToString();
        GameRules.wormsMaxHealth = (half)health;
    }
    
    public void ChangeTurnTime(int dir)
    {
        float roundTimer = GameRules.roundTimer;
        
        switch (dir)
        {
            case 0 when roundTimer > GameRules.MinRoundTime:
                roundTimer -= 5;
                break;
            case 1 when roundTimer < GameRules.MaxRoundTime:
                roundTimer += 5;
                break;
        }

        turnTimeDisplay.text = roundTimer.ToString();
        GameRules.roundTimer = (half)roundTimer;
    }

}
