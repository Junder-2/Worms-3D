using UnityEngine;

public interface IEntity
{
    public void Damage(float amount, Vector3 force);

    public Vector3 GetPos();
} 
