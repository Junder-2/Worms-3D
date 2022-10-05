using System.Collections;
using Player;
using UnityEngine;

namespace Weapons
{
    public class WeaponBat : Weapon
    {
        [SerializeField] private float launchKnockback;

        public override void UseWeapon(WormController worm)
        {
            StartCoroutine(SwingBat(worm));
        }

        IEnumerator SwingBat(WormController worm)
        {
            worm.effects.SetAnimInt("Swing", 1);
            worm.effects.PlaySound((int)AudioSet.AudioID.ughhYa1);
        
            RaycastHit hit;

            Vector3 wormPos = worm.GetPos();
            Vector3 wormForwards = worm.GetForwards();

            float timer = 0;

            bool chargeBat = true;
            float chargeStrength = .5f;
            float zoom = 1f;

            float lastZoom;

            yield return new WaitForSeconds(.2f);

            do
            {
                PlayerInput.InputAction input = worm.GetInput();

                if (input.BInput == 0)
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

            worm.effects.PlaySound((int)AudioSet.AudioID.ughhYa2);

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
                
                    worm.effects.SetAnimInt("Swing", 2);
                    //worm.effects.SetAnimTrigger("SwingB");

                    yield return new WaitForSeconds(.1f);
                
                    PlayAudio((int)AudioSet.AudioID.batImpact, Mathf.Max(chargeStrength-1, 0.2f));

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

                    StopAllCoroutines();
                }
            }

            worm.effects.SetAnimInt("Swing", 2);
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
