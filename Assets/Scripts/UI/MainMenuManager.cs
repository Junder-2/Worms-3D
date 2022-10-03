using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private SubMenu[] menus;

    private EventSystem _eventSystem;

    private void Awake()
    {
        _eventSystem = GetComponent<EventSystem>();
        
        SetMenu(0);
    }

    private void Start()
    {
        AudioManager.Instance.PlayMusic((int)AudioSet.MusicID.Menu);
    }

    private int menuIndex = 0;
    void SetMenu(int index)
    {
        for (int i = 0; i < menus.Length; i++)
        {
            if (index == i)
            {
                menus[i].GetWindow().SetActive(true);
                _eventSystem.SetSelectedGameObject(menus[i].GetFirstSelect());
                _eventSystem.UpdateModules();
            }
            else
            {
                menus[i].GetWindow().SetActive(false);
            }
        }

        menuIndex = index;
    }

    public void SwitchMenu(int index)
    {
        if(menuIndex - index > 0)
            AudioManager.Instance.PlayGlobalSound((int)AudioSet.AudioID.UIClickB);
        else 
            AudioManager.Instance.PlayGlobalSound((int)AudioSet.AudioID.UIClickA);
        
        SetMenu(index);
    }

    public void OnQuitButton()
    {
        AudioManager.Instance.PlayGlobalSound((int)AudioSet.AudioID.UIClickB);
        Application.Quit();
    }

    public void SwitchScenes(int index)
    {
        SceneManager.LoadScene(index);
    }
}
