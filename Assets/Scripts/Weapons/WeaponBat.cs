using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponBat : Weapon
{
    [SerializeField] private float launchKnockback;

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

        float timer = 0;

        bool chargeBat = true;
        float chargeStrength = 1f;
        float zoom = 1f;

        float lastZoom;

        yield return new WaitForSeconds(.2f);

        do
        {
            PlayerInput.InputAction input = worm.GetInput();

            if (input.bInput == 0)
                chargeBat = false;
            else
            {
                chargeStrength += Time.deltaTime;
                zoom -= Time.deltaTime / 3;
                
                worm.SetCamZoom(zoom);
                if (chargeStrength > 2)
                {
                    chargeBat = false;
                    chargeStrength = 2;
                }
            }
            yield return null;
        } while (chargeBat);

        lastZoom = zoom;

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

                wormPos.y -= .25f;
                
                Vector3 force = (entity.GetPos() - wormPos).normalized * baseKnockback + Vector3.up*launchKnockback;
                
                worm.SetAnimTrigger("SwingB");

                yield return new WaitForSeconds(.1f);

                dist *= chargeStrength;
        
                entity.Damage(baseDamage*dist, force*dist);
                
                //yield return new WaitForSeconds(.2f);
                do
                {
                    timer += Time.deltaTime/.2f;
                    zoom = Mathf.Lerp(lastZoom, 1, timer);
                    worm.SetCamZoom(zoom);
                    
                    yield return null;
                } while (timer < 1);
                
                worm.StopAttackWait();
            }
        }
        else
        {
            worm.SetAnimTrigger("SwingB");
            //yield return new WaitForSeconds(.75f);
            
            do
            {
                timer += Time.deltaTime/.75f;
                zoom = Mathf.Lerp(lastZoom, 1, timer);
                worm.SetCamZoom(zoom);
                    
                yield return null;
            } while (timer < 1);
            
            worm.StopAttackWait();
        }
    }
}
