using System;
using System.Collections.Generic;
using SO;

namespace Models
{
    [Serializable]
    public class PlanetState
    {
        public int hp;
        public Dictionary<RocketType, int> rocketAmmo = new Dictionary<RocketType, int>();
        public float angleToSun;
        public SettingsSO.PlanetSettings settings;
        public bool isPlayer;
        public List<AmmoInfo> ammoInfoList;
    }
}