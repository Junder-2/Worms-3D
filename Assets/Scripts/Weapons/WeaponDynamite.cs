using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponDynamite : Weapon
{
    [SerializeField] private byte startAmount;
    
    private byte amount;

    [SerializeField] private float fuseTime, explosionRange;

    [SerializeField]
        private GameObject plantedDynamitePrefab;

    private void Start()
    {
        amount = startAmount;
    }

    public override bool IsMelee()
    {
        return true;
    }

    public override bool CanEquip()
    {
        return amount > 0;
    }
    
    public override float UseWeapon(WormController worm)
    {
        worm.SetAnimTrigger("Plant");

        amount--;

        StartCoroutine(DelayPlace(worm.GetPos() + worm.GetForwards()*.5f, worm));

        return 1.5f;
    }

    IEnumerator DelayPlace(Vector3 pos, WormController worm)
    {
        yield return new WaitForSeconds(.25f);
        GameObject plantedDynamite = Instantiate(plantedDynamitePrefab, pos, Quaternion.identity);
        plantedDynamite.GetComponent<ExplosionObject>().Instantiate(baseDamage, baseKnockback, fuseTime, explosionRange);
        
        if(amount <= 0)
            worm.DeEquipWeapon();
    }
}
