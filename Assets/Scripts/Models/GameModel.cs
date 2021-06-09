using System;
using System.Collections.Generic;
using UnityEngine;

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

        public void AddNewPlanetModel(PlanetModel planetModel)
        {
            planetModels.Add(planetModel);
        }
    }
}