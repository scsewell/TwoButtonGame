using System;
using System.Collections.Generic;
using System.Linq;

public class RaceParameters
{
    private LevelConfig m_levelConfig;
    public LevelConfig LevelConfig { get { return m_levelConfig; } }

    private int m_laps;
    public int Laps { get { return m_laps; } }

    private List<int> m_playerIndicies;
    public List<int> PlayerIndicies { get { return m_playerIndicies; } }

    private List<PlayerConfig> m_playerConfigs;
    public List<PlayerConfig> PlayerConfigs { get { return m_playerConfigs; } }

    public RaceParameters(LevelConfig levelConfig, int laps, List<int> playerIndicies, List<PlayerConfig> playerConfigs)
    {
        m_levelConfig = levelConfig;
        m_laps = laps;
        m_playerConfigs = playerConfigs;
        m_playerIndicies = playerIndicies;
    }

    public PlayerInput GetPlayerInput(int playerNum)
    {
        return InputManager.Instance.PlayerInputs[m_playerIndicies[playerNum]];
    }
}
