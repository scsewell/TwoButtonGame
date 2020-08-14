using UnityEngine;
using Framework;

public class InputManagerOld : ComponentSingleton<InputManagerOld>
{
    private PlayerBaseInput[] m_playerInputs =
    {
        new PlayerJoystickInput(1),
        new PlayerJoystickInput(2),
        new PlayerJoystickInput(3),
        new PlayerJoystickInput(4),
        new PlayerKeyboardInput(
            KeyCode.LeftArrow,
            KeyCode.RightArrow,
            KeyCode.UpArrow,
            KeyCode.DownArrow,
            KeyCode.UpArrow,
            KeyCode.Return,
            KeyCode.RightControl,
            KeyCode.Delete
        ),
        new PlayerKeyboardInput(
            KeyCode.A, 
            KeyCode.D, 
            KeyCode.W, 
            KeyCode.S, 
            KeyCode.W,
            KeyCode.E, 
            KeyCode.Q,
            KeyCode.Escape
        ),
    };

    public PlayerBaseInput[] PlayerInputs
    {
        get { return m_playerInputs; }
    }

    public void Update()
    {
        foreach (PlayerBaseInput input in m_playerInputs)
        {
            input.Update();
        }
    }

    public void SetKeyboardMuting(InputMuting muting)
    {
        foreach (PlayerBaseInput input in m_playerInputs)
        {
            input.SetMuting(muting);
        }
    }
}
