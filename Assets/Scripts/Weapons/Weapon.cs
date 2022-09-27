using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    [SerializeField] protected float baseDamage;
    [SerializeField] protected float baseKnockback;

    public virtual int GetBaseAmount()
    {
        return -1;
    }

    protected virtual void Start()
    {
        gameObject.SetActive(false);
    }

    public virtual bool CanEquip()
    {
        return true;
    }

    public virtual int GetAmount()
    {
        return -1;
    }

    public virtual void SetAmount(byte value)
    {
        
    }

    public virtual void UseWeapon(WormController worm)
    {
    }

    public void CancelWeapon(WormController worm)
    {
        StopAllCoroutines();
        
        worm.SetAnimTrigger("Cancel");
        worm.effects.DisableAimLine();
        worm.StopAttackWait();
    }
}
