using System;
using System.Collections.Generic;
using System.Linq;

using Framework.UI;

using TMPro;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

using BoostBlasters.Input;

namespace BoostBlasters.UI
{
    /// <summary>
    /// A UI widget that displays a control hint.
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

        [SerializeField]
        [Tooltip("The filter that determines which controls are shown for the assigned action. " +
            "Use \"None\" to show every control for the action. " +
            "Use \"ControlScheme\" to only show controls for a single control scheme (by default the last scheme that gave input).")]
        private FilterMode m_filter = FilterMode.ControlScheme;


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

        private class ControlImage
        {
            private readonly RectTransform m_rect;
            private readonly LayoutElement m_layout;
            private readonly Image m_image;
            private List<ControlInfo> m_controls;
            private float m_aspect;

            public IReadOnlyList<ControlInfo> Controls => m_controls;

            public ControlImage(Transform parent)
            {
                m_rect = UIHelper.Create(parent);
                var go = m_rect.gameObject;

                m_layout = go.AddComponent<LayoutElement>();

                m_image = go.AddComponent<Image>();
                m_image.preserveAspect = true;
            }

            public void Enable(Sprite sprite, List<ControlInfo> controls)
            {
                m_image.name = sprite.name;
                m_image.sprite = sprite;

                m_aspect = sprite.rect.width / sprite.rect.height;

                m_controls = controls;
            }

            public void Disable()
            {
                m_image.name = "Unused";
                m_image.sprite = null;
                m_image.gameObject.SetActive(false);

                m_controls = null;
            }

            public void SetVisible(bool show)
            {
                if (show)
                {
                    var height = m_rect.rect.height;
                    m_layout.preferredHeight = height;
                    m_layout.preferredWidth = height * m_aspect;
                }

                m_image.gameObject.SetActive(show);
            }
        }

        private readonly List<ControlImage> m_images = new List<ControlImage>();
        private MenuScreen m_menu = null;

        private InputActionReference m_action = null;
        private InputActionAsset m_actionAsset = null;
        private InputControlScheme? m_currentScheme = null;
        private BaseInput m_inputOverride = null;

        /// <summary>
        /// The action to show the controls for.
        /// </summary>
        public InputActionReference Action
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
        /// The filter that determines which controls are shown for the assigned action.
        /// </summary>
        public FilterMode Filter
        {
            get => m_filter;
            set => m_filter = value;
        }

        /// <summary>
        /// Overrides the input from which the current control scheme is shown when using the
        /// <see cref="FilterMode.ControlScheme"/> filter.
        /// </summary>
        public BaseInput InputOverride
        {
            get => m_inputOverride;
            set
            {
                if (m_inputOverride != value)
                {
                    m_inputOverride = value;

                    if (value)
                    {
                        if (m_menu != null)
                        {
                            m_menu.InputChanged -= OnInputChanged;
                        }
                        OnInputChanged(value);
                    }
                    else
                    {
                        if (m_menu != null)
                        {
                            m_menu.InputChanged += OnInputChanged;
                            OnInputChanged(m_menu.Input);
                        }
                        else
                        {
                            OnInputChanged(InputManager.Global);
                        }
                    }
                }
            }
        }


        private void Awake()
        {
            m_action = m_defaultAction;
            m_menu = GetComponentInParent<MenuScreen>();

            if (m_menu != null)
            {
                m_menu.InputChanged += OnInputChanged;
            }

            // TODO: REVIEW should global input scheme always be tracked? might catch a few issues...

            // Don't use the menu input to start or else the default control scheme will not be initialzied
            // until after the screen has been opened.
            OnInputChanged(InputManager.Global);
        }

        private void OnDestroy()
        {
            if (m_menu != null)
            {
                m_menu.InputChanged -= OnInputChanged;
            }
        }

        private void OnEnable()
        {
            InputSystem.onDeviceChange += OnDeviceChanged;

            Initialize();
        }

        private void OnDisable()
        {
            InputSystem.onDeviceChange -= OnDeviceChanged;
        }

        private void OnInputChanged(BaseInput input)
        {
            switch (input)
            {
                case UserInput userInput:
                {
                    // When a user is assigned, we know that the control scheme
                    // never changes from the one paried to the user.
                    SetActionAsset(null);
                    m_currentScheme = userInput.ControlScheme;
                    break;
                }
                case GlobalInput globalInput:
                {
                    // Listen for all actions and set the current control scheme
                    // to be the one used to perform the most recent action.
                    var asset = input.Actions.asset;
                    SetActionAsset(asset);

                    var schemes = asset.controlSchemes;
                    if (m_currentScheme == null && schemes.Count > 0)
                    {
                        m_currentScheme = asset.controlSchemes[0];
                    }
                    break;
                }
            }
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
            var spriteToControls = new Dictionary<Sprite, List<ControlInfo>>();

            var action = m_action != null ? m_action.action : null;

            if (action != null)
            {
                // display the action name
                if (m_description != null)
                {
                    m_description.text = action.name;
                }

                // find the set of unique sprites needed to represent the controls
                foreach (var control in action.controls)
                {
                    var sprite = GetSprite(control);

                    if (!spriteToControls.TryGetValue(sprite, out var controls))
                    {
                        controls = new List<ControlInfo>();
                        spriteToControls.Add(sprite, controls);
                    }

                    var schemes = new List<InputControlScheme>();
                    action.TryGetControlSchemes(control, schemes);

                    controls.Add(new ControlInfo(schemes));
                }
            }

            // configure the image to show the required sprites
            var i = 0;
            foreach (var sprite in spriteToControls)
            {
                if (i < m_images.Count)
                {
                    m_images[i].Enable(sprite.Key, sprite.Value);
                }
                else
                {
                    var image = new ControlImage(m_content);
                    image.Enable(sprite.Key, sprite.Value);
                    m_images.Add(image);
                }

                i++;
            }

            // hide unused images
            for (; i < m_images.Count; i++)
            {
                m_images[i].Disable();
            }
        }

        private void LateUpdate()
        {
            var action = m_action != null ? m_action.action : null;

            // only show images that correspond to an active control
            foreach (var image in m_images)
            {
                var visible = false;

                if (image.Controls != null)
                {
                    foreach (var control in image.Controls)
                    {
                        var active = action != null;

                        switch (m_filter)
                        {
                            case FilterMode.ControlScheme:
                            {
                                active &= m_currentScheme.HasValue ? control.UsesScheme(m_currentScheme.Value) : false;
                                break;
                            }
                        }

                        if (active)
                        {
                            visible = true;
                            break;
                        }
                    }
                }

                image.SetVisible(visible);
            }
        }

        /// <summary>
        /// Sets if this control hint is visible.
        /// </summary>
        /// <param name="active">True to enable the control panel, false to disable.</param>
        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }

        private void SetActionAsset(InputActionAsset asset)
        {
            if (m_actionAsset != asset)
            {
                if (m_actionAsset != null)
                {
                    for (var i = 0; i < m_actionAsset.actionMaps.Count; i++)
                    {
                        var map = m_actionAsset.actionMaps[i];

                        for (var j = 0; j < map.actions.Count; j++)
                        {
                            map.actions[j].performed -= OnActionPerformed;
                        }
                    }
                }

                m_actionAsset = asset;

                if (m_actionAsset != null)
                {
                    for (var i = 0; i < m_actionAsset.actionMaps.Count; i++)
                    {
                        var map = m_actionAsset.actionMaps[i];

                        for (var j = 0; j < map.actions.Count; j++)
                        {
                            map.actions[j].performed += OnActionPerformed;
                        }
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
