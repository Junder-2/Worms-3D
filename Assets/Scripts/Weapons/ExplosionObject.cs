using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionObject : MonoBehaviour, IEntity
{
    private float _maxDamage, _maxForce, _explosionRange;

    [SerializeField] private GameObject bombMesh;
    [SerializeField] private GameObject explosionEffect;

    private Rigidbody _rb;
    private AudioSource _audioSource;

    [SerializeField] private bool impactExplode = false;

    public void Instantiate(float maxDamage, float force, float fuseTime, float explosionRange)
    {
        _rb = GetComponent<Rigidbody>();
        _audioSource = GetComponent<AudioSource>();
        _maxDamage = maxDamage;
        _maxForce = force;
        _explosionRange = explosionRange;
        
        LevelController.Instance.SetCamFollow(transform);
        
        StartCoroutine(Explode(fuseTime));
    }

    private void LateUpdate()
    {
        if(_exploded) return;
        
        if (_rb.position.y < 0)
        {
            StopAllCoroutines();
            StartCoroutine(Explode(0f));
        }
    }

    IEnumerator Explode(float fuseTime)
    {
        _audioSource.PlayOneShot(AudioManager.Instance.GetAudioClip((int)AudioSet.AudioID.FuseLit), .5f);
        yield return new WaitForSeconds(fuseTime);
        _audioSource.Stop();

        _exploded = true;
        _rb.isKinematic = true;
        bombMesh.SetActive(false);
        explosionEffect.SetActive(true);
        _audioSource.PlayOneShot(AudioManager.Instance.GetAudioClip((int)AudioSet.AudioID.Explosion));
        explosionEffect.transform.localScale = Vector3.one*(_explosionRange*2);

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
        
        DestroySelf();
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

    private void OnCollisionEnter(Collision collision)
    {
        if(!impactExplode) return;
        
        if(_exploded)
            return;
        StopAllCoroutines();
        StartCoroutine(Explode(0));
    }

    void DestroySelf()
    {
        LevelController.Instance.CancelCamFollow();
        Destroy(gameObject);
    }
}
