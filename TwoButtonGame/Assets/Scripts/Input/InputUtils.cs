using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;

namespace BoostBlasters.Input
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
            foreach (var device in InputSystem.devices)
            {
                InputSystem.EnableDevice(device);
            }

            s_currentDevice = null;

            InputSystem.onDeviceChange -= OnDeviceChange;
            InputSystem.onDeviceChange += OnDeviceChange;

            InputSystem.onAfterUpdate -= OnUpdate;
            InputSystem.onAfterUpdate += OnUpdate;
        }

        private static void OnDeviceChange(InputDevice device, InputDeviceChange change)
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
        /// <returns>True if the control is valid for this action and a control scheme match was found, false otherwise.</returns>
        public static bool TryGetControlScheme(this InputAction action, InputControl control, out InputControlScheme controlScheme)
        {
            if (action.TryGetControlBinding(control, out var binding) && binding.groups != null)
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
        /// Gets all the <see cref="InputControlScheme"/> that a given control belongs to.
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
                if (binding.groups == null)
                {
                    return true;
                }

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

        /// <summary>
        /// Gets the parent composite <see cref="InputBinding"/> that a part of composite binding belongs to.
        /// </summary>
        /// <param name="action">The action to get the binding from.</param>
        /// <param name="part">A part of composite binding to get the parent compsite binding of.</param>
        /// <param name="composite">The returned composite binding, or default if no suitable binding was found.</param>
        /// <returns>True if the parent bindings was found, false otherwise.</returns>
        public static bool TryGetCompositeFromPart(this InputAction action, InputBinding part, out InputBinding composite)
        {
            if (part.isPartOfComposite)
            {
                var hasLastComposite = false;
                var lastComposite = default(InputBinding);
                var bindings = action.bindings;

                for (var i = 0; i < bindings.Count; i++)
                {
                    var binding = bindings[i];

                    if (binding == part)
                    {
                        composite = lastComposite;
                        return hasLastComposite;
                    }
                    if (binding.isComposite)
                    {
                        lastComposite = binding;
                        hasLastComposite = true;
                    }
                }
            }

            composite = default;
            return false;
        }

        /// <summary>
        /// Gets the parent composite <see cref="InputBinding"/> that a part of composite binding belongs to.
        /// </summary>
        /// <param name="action">The action to get the binding from.</param>
        /// <param name="part">A part of composite binding to get the parent compsite binding of.</param>
        /// <returns>The returned composite binding, or null if no suitable binding was found.</returns>
        public static InputBinding? GetCompositeFromPart(this InputAction action, InputBinding part)
        {
            if (action.TryGetCompositeFromPart(part, out var binding))
            {
                return binding;
            }
            return null;
        }
    }
}
