using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponDynamite : Weapon
{
    [SerializeField] private byte startAmount;
    
    private byte amount = 3;

    [SerializeField] private float fuseTime, explosionRange;

    [SerializeField]
        private GameObject plantedDynamitePrefab;

    protected override void Start()
    {
        amount = startAmount;
        base.Start();
    }

    public override int GetAmount()
    {
        return amount;
    }

    public override bool CanEquip()
    {
        return amount > 0;
    }
    
    public override void UseWeapon(WormController worm)
    {
        worm.SetAnimTrigger("Plant");

        amount--;

        StartCoroutine(DelayPlace(worm.GetPos() + worm.GetForwards()*.5f, worm));
    }

    IEnumerator DelayPlace(Vector3 pos, WormController worm)
    {
        yield return new WaitForSeconds(.25f);
        GameObject plantedDynamite = Instantiate(plantedDynamitePrefab, pos, Quaternion.identity);
        plantedDynamite.GetComponent<ExplosionObject>().Instantiate(baseDamage, baseKnockback, fuseTime, explosionRange);
        
        yield return new WaitForSeconds(.5f);
        worm.StopAttackWait();
        if(amount <= 0)
            worm.DeEquipWeapon();
    }
}
