using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIWinDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI winText;

    private void Start()
    {
        gameObject.SetActive(false);
    }

    public void SetWinner(byte playerIndex)
    {
        gameObject.SetActive(true);
        
        winText.color = GameRules.PlayerUIColors[playerIndex];

        winText.text = "Player " + (playerIndex + 1) + "Wins!!!";
    }
}
