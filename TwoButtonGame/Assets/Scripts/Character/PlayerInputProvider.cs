using UnityEngine;

public class PlayerInputProvider : IInputProvider
{
    private PlayerBaseInput m_playerInput;
    private MovementInputs m_movementInputs;
    private bool m_boost;

    public PlayerInputProvider(PlayerBaseInput input)
    {
        m_playerInput = input;
        ResetProvider();
    }

    public void ResetProvider()
    {
        m_boost = false;
    }

    public void LateUpdateProvider()
    {
        if (Time.timeScale > 0)
        {
            if (m_playerInput.BoostPress)
            {
                m_boost = true;
            }
            if (m_playerInput.BoostRelease)
            {
                m_boost = false;
            }
        }
        else
        {
            m_boost = false;
        }
    }

    public void FixedUpdateProvider()
    {
        m_movementInputs = new MovementInputs();
        m_movementInputs.h = m_playerInput.H;
        m_movementInputs.v = m_playerInput.V;
        m_movementInputs.boost = m_boost;
    }
    
    public MovementInputs GetInput()
    {
        return m_movementInputs;
    }
}
