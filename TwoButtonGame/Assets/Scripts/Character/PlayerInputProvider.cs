using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerInputProvider : IInputProvider
{
    private PlayerInput m_playerInput;
    private MovementInputs m_movementInputs;
    private bool m_bothDoubleTap = false;

    public PlayerInputProvider(PlayerInput input)
    {
        m_playerInput = input;
    }

    public void UpdateProvider()
    {
        if (m_playerInput.BothDoubleTap)
        {
            m_bothDoubleTap = true;
        }
    }

    public void FixedUpdateProvider()
    {
        m_movementInputs = new MovementInputs();
        m_movementInputs.left           = m_playerInput.Button2.IsDown;
        m_movementInputs.right          = m_playerInput.Button1.IsDown;
        m_movementInputs.boost          = m_bothDoubleTap;
        m_movementInputs.boostHold      = m_playerInput.BothDoubleTapHeld;

        m_bothDoubleTap = false;
    }
    
    public MovementInputs GetInput()
    {
        return m_movementInputs;
    }
}
