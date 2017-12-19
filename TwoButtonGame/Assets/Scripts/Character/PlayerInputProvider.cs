using UnityEngine;

public class PlayerInputProvider : IInputProvider
{
    private PlayerBaseInput m_playerInput;
    private MovementInputs m_movementInputs;
    private bool m_boost;

    public PlayerInputProvider(PlayerBaseInput input)
    {
        m_playerInput = input;
        m_boost = false;
    }

    public void ResetProvider()
    {
    }

    public void LateUpdateProvider()
    {
        if (Time.timeScale > 0 && m_playerInput.Boost)
        {
            m_boost = true;
        }
    }

    public void FixedUpdateProvider()
    {
        m_movementInputs = new MovementInputs();
        m_movementInputs.h = m_playerInput.H;
        m_movementInputs.v = m_playerInput.V;
        m_movementInputs.boost = m_boost;

        m_boost = false;
    }
    
    public MovementInputs GetInput()
    {
        return m_movementInputs;
    }
}
