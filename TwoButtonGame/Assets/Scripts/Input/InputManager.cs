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

    public static string GetName(KeyCode key)
    {
        string str = key.ToString();
        switch (key)
        {
            case KeyCode.Escape: return "Esc";
            case KeyCode.Return: return "Enter";

            case KeyCode.Keypad0:
            case KeyCode.Keypad1:
            case KeyCode.Keypad2:
            case KeyCode.Keypad3:
            case KeyCode.Keypad4:
            case KeyCode.Keypad5:
            case KeyCode.Keypad6:
            case KeyCode.Keypad7:
            case KeyCode.Keypad8:
            case KeyCode.Keypad9: return "Num" + str.Substring(str.Length - 1);

            case KeyCode.LeftArrow: return "Left";
            case KeyCode.RightArrow: return "Right";
            case KeyCode.UpArrow: return "Up";
            case KeyCode.DownArrow: return "Down";
        }
        return str;
    }
}
