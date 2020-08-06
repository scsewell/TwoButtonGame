using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;

namespace BoostBlasters
{
    /// <summary>
    /// Utility methods for handling input.
    /// </summary>
    public static class InputUtils
    {
        private static InputDevice s_currentDevice = null;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Init()
        {
            s_currentDevice = null;

            InputSystem.onDeviceChange -= OnDeviceChanged;
            InputSystem.onDeviceChange += OnDeviceChanged;

            InputSystem.onAfterUpdate -= OnUpdate;
            InputSystem.onAfterUpdate += OnUpdate;
        }

        private static void OnDeviceChanged(InputDevice device, InputDeviceChange change)
        {
            switch (change)
            {
                case InputDeviceChange.Destroyed:
                case InputDeviceChange.Disconnected:
                {
                    if (s_currentDevice == device)
                    {
                        s_currentDevice = null;
                    }
                    break;
                }
            }
        }

        private static void OnUpdate()
        {
            // find which device was last used
            if (s_currentDevice == null || !IsActive(s_currentDevice))
            {
                if (s_currentDevice != Keyboard.current && IsActive(Keyboard.current))
                {
                    s_currentDevice = Keyboard.current;
                }
                else if (s_currentDevice != Gamepad.current && IsActive(Gamepad.current))
                {
                    s_currentDevice = Gamepad.current;
                }
            }
        }

        private static bool IsActive(InputDevice device)
        {
            switch (device)
            {
                case Keyboard keyboard:
                {
                    return keyboard.anyKey.isPressed;
                }
                default:
                {
                    var controls = device.allControls;

                    for (var i = 0; i < controls.Count; ++i)
                    {
                        var control = controls[i];

                        if (control.noisy || control.synthetic)
                        {
                            continue;
                        }

                        if (control.children.Count > 0)
                        {
                            continue;
                        }

                        if (control.IsActuated(0.5f))
                        {
                            return true;
                        }
                    }

                    return false;
                }
            }
        }

        /// <summary>
        /// Gets the device that was most recently actuated, or if multiple device are
        /// currently actuated, the device with the longest duration actuation.
        /// </summary>
        /// <returns>The current device, or null if none is available.</returns>
        public static InputDevice GetCurrentDevice() => s_currentDevice;

        /// <summary>
        /// Gets the <see cref="InputBinding"/> from the action that corresponds to a given control.
        /// </summary>
        /// <param name="action">The action to get the binding from.</param>
        /// <param name="control">The control to get the bindng for.</param>
        /// <param name="binding">The returned control binding, or default if no suitable binding for
        /// the control was found.</param>
        /// <returns>True if the control is valid for this action, false otherwise.</returns>
        public static bool TryGetControlBinding(this InputAction action, InputControl control, out InputBinding binding)
        {
            var bindings = action.bindings;

            for (var i = 0; i < bindings.Count; i++)
            {
                binding = bindings[i];

                if (InputControlPath.Matches(binding.path, control))
                {
                    return true;
                }
            }

            binding = default;
            return false;
        }

        /// <summary>
        /// Gets the <see cref="InputControlScheme"/> that a given control belongs to.
        /// </summary>
        /// <param name="action">The action the control is for.</param>
        /// <param name="control">The control to get the control scheme for.</param>
        /// <param name="controlScheme">The returned control scheme, or default if no matching scheme
        /// for the control was found.</param>
        /// <returns>True if the control is valid for this action, false otherwise.</returns>
        public static bool TryGetControlScheme(this InputAction action, InputControl control, out InputControlScheme controlScheme)
        {
            if (action.TryGetControlBinding(control, out var binding))
            {
                var schemes = action.actionMap.controlSchemes;

                for (var i = 0; i < schemes.Count; i++)
                {
                    controlScheme = schemes[i];

                    if (binding.groups.Contains(controlScheme.name))
                    {
                        return true;
                    }
                }
            }

            controlScheme = default;
            return false;
        }


        /// <summary>
        /// Gets the <see cref="InputControlScheme"/> that a given control belongs to.
        /// </summary>
        /// <param name="action">The action the control is for.</param>
        /// <param name="control">The control to get the control scheme for.</param>
        /// <param name="controlScheme">The list used to return any matching control schemes.</param>
        /// <returns>True if the control is valid for this action, false otherwise.</returns>
        public static bool TryGetControlSchemes(this InputAction action, InputControl control, List<InputControlScheme> controlSchemes)
        {
            controlSchemes.Clear();

            if (action.TryGetControlBinding(control, out var binding))
            {
                var schemes = action.actionMap.controlSchemes;

                for (var i = 0; i < schemes.Count; i++)
                {
                    var scheme = schemes[i];

                    if (binding.groups.Contains(scheme.name))
                    {
                        controlSchemes.Add(scheme);
                    }
                }

                return true;
            }

            return false;
        }
    }
}
