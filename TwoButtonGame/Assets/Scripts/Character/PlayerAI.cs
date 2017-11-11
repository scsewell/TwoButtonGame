using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerAI : IInputProvider
{
    private Player m_player;
    private MovementInputs m_input;

    public PlayerAI(Player player)
    {
        m_player = player;
    }

    public void UpdateProvider()
    {
    }

    public void FixedUpdateProvider()
    {
        m_input = new MovementInputs();

        Waypoint next = m_player.NextWaypoint;
        if (next != null)
        {
            Vector3 disp = next.Position - m_player.transform.position;
            Vector3 fore = m_player.transform.forward;

            Vector3 foreCross = Vector3.Cross(disp, fore);
            float facing = Mathf.Acos(Vector3.Dot(Vector3.ProjectOnPlane(disp, Vector3.up).normalized, fore)) * Mathf.Rad2Deg;

            if (facing > 3.5f)
            {
                m_input.left = foreCross.y < 0;
                m_input.right = foreCross.y > 0;
            }
            else
            {
                if (disp.magnitude > 65 && Mathf.Abs(disp.normalized.y) < 0.1f)
                {
                    m_input.boost = true;
                }

                if (disp.y > 0)
                {
                    m_input.left = true;
                    m_input.right = true;
                }
            }
        }
    }

    public MovementInputs GetInput()
    {
        return m_input;
    }
}
