using UnityEngine;

/// <summary>
/// Base class for all planets and sun
/// </summary>
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