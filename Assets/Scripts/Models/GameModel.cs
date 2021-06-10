using System;
using System.Collections.Generic;
using SO;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Models
{
    [Serializable]
    public class GameModel
    {
        [SerializeField] private List<PlanetModel> planetModels = new List<PlanetModel>();
        [SerializeField] private List<RocketModel> rocketModels = new List<RocketModel>();

        public IEnumerable<RocketModel> GetRocketModels()
        {
            return rocketModels;
        }

        public IEnumerable<PlanetModel> GetPlanetModels()
        {
            return planetModels;
        }

        public void ClearRocketStates()
        {
            rocketModels.Clear();
        }

        public void AddNewRocketModel(RocketModel rocketModel)
        {
            rocketModels.Add(rocketModel);
        }

        public void ClearPlanetStates()
        {
            planetModels.Clear();
        }

        public void CreateRandomizedState(SettingsSO.GameSettings gameSettings,
            List<SettingsSO.RocketSettings> rocketSettings)
        {
            var playerIndex = Random.Range(0, gameSettings.initialPlanetAmount);
            float previousOrbit = gameSettings.MinimumOrbitRadius;
            ClearPlanetStates();
            for (int i = 0; i < gameSettings.initialPlanetAmount; i++)
            {
                var randomPlanetSetting = new SettingsSO.PlanetSettings();
                randomPlanetSetting.planetScale =
                    Random.Range(gameSettings.MinPlanetScale, gameSettings.MaxPlanetScale);
                randomPlanetSetting.orbitRadius =
                    previousOrbit + randomPlanetSetting.planetScale / 2 + Random.Range(2, 7);
                previousOrbit = randomPlanetSetting.orbitRadius + randomPlanetSetting.planetScale / 2;
                randomPlanetSetting.clockwise = Random.value > 0.5f;
                randomPlanetSetting.solarAngularVelocity = Random.Range(gameSettings.SolarAngularVelocityMin,
                    gameSettings.SolarAngularVelocityMax);
                randomPlanetSetting.selfRotationVelocity = Random.Range(gameSettings.SelfRotationVelocityMin,
                    gameSettings.SelfRotationVelocityMax);
                var randomColor = new Color(Random.Range(0.2f, 1f), Random.Range(0.2f, 1f), Random.Range(0.2f, 1f));
                var randomAngleToSun = Random.Range(0, Mathf.PI * 2);
                var isPlayer = playerIndex == i;
                var planetModel = new PlanetModel(randomPlanetSetting,
                    gameSettings.initialPlanetHP,
                    isPlayer,
                    randomAngleToSun,
                    randomColor
                );
                foreach (var rocket in rocketSettings)
                {
                    planetModel.AddRocketAmmo(rocket.rocketType, Random.Range(rocket.minAmmo, rocket.maxAmmo));
                }

                planetModels.Add(planetModel);
            }
        }
    }
}