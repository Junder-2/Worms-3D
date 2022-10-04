using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPlayerPreset : MonoBehaviour
{
    [SerializeField] private Image iconDisplay;

    public void SetIcon(Sprite icon) => iconDisplay.sprite = icon;

    public bool Active
    {
        get => _active;

        set
        {
            _active = value;
            
            gameObject.SetActive(_active);
        }
    }

    private bool _active;
}
