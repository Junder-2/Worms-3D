using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    [SerializeField] protected float baseDamage;
    [SerializeField] protected float baseKnockback;

    protected virtual void Start()
    {
        gameObject.SetActive(false);
    }

    public virtual float GetDamage()
    {
        return baseDamage;
    }

    public virtual float GetKnockback()
    {
        return baseKnockback;
    }

    public virtual bool IsMelee()
    {
        return false;
    }

    public virtual bool CanEquip()
    {
        return true;
    }

    public virtual void UseWeapon(WormController worm)
    {
    }

    public void CancelWeapon(WormController worm)
    {
        StopAllCoroutines();
        
        worm.StopAttackWait();
    }
}
