using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponBomb : Weapon
{
    [SerializeField] private byte startAmount;
    
    private byte amount = 3;

    [SerializeField] private float fuseTime, explosionRange;

    [SerializeField]
    private GameObject thrownBombPrefab;

    protected override void Start()
    {
        amount = startAmount;
        base.Start();
    }

    public override bool CanEquip()
    {
        return amount > 0;
    }

    public override int GetAmount()
    {
        return amount;
    }

    public override void UseWeapon(WormController worm)
    {
        StartCoroutine(DelayAction(worm));
    }

    IEnumerator DelayAction(WormController worm)
    {
        yield return new WaitForSeconds(.25f);
        
        bool aiming = true;

        float throwUp = 0, throwForward = 0;

        worm.State.freezeCamPitch = true;

        do
        {
            PlayerInput.InputAction input = worm.GetInput();
            if (input.aInput == 1)
            {
                
                worm.StopAttackWait();
                StopAllCoroutines();
            }

            if (input.bInput == 1)
                aiming = false;

            if (input.moveNonZero)
            {
                worm.TurnPlayer(input.rawMoveInput.x*45f*Time.deltaTime);

                throwForward = Mathf.Max(0, throwForward + input.rawMoveInput.y * Time.deltaTime*5f);
                throwUp = Mathf.Max(-.5f, throwUp + input.cameraInput.y * Time.deltaTime*5f);
            }
            
            Debug.DrawRay(transform.position, throwUp/10*worm.GetUp()+throwForward/10*worm.GetForwards());

            yield return null;

        } while (aiming);
        
        worm.SetAnimTrigger("Throw");

        yield return new WaitForSeconds(.31f);

        amount--;

        Vector3 pos = worm.GetPos() + worm.GetForwards() * .5f;

        GameObject thrownBomb = Instantiate(thrownBombPrefab, pos, Quaternion.identity);
        thrownBomb.GetComponent<ExplosionObject>().Instantiate(baseDamage, baseKnockback, fuseTime, explosionRange);
        thrownBomb.GetComponent<Rigidbody>().velocity = throwUp * worm.GetUp() + throwForward * worm.GetForwards();

        yield return new WaitForSeconds(.3f);
        worm.StopAttackWait();
        
        if(amount <= 0)
            worm.DeEquipWeapon();
    }
}
