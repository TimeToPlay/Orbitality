using System;
using SO;
using UnityEngine;

namespace Models
{
    /// <summary>
    /// Model contains rocket info within space
    /// </summary>
    [Serializable]
    public class RocketModel
    {
        [SerializeField] private RocketType rocketType;
        [SerializeField] private float posX;
        [SerializeField] private float posY;
        [SerializeField] private float rotationZ;
        [SerializeField] private float velocity;

        public RocketType RocketType
        {
            get => rocketType;
            set => rocketType = value;
        }

        public float PosX => posX;

        public float PosY => posY;

        public float RotationZ => rotationZ;

        public float Velocity => velocity;

        public void UpdateState(Vector3 pos, Quaternion rotation, float velocity)
        {
            posX = pos.x;
            posY = pos.y;
            rotationZ = rotation.eulerAngles.z;
            this.velocity = velocity;
        }

        public void UpdateState(Transform transform, float velocityMagnitude)
        {
            UpdateState(transform.position, transform.rotation, velocityMagnitude);
        }
    }
}