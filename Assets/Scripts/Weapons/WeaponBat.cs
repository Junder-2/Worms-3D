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

    public override void UseWeapon(WormController worm)
    {
        StartCoroutine(BatAction(worm));
    }

    IEnumerator BatAction(WormController worm)
    {
        worm.SetAnimTrigger("SwingA");
        
        RaycastHit hit;

        Vector3 wormPos = worm.GetPos();
        Vector3 wormForwards = worm.GetForwards();

        bool chargeBat = true;
        float chargeStrength = 1f;

        yield return new WaitForSeconds(.2f);

        do
        {
            PlayerInput.InputAction input = worm.GetInput();

            if (input.bInput == 0)
                chargeBat = false;
            else
            {
                chargeStrength += Time.deltaTime;
                if (chargeStrength > 2)
                {
                    chargeBat = false;
                    chargeStrength = 2;
                }
            }
            yield return null;
        } while (chargeBat);

        if (Physics.Raycast(wormPos, wormForwards, out hit, 5f))
        {
            var entity = hit.transform.GetComponent<IEntity>();
            
            if (entity != null)
            {
                float dist = (entity.GetPos() - wormPos).magnitude;

                if (dist > 1.5f)
                {
                    worm.StopAttackWait();
                    StopAllCoroutines();
                }

                dist = Mathf.Clamp01(2 - dist);
                
                Vector3 force = (entity.GetPos() - wormPos).normalized * baseKnockback + Vector3.up*launchKnockback;
                
                worm.SetAnimTrigger("SwingB");

                yield return new WaitForSeconds(.1f);

                dist *= chargeStrength;
        
                entity.Damage(baseDamage*dist, force*dist);

                yield return new WaitForSeconds(.2f);
                worm.StopAttackWait();
            }
        }
        else
        {
            worm.SetAnimTrigger("SwingB");
            yield return new WaitForSeconds(.75f);
            worm.StopAttackWait();
        }
    }
}
