using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine.InputSystem;

namespace BoostBlasters.Input
{
    /// <summary>
    /// The input for a single player.
    /// </summary>
    public class UserInput : BaseInput
    {
        private static readonly Dictionary<Mode, string> s_maps =
            Enum.GetValues(typeof(Mode)).Cast<Mode>()
            .ToDictionary(n => n, n => n.ToString());

        /// <summary>
        /// The input mode for a player.
        /// </summary>
        public enum Mode
        {
            /// <summary>
            /// The mode used to stop all input for a player.
            /// </summary>
            None,
            /// <summary>
            /// The input used for the game.
            /// </summary>
            Player,
            /// <summary>
            /// The input used for the UI.
            /// </summary>
            UI,
        }

        /// <summary>
        /// The player input backing this user.
        /// </summary>
        public PlayerInput Player { get; private set; }

        /// <summary>
        /// The mode determining what inputs are currently enabled for this user.
        /// </summary>
        public Mode ActiveMode { get; private set; }


        protected override void OnEnable()
        {
            transform.SetParent(PlayerInputManager.instance.transform, false);

            Player = GetComponent<PlayerInput>();
            m_primaryInput.actionsAsset = Player.actions;
            m_secondaryInput.actionsAsset = Player.actions;

            SetMode(Mode.UI);

            base.OnEnable();
        }

        /// <summary>
        /// Sets the input mode for this user.
        /// </summary>
        /// <param name="map">The new input mode.</param>
        public void SetMode(Mode map)
        {
            if (ActiveMode != map)
            {
                if (map == Mode.None)
                {
                    Player.DeactivateInput();
                }
                else
                {
                    Player.ActivateInput();
                    Player.SwitchCurrentActionMap(s_maps[map]);
                }

                ActiveMode = map;
            }
        }

        /// <summary>
        /// Unjoins this user.
        /// </summary>
        public void Leave()
        {
            Destroy(gameObject);
        }
    }
}
