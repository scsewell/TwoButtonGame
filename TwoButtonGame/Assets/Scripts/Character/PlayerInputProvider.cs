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

    public void ResetProvider()
    {
    }

    public void UpdateProvider()
    {
    }

    public void FixedUpdateProvider()
    {
        float hAxis = Input.GetAxis("Horizontal");
        float vAxis = Input.GetAxis("Vertical");
        
        m_movementInputs = new MovementInputs();
        m_movementInputs.h = hAxis + (Input.GetKey(KeyCode.A) ? -1 : 0) + (Input.GetKey(KeyCode.D) ? 1 : 0);
        m_movementInputs.v = vAxis + (Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.D) ? 1 : 0) + (Input.GetKey(KeyCode.S) ? -1 : 0);
        m_movementInputs.boost  = Input.GetKey(KeyCode.Joystick1Button0) || Input.GetKey(KeyCode.W);
    }
    
    public MovementInputs GetInput()
    {
        return m_movementInputs;
    }
}
