using System;
using System.Collections.Generic;
using SO;

namespace Models
{
    [Serializable]
    public class PlanetState
    {
        public int hp;
        public float currentAngleToSun;
        public Dictionary<RocketType, int> rocketAmmo = new Dictionary<RocketType, int>();
        public SettingsSO.PlanetSettings settings;
        public bool isPlayer;
        public List<AmmoInfo> ammoInfoList;
        public string nickname;
        public SerializableColor color;

        public void PrepareForSerialize()
        {
            ammoInfoList = new List<AmmoInfo>();
            foreach (var key in rocketAmmo.Keys)
            {
                var ammoInfo = new AmmoInfo
                {
                    ammo = rocketAmmo[key],
                    RocketType = key.ToString()
                };
                ammoInfoList.Add(ammoInfo);
            }
        }

        public void PrepareForDeserialize()
        {
            foreach (var ammoInfo in ammoInfoList)
            {
                rocketAmmo.Add(ammoInfo.GetRocketType(), ammoInfo.ammo);
            }
        }
    }
}