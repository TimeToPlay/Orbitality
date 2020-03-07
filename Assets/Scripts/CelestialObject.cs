using UnityEngine;

namespace DefaultNamespace
{
    public class CelestialObject : MonoBehaviour
    {
        [SerializeField] private float mass;
        public virtual float GetGravityModifier()
        {
            return mass;
        }
    }
}