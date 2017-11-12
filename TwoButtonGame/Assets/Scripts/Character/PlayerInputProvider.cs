using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerInputProvider : IInputProvider
{
    private PlayerInput m_playerInput;
    private MovementInputs m_movementInputs;

    public PlayerInputProvider(PlayerInput input)
    {
        m_playerInput = input;
    }

    public void UpdateProvider()
    {
    }

    public void FixedUpdateProvider()
    {
        m_movementInputs = new MovementInputs();
        m_movementInputs.left       = m_playerInput.Button2.IsDown;
        m_movementInputs.right      = m_playerInput.Button1.IsDown;
        m_movementInputs.boost      = m_playerInput.BothDoubleTapHeld;
    }
    
    public MovementInputs GetInput()
    {
        return m_movementInputs;
    }
}
