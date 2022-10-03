using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponDynamite : Weapon
{
    //[SerializeField] private byte startAmount;

    private byte _amount = 1;

    [SerializeField] private float fuseTime, explosionRange;

    [SerializeField]
        private GameObject plantedDynamitePrefab;

    public override int GetBaseAmount()
    {
        switch (GameRules.WormsPerPlayer)
        {
            case 1:
                return 1;
            case 2:
                return 2;
            case 3:
                return 3;
            case 4:
                return 3;
        }
        
        return -1;
    }

    public override int GetAmount()
    {
        return _amount;
    }
    
    public override void SetAmount(byte value)
    {
        _amount = value;
    }

    public override bool CanEquip()
    {
        return _amount > 0;
    }
    
    public override void UseWeapon(WormController worm)
    {
        worm.effects.SetAnimTrigger("Plant");
        worm.effects.PlaySound((int)AudioSet.AudioID.HghWuh2);

        _amount--;

        StartCoroutine(DelayPlace(worm.GetPos() + worm.GetForwards()*.5f, worm));
    }

    IEnumerator DelayPlace(Vector3 pos, WormController worm)
    {
        yield return new WaitForSeconds(.25f);
        GameObject plantedDynamite = Instantiate(plantedDynamitePrefab, pos, Quaternion.identity);
        plantedDynamite.GetComponent<ExplosionObject>().Instantiate(baseDamage, baseKnockback, fuseTime, explosionRange);
        
        yield return new WaitForSeconds(.5f);
        worm.StopAttackWait();
        if(_amount <= 0)
            worm.DeEquipWeapon();
    }
}
