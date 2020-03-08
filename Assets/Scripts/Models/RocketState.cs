using System;
using SO;

namespace Models
{
    [Serializable]
    public class RocketState
    {
        public string rocketType;
        public float posX;
        public float posY;
        public float rotationZ;
        public float velocity;

        public RocketType GetRocketType()
        {
            return (RocketType)Enum.Parse(typeof(RocketType), rocketType);
        }
    }
}