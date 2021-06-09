using System;
using System.Collections.Generic;
using SO;
using UnityEngine;

namespace Models
{
    /// <summary>
    /// Planet state and info
    /// </summary>
    [Serializable]
    public class PlanetModel
    {
        [SerializeField] private int hp;
        [SerializeField] private float currentAngleToSun;
        [SerializeField] private SettingsSO.PlanetSettings settings;
        [SerializeField] private bool isPlayer;
        [SerializeField] private List<AmmoModel> ammoInfoList = new List<AmmoModel>();
        [SerializeField] private string nickname;
        [SerializeField] private SerializableColor color;

        public PlanetModel(SettingsSO.PlanetSettings planetSetting, int initialPlanetHp, bool isPlayer, float currentAngleToSun, Color color)
        {
            settings = planetSetting;
            hp = initialPlanetHp;
            this.isPlayer = isPlayer;
            this.currentAngleToSun = currentAngleToSun;
            this.color = color;
        }

        public int Hp => hp;

        public float CurrentAngleToSun
        {
            get => currentAngleToSun;
            set => currentAngleToSun = value;
        }

        public SettingsSO.PlanetSettings Settings => settings;

        public bool IsPlayer => isPlayer;

        public string Nickname => nickname;

        public SerializableColor Color => color;

        public int GetAmmoByType(RocketType rocketType)
        {
            var foundAmmoInfo = ammoInfoList.Find(info => info.RocketType == rocketType);
            return foundAmmoInfo?.Ammo ?? 0;

        }

        public void AddRocketAmmo(RocketType rocketType, int ammoCount)
        {
            ammoInfoList.Add(new AmmoModel(rocketType, ammoCount));
        }

        public void DecrementAmmoByType(RocketType rocketType)
        {
            var foundAmmoInfo = ammoInfoList.Find(info => info.RocketType == rocketType);
            foundAmmoInfo?.DecrementAmmo();
        }

        public IEnumerable<AmmoModel> GetAmmoModelList()
        {
            return ammoInfoList;
        }

        public void SetNickname(string nick)
        {
            //NOTE: validate it somehow
            this.nickname = nick;
        }

        public void ApplyDamage(int damage)
        {
            hp -= damage;
            if (hp < 0) hp = 0;
        }
    }
}