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

    public override float UseWeapon(WormController worm)
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
                
                if(dist > 1.5f)
                    return 1.5f;
                
                Vector3 force = (entity.GetPos() - wormPos).normalized * baseKnockback + Vector3.up*launchKnockback;

                StartCoroutine(Damage(entity, baseDamage, force, .5f));
            }
        }

        return 1.5f;
    }

    IEnumerator Damage(IEntity entity, float damage, Vector3 force, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        entity.Damage(baseDamage, force);
    }
}
