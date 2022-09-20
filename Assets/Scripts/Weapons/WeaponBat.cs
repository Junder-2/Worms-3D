using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponBat : Weapon
{
    [SerializeField] private float launchKnockback;
    
    public override bool CanEquip()
    {
        return base.CanEquip();
    }

    public override bool IsMelee()
    {
        return true;
    }

    public override void UseWeapon(WormController worm)
    {
        worm.SetAnimTrigger("Swing");
        
        RaycastHit hit;

        Vector3 wormPos = worm.GetPos();
        Vector3 wormForwards = worm.GetForwards();

        if (Physics.Raycast(wormPos, wormForwards, out hit, 5f))
        {
            var entity = hit.transform.GetComponent<IEntity>();
            
            if (entity != null)
            {
                float dist = (entity.GetPos() - wormPos).magnitude;

                if (dist > 1.5f)
                {
                    worm.StopAttackWait();
                    return;
                }
                    

                dist = Mathf.Clamp01(2 - dist);
                
                Vector3 force = (entity.GetPos() - wormPos).normalized * baseKnockback + Vector3.up*launchKnockback;

                StartCoroutine(Damage(entity, baseDamage*dist, force*dist, worm));
            }
        }
        else
        {
            StartCoroutine(DelayReturn(worm));
        }
    }

    IEnumerator Damage(IEntity entity, float damage, Vector3 force, WormController worm)
    {
        yield return new WaitForSeconds(.5f);
        
        entity.Damage(baseDamage, force);

        yield return new WaitForSeconds(.2f);
        worm.StopAttackWait();
    }

    IEnumerator DelayReturn(WormController worm)
    {
        yield return new WaitForSeconds(.75f);
        worm.StopAttackWait();
    }
}
