using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionObject : MonoBehaviour, IEntity
{
    private float _maxDamage, _maxForce, _explosionRange;

    [SerializeField] private GameObject bombMesh;
    [SerializeField] private GameObject explosionEffect;

    private Rigidbody _rb;
    
    public void Instantiate(float maxDamage, float force, float fuseTime, float explosionRange)
    {
        _rb = GetComponent<Rigidbody>();
        _maxDamage = maxDamage;
        _maxForce = force;
        _explosionRange = explosionRange;

        StartCoroutine(Explode(fuseTime));
    }

    IEnumerator Explode(float fuseTime)
    {
        yield return new WaitForSeconds(fuseTime);

        _exploded = true;
        _rb.isKinematic = true;
        bombMesh.SetActive(false);
        explosionEffect.SetActive(true);
        explosionEffect.transform.localScale = Vector3.one*_explosionRange;

        Collider[] hits;

        hits = Physics.OverlapSphere(transform.position, _explosionRange);

        foreach (var hit in hits)
        {
            var entity = hit.transform.GetComponent<IEntity>();

            if (entity != null)
            {
                Vector3 dir = (entity.GetPos() - transform.position);
                
                float dist = dir.magnitude;

                dist = 1- dist / _explosionRange;

                dir = dir.normalized + Vector3.up;
                
                entity.Damage(_maxDamage*dist, dir*(_maxForce*dist));
            }
        }

        yield return new WaitForSeconds(.5f);
        
        Destroy(gameObject);
    }

    private bool _exploded = false;
    public void Damage(float amount, Vector3 force)
    {
        if(_exploded)
            return;
        StopAllCoroutines();
        StartCoroutine(Explode(0.1f));
    }

    public Vector3 GetPos()
    {
        return _rb.position;
    }
}
