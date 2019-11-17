using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

using TMPro;

using Framework.UI;

namespace BoostBlasters.UI
{
    /// <summary>
    /// Displays a control prompt.
    /// </summary>
    public class ControlPanel : MonoBehaviour
    {
        [Header("UI Elements")]

        [SerializeField]
        private TMP_Text m_description = null;

        [Header("Options")]

        [SerializeField] 
        private InputActionReference m_defaultAction = null;

        private class ImageMapping
        {
            public readonly Image image;
            public readonly List<Control> controls;

            public ImageMapping(Image image, Control control)
            {
                this.image = image;
                controls = new List<Control>() { control };
            }
        }

        private class Control
        {
            public readonly InputControl control;
            private readonly List<InputControlScheme> m_schemes;

            public Control(InputControl control, List<InputControlScheme> schemes)
            {
                this.control = control;
                m_schemes = schemes;
            }

            /// <summary>
            /// Gets if the control corresponds to the given device.
            /// </summary>
            public bool IsDevice(InputDevice device)
            {
                return control.device.deviceId == device.deviceId;
            }

            /// <summary>
            /// Checks if the provided input scheme supports this control.
            /// </summary>
            public bool HasScheme(InputControlScheme scheme)
            {
                for (int i = 0; i < m_schemes.Count; i++)
                {
                    if (m_schemes[i] == scheme)
                    {
                        return true;
                    }
                }
                return false;
            }

            /// <summary>
            /// Checks if this control is currently active.
            /// </summary>
            public bool IsCurrent()
            {
                return true;
            }
        }

        private InputAction m_action = null;
        private readonly List<ImageMapping> m_images = new List<ImageMapping>();

        /// <summary>
        /// The action to show the controls for.
        /// </summary>
        public InputAction Action
        {
            get => m_action;
            set
            {
                if (m_action != value)
                {
                    m_action = value;
                    Initialize();
                }
            }
        }

        /// <summary>
        /// The device for which to show the controls. When null
        /// controls are shown for all supported active devices.
        /// </summary>
        public InputDevice DeviceOverride { get; set; } = null;

        /// <summary>
        /// The control scheme for which to show the controls. When null
        /// controls are shown for all active control schemes.
        /// </summary>
        public InputControlScheme SchemeOverride { get; set; } = default;


        private void Awake()
        {
            m_action = m_defaultAction;
            Initialize();

            InputSystem.onDeviceChange += OnDeviceChanged;
        }

        private void OnDeviceChanged(InputDevice device, InputDeviceChange change)
        {
            switch (change)
            {
                case InputDeviceChange.Added:
                case InputDeviceChange.Destroyed:
                case InputDeviceChange.Reconnected:
                case InputDeviceChange.Disconnected:
                    Initialize();
                    break;
            }
        }

        private void Initialize()
        {
            // we need an action to show the controls for
            if (Action == null)
            {
                return;
            }

            // set the control description text
            if (m_description != null)
            {
                m_description.text = Action.name;
            }

            // remove the current action images
            foreach (ImageMapping mapping in m_images)
            {
                Destroy(mapping.image.gameObject);
            }
            m_images.Clear();

            // create the images
            foreach (InputControl control in Action.controls)
            {
                // get the sprite used to indicate this control
                Sprite sprite = GetSprite(control);

                // get the control schemes this control belongs to
                List<InputControlScheme> schemes = new List<InputControlScheme>();

                foreach (InputControlScheme scheme in Action.actionMap.asset.controlSchemes)
                {
                    if (scheme.SupportsDevice(control.device))
                    {
                        schemes.Add(scheme);
                    }
                }

                // check if an image using this sprite has already been created for a separate control
                bool redundant = false;

                foreach (ImageMapping mapping in m_images)
                {
                    if (mapping.image.sprite == sprite)
                    {
                        mapping.controls.Add(new Control(control, schemes));
                        redundant = true;
                        break;
                    }
                }

                if (redundant)
                {
                    continue;
                }

                // if this sprite is distinct create an image to display it
                GameObject go = UIHelper.Create(transform).gameObject;
                go.AddComponent<CanvasRenderer>();

                Image image = go.AddComponent<Image>();
                image.sprite = sprite;
                image.preserveAspect = true;

                LayoutElement layout = go.AddComponent<LayoutElement>();

                m_images.Add(new ImageMapping(image, new Control(control, schemes)));
            }
        }

        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }

        public void UpdateUI(string descrption, Sprite sprite)
        {
            UpdateUI(descrption, new List<Sprite>());
        }

        public void UpdateUI(string descrption, List<Sprite> sprites)
        {
        }

        private void LateUpdate()
        {
            if (Action == null)
            {
                return;
            }

            // determine which input image need to be shown
            foreach (ImageMapping mapping in m_images)
            {
                bool showImage = false;

                foreach (Control control in mapping.controls)
                {
                    bool active = control.IsCurrent();
                    bool showScheme = SchemeOverride == default || control.HasScheme(SchemeOverride);
                    bool showDevice = DeviceOverride == null || control.IsDevice(DeviceOverride);

                    if (active && showScheme && showDevice)
                    {
                        showImage = true;
                    }
                }

                if (showImage)
                {
                    LayoutElement layout = mapping.image.GetComponent<LayoutElement>();
                    Sprite sprite = mapping.image.sprite;

                    layout.preferredHeight = GetComponent<RectTransform>().rect.height;
                    layout.preferredWidth = sprite.rect.width * (layout.preferredHeight / sprite.rect.height);
                }

                mapping.image.gameObject.SetActive(showImage);
            }
        }

        private static Sprite GetSprite(InputControl control)
        {
            if (control == null)
            {
                return null;
            }

            // The first part of the path includes the actual device name.
            // We want to check if there is a specific image to use for this
            // device first (ie XInputControllerWindows), then fall back to the
            // general device class (ie Gamepad).

            // remove the leading slash
            string path = control.path.Substring(1);

            Sprite sprite = Resources.Load<Sprite>(path);

            if (sprite == null)
            {
                // The device class seems to be null for at least some gamepads.
                // We should try to find out what device class is suitable ourselves
                // in these cases.
                string deviceClass = control.device.description.deviceClass;

                if (string.IsNullOrEmpty(deviceClass))
                {
                    foreach (Gamepad gamepad in Gamepad.all)
                    {
                        if (gamepad.deviceId == control.device.deviceId)
                        {
                            deviceClass = "gamepad";
                            break;
                        }
                    }
                }

                // remove the device name
                string controlPath = path.Substring(path.IndexOf('/'));
                string backupPath = $"{deviceClass}{controlPath}";
                sprite = Resources.Load<Sprite>(backupPath);
            }

            if (sprite == null)
            {
                Debug.LogError($"Unable to find sprite for control \"{path}\"");
            }

            return sprite;
        }
    }
}
