using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleWorm : MonoBehaviour
{
    [SerializeField]
        private byte animIndex;

    private void Start() 
    {
        GetComponent<Animator>().SetInteger("Index", animIndex);
    }
}
