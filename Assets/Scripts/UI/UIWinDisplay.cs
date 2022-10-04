using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIWinDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI winText;
    
    [SerializeField] 
        private GameObject[] displayHat = new GameObject[4];

    private void Start()
    {
        gameObject.SetActive(false);
    }

    public void SetWinner(byte playerIndex)
    {
        gameObject.SetActive(true);

        int presetIndex = GameRules.playerAssignedPreset[playerIndex];
        
        winText.color = GameRules.PlayerUIColors[presetIndex];

        winText.text = "Player " + (playerIndex + 1) + " Wins!!!";
        
        for (int i = 0; i < displayHat.Length; i++)
        {
            displayHat[i].SetActive(i == presetIndex);
        }
    }
}
