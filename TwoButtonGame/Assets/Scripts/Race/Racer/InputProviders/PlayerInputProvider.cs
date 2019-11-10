using UnityEngine;

namespace BoostBlasters.Races.Racers
{
    /// <summary>
    /// Generates movement input from human input.
    /// </summary>
    public class PlayerInputProvider : IInputProvider
    {
        private PlayerBaseInput m_playerInput = null;
        private Inputs m_movementInputs = default;
        private bool m_boost = false;


        public PlayerInputProvider(PlayerBaseInput input)
        {
            m_playerInput = input;
            ResetProvider();
        }

        public void ResetProvider()
        {
            m_boost = false;
        }

        public Inputs GetInput()
        {
            return m_movementInputs;
        }

        public void FixedUpdateProvider()
        {
            m_movementInputs = new Inputs();
            m_movementInputs.H = m_playerInput.H;
            m_movementInputs.V = m_playerInput.V;
            m_movementInputs.Boost = m_boost;
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
    }
}
