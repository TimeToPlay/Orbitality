using System;
using SO;

namespace Models
{
    [Serializable]
    public class AmmoInfo
    {
        public string RocketType;
        public int ammo;

        public RocketType GetRocketType()
        {
            return (RocketType) Enum.Parse(typeof(RocketType), RocketType);
        }
    }
}