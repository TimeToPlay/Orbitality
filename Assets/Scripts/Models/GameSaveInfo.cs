using System;
using System.Collections.Generic;

namespace Models
{
    [Serializable]
    public class GameSaveInfo
    {
        public List<PlanetState> PlanetStates = new List<PlanetState>();
        public List<RocketState> RocketStates = new List<RocketState>();

        public void PrepareForSerialize()
        {
            foreach (var state in PlanetStates)
            {
                state.PrepareForSerialize();
            }
        }

        public void PrepareForDeserialize()
        {
            foreach (var state in PlanetStates)
            {
                state.PrepareForDeserialize();
            }
        }
    }
}