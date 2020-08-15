using System;
using System.Collections.Generic;
using System.Linq;

using Framework.UI;

using TMPro;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace BoostBlasters.UI
{
    /// <summary>
    /// A UI widget that displays a control prompt.
    /// </summary>
    public class Control : MonoBehaviour
    {
        private static readonly Dictionary<string, Sprite> s_pathToSprite = new Dictionary<string, Sprite>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Init()
        {
            s_pathToSprite.Clear();
        }


        [Header("UI Elements")]

        [SerializeField]
        [Tooltip("The text element used to display the control name.")]
        private TMP_Text m_description = null;

        [SerializeField]
        [Tooltip("The transform the control sprites are parented under.")]
        private RectTransform m_content = null;

        [Header("Options")]

        [SerializeField]
        [Tooltip("The action to show the controls for.")]
        private InputActionReference m_defaultAction = null;

        /// <summary>
        /// The filters that determines which controls are shown for the assigned action. 
        /// </summary>
        public enum FilterMode
        {
            /// <summary>
            /// Show every control for the assigned action.
            /// </summary>
            None,
            /// <summary>
            /// Only show controls for a single control scheme (by default the last scheme that gave input).
            /// </summary>
            ControlScheme,
        }

        [SerializeField]
        [Tooltip("The filter that determines which controls are shown for the assigned action. " +
            "Use \"None\" to show every control for the action. " +
            "Use \"ControlScheme\" to only show controls for a single control scheme (by default the last scheme that gave input).")]
        private FilterMode m_filter = FilterMode.ControlScheme;

        /// <summary>
        /// The filter that determines which controls are shown for the assigned action.
        /// </summary>
        public FilterMode Filter
        {
            get => m_filter;
            set => m_filter = value;
        }


        private class ControlInfo
        {
            private readonly List<InputControlScheme> m_schemes;

            public ControlInfo(List<InputControlScheme> schemes)
            {
                m_schemes = schemes;
            }

            public bool UsesScheme(InputControlScheme scheme)
            {
                for (var i = 0; i < m_schemes.Count; i++)
                {
                    if (m_schemes[i] == scheme)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        private InputAction m_action = null;
        private InputActionMap m_map = null;
        private InputControlScheme? m_currentScheme = null;
        private readonly List<Image> m_images = new List<Image>();
        private readonly Dictionary<Sprite, List<ControlInfo>> m_spriteToControls = new Dictionary<Sprite, List<ControlInfo>>();


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
        /// Overrides the control scheme to filter by when using the
        /// <see cref="FilterMode.ControlScheme"/> filter.
        /// </summary>
        /// <remarks>
        /// If null, the input scheme that last gave input is used.
        /// </remarks>
        public InputControlScheme? SchemeOverride { get; set; } = null;


        private void Awake()
        {
            m_action = m_defaultAction;
        }

        private void OnEnable()
        {
            Initialize();

            InputSystem.onDeviceChange += OnDeviceChanged;
        }

        private void OnDisable()
        {
            InputSystem.onDeviceChange -= OnDeviceChanged;

            SetActionMap(null);
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
            m_spriteToControls.Clear();

            if (m_action == null)
            {
                SetActionMap(null);
            }
            else
            {
                SetActionMap(m_action.actionMap);

                // display the action name
                if (m_description != null)
                {
                    m_description.text = m_action.name;
                }

                // find the set of unique sprites needed to represent the controls
                for (var i = 0; i < m_action.controls.Count; i++)
                {
                    var control = m_action.controls[i];
                    var sprite = GetSprite(control);

                    if (!m_spriteToControls.TryGetValue(sprite, out var controls))
                    {
                        controls = new List<ControlInfo>();
                        m_spriteToControls.Add(sprite, controls);
                    }

                    var schemes = new List<InputControlScheme>();
                    m_action.TryGetControlSchemes(control, schemes);

                    controls.Add(new ControlInfo(schemes));
                }
            }

            // configure the image to show the required sprites
            var imageIndex = 0;

            foreach (var spriteControls in m_spriteToControls)
            {
                // if this sprite is distinct create an image to display it
                Image image;
                if (imageIndex < m_images.Count)
                {
                    image = m_images[imageIndex];
                }
                else
                {
                    var go = UIHelper.Create(m_content).gameObject;

                    go.AddComponent<LayoutElement>();
                    go.AddComponent<CanvasRenderer>();
                    image = go.AddComponent<Image>();

                    m_images.Add(image);
                }

                var sprite = spriteControls.Key;

                image.name = sprite.name;
                image.sprite = sprite;
                image.preserveAspect = true;

                imageIndex++;
            }
        }

        /// <summary>
        /// Sets if this control panel is visible.
        /// </summary>
        /// <param name="active">True to enable the control panel, false to disable.</param>
        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }

        public void UpdateUI(string descrption, List<Sprite> sprites)
        {
        }

        private void LateUpdate()
        {
            if (m_action == null)
            {
                return;
            }

            // only show images that correspond to an active control
            for (var i = 0; i < m_images.Count; i++)
            {
                var image = m_images[i];
                var sprite = image.sprite;

                var showImage = false;

                if (sprite != null && m_spriteToControls.TryGetValue(sprite, out var controls))
                {
                    foreach (var control in controls)
                    {
                        bool active;
                        switch (m_filter)
                        {
                            case FilterMode.None:
                            {
                                active = true;
                                break;
                            }
                            case FilterMode.ControlScheme:
                            {
                                if (SchemeOverride.HasValue)
                                {
                                    active = control.UsesScheme(SchemeOverride.Value);
                                }
                                else if (m_currentScheme.HasValue)
                                {
                                    active = control.UsesScheme(m_currentScheme.Value);
                                }
                                else
                                {
                                    active = false;
                                }
                                break;
                            }
                            default:
                                throw new NotImplementedException();
                        }

                        if (active)
                        {
                            showImage = true;
                            break;
                        }
                    }
                }

                if (showImage)
                {
                    var layout = image.GetComponent<LayoutElement>();

                    layout.preferredHeight = GetComponent<RectTransform>().rect.height;
                    layout.preferredWidth = layout.preferredHeight * (sprite.rect.width / sprite.rect.height);
                }

                image.gameObject.SetActive(showImage);
            }
        }

        private void SetActionMap(InputActionMap map)
        {
            if (m_map != map)
            {
                if (m_map != null)
                {
                    var actions = m_map.actions;

                    for (var i = 0; i < actions.Count; i++)
                    {
                        actions[i].performed -= OnActionPerformed;
                    }
                }

                m_map = map;
                m_currentScheme = null;

                if (m_map != null)
                {
                    var actions = m_map.actions;

                    for (var i = 0; i < actions.Count; i++)
                    {
                        actions[i].performed += OnActionPerformed;
                    }

                    if (m_map.controlSchemes.Count > 0)
                    {
                        m_currentScheme = m_map.controlSchemes[0];
                    }
                }
            }
        }

        private void OnActionPerformed(InputAction.CallbackContext context)
        {
            var control = context.control;

            if (control.noisy || control.synthetic)
            {
                return;
            }

            if (context.action.TryGetControlScheme(control, out var scheme))
            {
                m_currentScheme = scheme;
            }
        }

        private static Sprite GetSprite(InputControl control)
        {
            if (control == null)
            {
                return null;
            }

            // check if we have cached the sprite for this control
            if (s_pathToSprite.TryGetValue(control.path, out var sprite))
            {
                return sprite;
            }

            // The first part of the path includes the actual device name.
            // We want to check if there is a specific image to use for this
            // device first (ie XInputControllerWindows), then fall back to the
            // general device class (ie Gamepad).

            // there is a leading slash we should remove
            var path = control.path.Substring(1);

            // we have one sprite sheet per device class
            var sheet = path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();

            // the item will be the path without the device and the slashes replaced as dashes
            var item = path.Substring(path.IndexOf('/') + 1).Replace('/', '-');

            sprite = GetSprite(sheet, item);

            if (sprite == null)
            {
                // try the loading the sprite using the appropriate class of device
                sheet = control.device.description.deviceClass;

                // The device class seems to be null for at least some gamepads.
                // We should try to find out what device class is suitable ourselves
                // in these cases.
                if (string.IsNullOrEmpty(sheet))
                {
                    switch (control.device)
                    {
                        case Gamepad gamepad:
                            sheet = nameof(Gamepad);
                            break;
                    }
                }

                sprite = GetSprite(sheet, item);
            }

            if (sprite == null)
            {
                Debug.LogError($"Unable to find sprite for control \"{path}\"!");
            }

            s_pathToSprite.Add(control.path, sprite);
            return sprite;
        }

        private static Sprite GetSprite(string spriteSheet, string itemName)
        {
            var sprites = Resources.LoadAll<Sprite>(spriteSheet);

            if (sprites != null && sprites.Length > 0)
            {
                foreach (var sprite in sprites)
                {
                    if (sprite.name == itemName)
                    {
                        return sprite;
                    }
                }

                Debug.LogError($"Unable to find control sprite \"{itemName}\" in sheet \"{spriteSheet}\"!");
            }

            return null;
        }
    }
}
