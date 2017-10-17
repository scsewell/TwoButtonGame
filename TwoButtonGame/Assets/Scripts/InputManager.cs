using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Framework;

public class InputManager : Singleton<InputManager>
{
    private PlayerInput[] m_playerInputs =
    {
        new PlayerInput(KeyCode.A, KeyCode.S),
        new PlayerInput(KeyCode.LeftArrow, KeyCode.DownArrow),
        new PlayerInput(KeyCode.J, KeyCode.K),
        new PlayerInput(KeyCode.Keypad2, KeyCode.Keypad3),
    };

    public PlayerInput[] PlayerInputs
    {
        get { return m_playerInputs; }
    }

    public void Update()
    {
        foreach (PlayerInput input in m_playerInputs)
        {
            input.Update();
        }
    }

    public void LateUpdate()
    {
        foreach (PlayerInput input in m_playerInputs)
        {
            input.LateUpdate();
        }
    }
}
