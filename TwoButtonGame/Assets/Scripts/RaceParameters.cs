using System;
using System.Collections.Generic;
using System.Linq;

public class RaceParameters
{
    private int m_humanCount;
    public int HumanCount { get { return m_humanCount; } }

    private List<int> m_playerIndicies;
    public List<int> PlayerIndicies { get { return m_playerIndicies; } }

    private int m_aiCount;
    public int AICount { get { return m_aiCount; } }

    private List<PlayerConfig> m_playerConfigs;
    public List<PlayerConfig> PlayerConfigs { get { return m_playerConfigs; } }

    private LevelConfig m_levelConfig;
    public LevelConfig LevelConfig { get { return m_levelConfig; } }

    private int m_laps;
    public int Laps { get { return m_laps; } }

    public RaceParameters(int humanCount, List<int> playerIndicies, int aiCount, List<PlayerConfig> playerConfigs, LevelConfig levelConfig, int laps)
    {
        m_humanCount = humanCount;
        m_playerIndicies = playerIndicies;
        m_aiCount = aiCount;
        m_playerConfigs = playerConfigs;
        m_levelConfig = levelConfig;
        m_laps = laps;
    }

    public PlayerInput GetPlayerInput(int playerNum)
    {
        return InputManager.Instance.PlayerInputs[m_playerIndicies[playerNum]];
    }
}
