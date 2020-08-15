using System.Linq;

using UnityEngine.InputSystem;

namespace BoostBlasters.Input
{
    /// <summary>
    /// The input for actions which can be triggered by any player.
    /// </summary>
    public class GlobalInput : BaseInput
    {
        private InputActionAsset m_actions;


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
            m_actions = CloneActions(m_primaryInput.actionsAsset);

            m_primaryInput.actionsAsset = m_actions;
            m_secondaryInput.actionsAsset = m_actions;

            m_actions.devices = InputSystem.devices;

            base.OnEnable();
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
            m_actions.devices = InputSystem.devices
                .Where(d => !InputManager.Users.Any(user => user.Player.devices.Contains(d)))
                .ToArray();
        }

        private InputActionAsset CloneActions(InputActionAsset actions)
        {
            var newActions = Instantiate(actions);

            for (var actionMap = 0; actionMap < actions.actionMaps.Count; actionMap++)
            {
                for (var binding = 0; binding < actions.actionMaps[actionMap].bindings.Count; binding++)
                {
                    var inputBinding = actions.actionMaps[actionMap].bindings[binding];
                    newActions.actionMaps[actionMap].ApplyBindingOverride(binding, inputBinding);
                }
            }

            return newActions;
        }
    }
}
