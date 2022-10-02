using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponBazooka : Weapon
{
    private byte _amount = 2;

    [SerializeField] private float fuseTime, explosionRange, rocketSpeed;

    [SerializeField]
        private GameObject rocketPrefab;

    [SerializeField] 
        private Transform rocketSpawn;

    public override int GetBaseAmount()
    {
        switch (GameRules.WormsPerPlayer)
        {
            case 1:
                return 1;
            case 2:
                return 1;
            case 3:
                return 2;
            case 4:
                return 3;
        }
        
        return -1;
    }

    public override bool CanEquip() => _amount > 0;

    public override int GetAmount() => _amount;

    public override void SetAmount(byte value) => _amount = value;

    public override void UseWeapon(WormController worm)
    {
        StartCoroutine(DelayAction(worm));
    }

    IEnumerator DelayAction(WormController worm)
    {
        //yield return new WaitForSeconds(.25f);
        float timer = 0;
        float zoom = 1;

        do
        {
            timer += Time.deltaTime/.5f;
            zoom = Mathf.Lerp(1, .5f, timer);
            worm.SetCamZoom(zoom);
                    
            yield return null;
        } while (timer < 1);

        bool aiming = true;

        float aimUpwards = 0, aimForwards = 5;

        worm.State.freezeCamPitch = true;

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

                aimForwards = Mathf.Clamp(aimForwards + input.rawMoveInput.y * Time.deltaTime*5f, 0, 15f);
                aimUpwards = Mathf.Clamp(aimUpwards + input.cameraInput.y * Time.deltaTime*5f, -10f, 15f);
            }

            worm.effects.SetLine(rocketSpawn.position, aimUpwards * Vector3.up + (aimForwards*rocketSpeed) * worm.GetForwards(), 1f);

            //Debug.DrawRay(transform.position, throwUp/10*worm.GetUp()+throwForward/10*worm.GetForwards());

            yield return null;

        } while (aiming);

        float lastZoom = zoom;
        
        //worm.SetAnimTrigger("Throw");

        timer = 0;
        do
        {
            timer += Time.deltaTime/.5f;
            zoom = Mathf.Lerp(lastZoom, 1, timer);
            worm.SetCamZoom(zoom);
                    
            yield return null;
        } while (timer < 1);

        _amount--;
        
        worm.effects.DisableAimLine();

        Vector3 vel = aimUpwards * Vector3.up + (aimForwards * rocketSpeed) * worm.GetForwards();

        float forceMulti = Mathf.Min(vel.magnitude / rocketSpeed, 2) + .5f;

        GameObject rocket = Instantiate(rocketPrefab, rocketSpawn.position, Quaternion.LookRotation(vel.normalized));
        rocket.GetComponent<ExplosionObject>().Instantiate(baseDamage*forceMulti, baseKnockback*forceMulti, fuseTime, explosionRange);
        rocket.GetComponent<Rigidbody>().velocity = vel;

        yield return new WaitForSeconds(.8f);
        worm.StopAttackWait();
        
        if(_amount <= 0)
            worm.DeEquipWeapon();
    }
}
