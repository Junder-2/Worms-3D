using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    [SerializeField] protected float baseDamage;
    [SerializeField] protected float baseKnockback;
    
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

    public virtual float UseWeapon(WormController worm)
    {
        return 0;
    }
}
