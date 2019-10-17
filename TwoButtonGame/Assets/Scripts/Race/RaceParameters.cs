using System;
using System.Collections.Generic;

using BoostBlasters.Players;
using BoostBlasters.Character;
using BoostBlasters.Levels;

namespace BoostBlasters.Races
{
    /// <summary>
    /// The configuration of a race.
    /// </summary>
    public class RaceParameters
    {
        private LevelConfig m_levelConfig = null;
        public LevelConfig LevelConfig => m_levelConfig;

        private int m_laps = 0;
        public int Laps => m_laps;

        private int m_humanCount = 0;
        public int HumanCount => m_humanCount;

        private int m_aiCount = 0;
        public int AICount => m_aiCount;

        public int RacerCount => m_humanCount + m_aiCount;

        private List<PlayerConfig> m_playerConfigs = null;
        public List<PlayerConfig> PlayerConfigs => m_playerConfigs;

        private List<PlayerProfile> m_profiles = null;
        public List<PlayerProfile> Profiles => m_profiles;

        private List<PlayerBaseInput> m_inputs = null;
        public List<PlayerBaseInput> Inputs => m_inputs;

        private List<int> m_playerIndicies = null;
        public List<int> PlayerIndicies => m_playerIndicies;

        public RaceParameters(
            LevelConfig levelConfig,
            int laps,
            int humanCount,
            int aiCount,
            List<PlayerConfig> playerConfigs,
            List<PlayerProfile> profiles,
            List<PlayerBaseInput> inputs,
            List<int> playerIndicies
        )
        {
            m_levelConfig = levelConfig;
            m_laps = laps;

            m_humanCount = humanCount;
            m_aiCount = aiCount;
            m_playerConfigs = playerConfigs;
            m_profiles = profiles;
            m_inputs = inputs;
            m_playerIndicies = playerIndicies;
        }
    }
}
