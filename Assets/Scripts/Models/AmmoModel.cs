using System;
using SO;
using UnityEngine;

namespace Models
{
    /// <summary>
    /// Planet ammunition model
    /// </summary>
    [Serializable]
    public class AmmoModel
    {
        [SerializeField] private RocketType rocketType;
        [SerializeField] private int ammo;
        public RocketType RocketType => rocketType;
        public int Ammo => ammo;
        public AmmoModel(RocketType rocketType, int ammoCount)
        {
            this.rocketType = rocketType;
            this.ammo = ammoCount;
        }

        public void DecrementAmmo()
        {
            ammo--;
            if (ammo < 0) ammo = 0;
        }
    }
}