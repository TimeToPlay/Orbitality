using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace SO
{
    [CreateAssetMenu(fileName = "SettingsSO", menuName = "SO/Game Settings", order = 51)]
    public class SettingsSO:  ScriptableObjectInstaller<SettingsSO>
    {
        public GameSettings gameSettings;
        public List<RocketSetting> rocketSettingList;
        [Serializable]
        public class RocketSetting
        {
            public RocketType rocketType;
            public float damage;
            public float acceleration;
            public float cooldown;
        }
        [Serializable]
        public class GameSettings
        {
            public int initialPlanetAmount;
        }
        public override void InstallBindings()
        {
            Container.BindInstance(rocketSettingList);
            Container.BindInstance(gameSettings);
        }
    }

    public enum RocketType
    {
        Normal,
        Fast,
        Deadly
    }
}