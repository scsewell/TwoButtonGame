using UnityEngine;

public class PlayerInputProvider : IInputProvider
{
    private PlayerBaseInput m_playerInput;
    private MovementInputs m_movementInputs;

    public PlayerInputProvider(PlayerBaseInput input)
    {
        m_playerInput = input;
    }

    public void ResetProvider()
    {
    }

    public void UpdateProvider()
    {
    }

    public void FixedUpdateProvider()
    {
        m_movementInputs = new MovementInputs();
        m_movementInputs.h = m_playerInput.H;
        m_movementInputs.v = m_playerInput.V;
        m_movementInputs.boost = m_playerInput.Boost;
    }
    
    public MovementInputs GetInput()
    {
        return m_movementInputs;
    }
}
