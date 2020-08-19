using System.Linq;

using UnityEngine.InputSystem;

namespace BoostBlasters.Input
{
    /// <summary>
    /// The input which is the combined inputs of all players.
    /// </summary>
    /// <remarks>
    /// This is useful for cases where all users should be able to
    /// control the same thing (ex. Main Menu UI).
    /// </remarks>
    public class GlobalInput : BaseInput
    {
        private void Awake()
        {
            InputManager.UserAdded += OnUserAdded;
            InputManager.UserRemoved += OnUserRemoved;

            InputSystem.onDeviceChange += OnDeviceChange;
        }

        private void OnDestroy()
        {
            InputManager.UserAdded -= OnUserAdded;
            InputManager.UserRemoved -= OnUserRemoved;

            InputSystem.onDeviceChange -= OnDeviceChange;
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            //Actions.asset.devices = InputSystem.devices;
        }

        private void OnDeviceChange(InputDevice device, InputDeviceChange change)
        {
            switch (change)
            {
                case InputDeviceChange.Added:
                case InputDeviceChange.Destroyed:
                case InputDeviceChange.Reconnected:
                case InputDeviceChange.Disconnected:
                {
                    UpdateDevices();
                    break;
                }
            }
        }

        private void OnUserAdded(UserInput user)
        {
            UpdateDevices();
        }

        private void OnUserRemoved(UserInput user)
        {
            UpdateDevices();
        }

        private void UpdateDevices()
        {
            // only use device that are not paired to a specific user
            //Actions.asset.devices = InputSystem.devices
            //    .Where(d => !InputManager.Users.Any(user => user.Player.devices.Contains(d)))
            //    .ToArray();
        }
    }
}
