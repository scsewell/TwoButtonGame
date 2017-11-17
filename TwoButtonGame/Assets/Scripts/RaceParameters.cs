using System;
using System.Collections.Generic;
using System.Linq;

public class RaceParameters
{
    private LevelConfig m_levelConfig;
    public LevelConfig LevelConfig { get { return m_levelConfig; } }

    private int m_laps;
    public int Laps { get { return m_laps; } }

    private int m_humanCount;
    public int HumanCount { get { return m_humanCount; } }

    private List<int> m_playerIndicies;
    public List<int> PlayerIndicies { get { return m_playerIndicies; } }

    private int m_aiCount;
    public int AICount { get { return m_aiCount; } }

    private List<PlayerConfig> m_playerConfigs;
    public List<PlayerConfig> PlayerConfigs { get { return m_playerConfigs; } }
    
    public int PlayerCount
    {
        get { return m_humanCount + m_aiCount; }
    }

    public RaceParameters(int humanCount, List<int> playerIndicies, int aiCount, List<PlayerConfig> playerConfigs, LevelConfig levelConfig, int laps)
    {
        m_levelConfig = levelConfig;
        m_laps = laps;

        m_humanCount = humanCount;
        m_playerIndicies = playerIndicies;
        m_aiCount = aiCount;
        m_playerConfigs = playerConfigs;
    }

    public PlayerInput GetPlayerInput(int playerNum)
    {
        if (m_playerIndicies != null && m_playerIndicies.Count == m_humanCount)
        {
            return InputManager.Instance.PlayerInputs[m_playerIndicies[playerNum]];
        }
        return null;
    }
}
