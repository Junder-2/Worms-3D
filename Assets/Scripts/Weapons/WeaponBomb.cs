using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponBomb : Weapon
{
    private byte _amount = 3;

    [SerializeField] private float fuseTime, explosionRange;

    [SerializeField]
    private GameObject thrownBombPrefab;

    public override int GetBaseAmount()
    {
        switch (GameRules.WormsPerPlayer)
        {
            case 1:
                return 2;
            case 2:
                return 3;
            case 3:
                return 4;
            case 4:
                return 4;
        }
        
        return -1;
    }

    public override bool CanEquip()
    {
        return _amount > 0;
    }

    public override int GetAmount()
    {
        return _amount;
    }

    public override void SetAmount(byte value)
    {
        _amount = value;
    }

    public override void UseWeapon(WormController worm)
    {
        StartCoroutine(DelayAction(worm));
    }

    IEnumerator DelayAction(WormController worm)
    {
        yield return new WaitForSeconds(.25f);

        Vector3 pos;
        
        bool aiming = true;

        float throwUp = 1, throwForward = 1;

        worm.State.freezeCamPitch = true;

        float zoom = 1;

        do
        {
            PlayerInput.InputAction input = worm.GetInput();
            if (input.aInput == 1)
            {
                worm.effects.DisableAimLine();
                worm.StopAttackWait();
                StopAllCoroutines();
            }

            if (input.bInput == 1)
                aiming = false;

            if (input.moveNonZero || input.camNonZero)
            {
                worm.TurnPlayer(input.rawMoveInput.x*45f*Time.deltaTime);

                throwForward = Mathf.Clamp(throwForward + input.rawMoveInput.y * Time.deltaTime*5f, 0, 15f);
                throwUp = Mathf.Clamp(throwUp + input.cameraInput.y * Time.deltaTime*5f, -1f, 10f);
            }
            
            pos = worm.GetPos() + worm.GetForwards() * .5f;
            
            worm.effects.SetLine(pos, throwUp * Vector3.up + throwForward * worm.GetForwards(), 1f);

            zoom = Mathf.MoveTowards(zoom, .9f, Time.deltaTime);
            worm.SetCamZoom(zoom);
            
            //Debug.DrawRay(transform.position, throwUp/10*worm.GetUp()+throwForward/10*worm.GetForwards());

            yield return null;

        } while (aiming);

        float lastZoom = zoom;
        
        worm.SetAnimTrigger("Throw");

        //yield return new WaitForSeconds(.31f);

        float timer = 0;
        
        do
        {
            timer += Time.deltaTime/.31f;
            zoom = Mathf.Lerp(lastZoom, 1, timer);
            worm.SetCamZoom(zoom);
                    
            yield return null;
        } while (timer < 1);

        _amount--;
        
        worm.effects.DisableAimLine();

        pos = worm.GetPos() + worm.GetForwards() * .5f;

        GameObject thrownBomb = Instantiate(thrownBombPrefab, pos, Quaternion.identity);
        thrownBomb.GetComponent<ExplosionObject>().Instantiate(baseDamage, baseKnockback, fuseTime, explosionRange);
        thrownBomb.GetComponent<Rigidbody>().velocity = throwUp * Vector3.up + throwForward * worm.GetForwards();

        yield return new WaitForSeconds(.3f);
        worm.StopAttackWait();
        
        if(_amount <= 0)
            worm.DeEquipWeapon();
    }
}
