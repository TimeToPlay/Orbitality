using UnityEngine;

public class CelestialObject : MonoBehaviour, IDamageReceiver
{
    [SerializeField] private float mass;

    public virtual float GetGravityModifier()
    {
        return mass;
    }

    public virtual void ReceiveDamage(int damage)
    {
    }
}

public interface IDamageReceiver
{
    void ReceiveDamage(int damage);
}