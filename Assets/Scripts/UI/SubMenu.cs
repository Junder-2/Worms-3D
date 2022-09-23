using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubMenu : MonoBehaviour
{
    [SerializeField] private GameObject firstSelect;
    [SerializeField] private GameObject menuWindow;

    public GameObject GetFirstSelect() => firstSelect;

    public GameObject GetWindow() => menuWindow;
}
