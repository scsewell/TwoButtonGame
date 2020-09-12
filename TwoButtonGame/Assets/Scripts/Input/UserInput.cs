using UnityEngine.InputSystem;

namespace BoostBlasters.Input
{
    /// <summary>
    /// The input of a single player.
    /// </summary>
    public class UserInput : BaseInput
    {
        private PlayerInput m_player;

        /// <summary>
        /// The unique zero-based index of the player.
        /// </summary>
        /// <remarks>
        /// The player index of this user will not change, even if other users join or leave.
        /// This does not correspond to the index in the <see cref="InputManager.Users"/> list.
        /// </remarks>
        public int PlayerIndex { get; private set; }

        /// <summary>
        /// The device this user is paired with.
        /// </summary>
        public InputDevice Device { get; private set; }

        /// <summary>
        /// The control scheme this user paired with.
        /// </summary>
        public InputControlScheme ControlScheme { get; private set; }


        protected override void Awake()
        {
            m_player = GetComponent<PlayerInput>();
            transform.SetParent(PlayerInputManager.instance.transform, false);

            base.Awake();

            m_player.actions = Actions.asset;

            PlayerIndex = m_player.playerIndex;
            Device = m_player.devices[0];
            ControlScheme = m_player.user.controlScheme.Value;
        }

        /// <summary>
        /// Unjoins this user.
        /// </summary>
        public void Leave()
        {
            Destroy(gameObject);
        }

        /// <summary>
        /// Checks if this user is using a given set of controls.
        /// </summary>
        /// <param name="device">The device the controls are from.</param>
        /// <param name="scheme">The control scheme the controls are from.</param>
        /// <returns>True if this user matches the device and control scheme; false otherwise.</returns>
        public bool Matches(InputDevice device, InputControlScheme scheme)
        {
            return Device == device && ControlScheme == scheme;
        }

        public override string ToString()
        {
            return $"{Device.name}-{ControlScheme.name}";
        }
    }
}
