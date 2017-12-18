using System;
using System.Collections.Generic;

public class RaceParameters
{
    private LevelConfig m_levelConfig;
    public LevelConfig LevelConfig { get { return m_levelConfig; } }

    private int m_laps;
    public int Laps { get { return m_laps; } }

    private int m_humanCount;
    public int HumanCount { get { return m_humanCount; } }

    private int m_aiCount;
    public int AICount { get { return m_aiCount; } }

    public int PlayerCount
    {
        get { return m_humanCount + m_aiCount; }
    }

    private List<PlayerConfig> m_playerConfigs;
    public List<PlayerConfig> PlayerConfigs { get { return m_playerConfigs; } }

    private List<PlayerProfile> m_profiles;
    public List<PlayerProfile> Profiles { get { return m_profiles; } }

    private List<PlayerBaseInput> m_inputs;
    public List<PlayerBaseInput> Inputs { get { return m_inputs; } }

    private List<int> m_playerIndicies;
    public List<int> PlayerIndicies { get { return m_playerIndicies; } }
    
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
