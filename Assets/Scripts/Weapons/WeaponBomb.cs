using System.Collections;
using Player;
using UnityEngine;

namespace Weapons
{
    public class WeaponBomb : Weapon
    {
        private byte _amount = 1;

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

        public override bool CanEquip() => _amount > 0;

        public override int GetAmount() => _amount;

        public override void SetAmount(byte value) => _amount = value;

        public override void UseWeapon(WormController worm)
        {
            StartCoroutine(ThrowBomb(worm));
        }

        IEnumerator ThrowBomb(WormController worm)
        {
            yield return new WaitForSeconds(.25f);

            Vector3 pos;
        
            bool aiming = true;

            float throwUp = 1, throwForward = 1;

            worm.State.FreezeCamPitch = true;

            float zoom = 1;

            do
            {
                PlayerInput.InputAction input = worm.GetInput();
                if (input.AInput == 1)
                {
                    worm.effects.DisableAimLine();
                    worm.StopAttackWait();
                    StopAllCoroutines();
                }

                if (input.BInput == 1)
                    aiming = false;

                if (input.MoveNonZero || input.CamNonZero)
                {
                    worm.TurnPlayer(input.RawMoveInput.x*45f*Time.deltaTime);

                    throwForward = Mathf.Clamp(throwForward + input.RawMoveInput.y * Time.deltaTime*5f, 0, 20f);
                    throwUp = Mathf.Clamp(throwUp + input.CameraInput.y * Time.deltaTime*5f, -2f, 15f);
                }
            
                pos = worm.GetPos() + worm.GetForwards() * .5f;
            
                worm.effects.SetLine(pos, throwUp * Vector3.up + throwForward * worm.GetForwards(), 1f);

                zoom = Mathf.MoveTowards(zoom, .9f, Time.deltaTime);
                worm.SetCamZoom(zoom);

                yield return null;

            } while (aiming);

            float lastZoom = zoom;
        
            worm.effects.PlaySound((int)AudioSet.AudioID.hghWuh1);
            worm.effects.SetAnimTrigger("Throw");

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

            worm.effects.PlaySound((int)AudioSet.AudioID.hghWuh2);
        
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
}
