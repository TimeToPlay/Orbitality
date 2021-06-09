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
        public List<RocketSettings> rocketSettingList;
        public PlanetSettings playerPlanetSettings;
        
        [Serializable]
        public class RocketSettings
        {
            public RocketType rocketType;
            public int damage;
            public float acceleration;
            public float cooldown;
            public int minAmmo;
            public int maxAmmo;
        }
        [Serializable]
        public class GameSettings
        {
            public int initialPlanetAmount;
            public float MinimumOrbitRadius = 15;
            public float MinPlanetScale = 4f;
            public float MaxPlanetScale = 8;
            public float SolarAngularVelocityMin = 0.2f;
            public float SolarAngularVelocityMax = 0.3f;
            public float SelfRotationVelocityMin = 200;
            public float SelfRotationVelocityMax = 300;
            public int initialPlanetHP;
        }

        [Serializable]
        public class PlanetSettings
        {
            public bool clockwise;
            public float planetScale;
            public float orbitRadius;
            public float solarAngularVelocity;
            public float selfRotationVelocity;
        }
        public override void InstallBindings()
        {
            Container.BindInstance(rocketSettingList);
            Container.BindInstance(gameSettings);
            Container.BindInstance(playerPlanetSettings);
        }
    }

    [Serializable]
    public enum RocketType
    {
        Normal,
        Fast,
        Deadly
    }
}