using System;
using System.Collections.Generic;
using System.Linq;

using BoostBlasters.Input;

using Framework;
using Framework.UI;

using TMPro;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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
        [Tooltip("If the action is a composite, this determines which controls in the composite are shown.")]
        [EnumFlags]
        private CompositeMode m_composite = CompositeMode.All;

        /// <summary>
        /// The different parts of a composite control.
        /// </summary>
        [Flags]
        public enum CompositeMode
        {
            None = 0,

            Positive = (1 << 0),
            Negative = (1 << 1),

            Up = (1 << 2),
            Down = (1 << 3),
            Left = (1 << 4),
            Right = (1 << 5),

            All = ~0,
        }

        [SerializeField]
        [Tooltip("Only show the sprite for the first available control.")]
        private bool m_firstOnly = false;

        [SerializeField]
        [Tooltip("The filter that determines which controls are shown for the assigned action. " +
            "Use \"None\" to show every control for the action. " +
            "Use \"ControlScheme\" to only show controls for the last used control scheme, or the schemes set in the input overrides.")]
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
            /// Only show controls for the last used control scheme, or the schemes set in the input overrides.
            /// </summary>
            ControlScheme,
        }

        private class ControlInfo
        {
            private readonly HashSet<string> m_schemes;
            private readonly CompositeMode m_composite;

            public InputBinding? CompositeParent { get; }

            public ControlInfo(
                CompositeMode composite,
                InputBinding? compositeParent,
                List<InputControlScheme> schemes)
            {
                m_schemes = new HashSet<string>(schemes.Select(s => s.name));
                m_composite = composite;
                CompositeParent = compositeParent;
            }

            public bool FilterComposite(CompositeMode mode)
            {
                return m_composite == CompositeMode.None || mode.Contains(m_composite);
            }

            public bool UsesAnyScheme(InputControlScheme[] schemes)
            {
                for (var i = 0; i < schemes.Length; i++)
                {
                    if (m_schemes.Contains(schemes[i].name))
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
            private float m_aspect;

            public List<ControlInfo> Controls { get; private set; }

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

                Controls = controls;
            }

            public void Disable()
            {
                m_image.name = "Unused";
                m_image.sprite = null;
                m_image.gameObject.SetActive(false);

                Controls = null;
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
        private UserInput[] m_inputOverrides = null;
        private InputControlScheme[] m_currentSchemes = null;
        private InputActionAsset m_actionAsset = null;
        private bool m_isDirty;

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
                    m_isDirty = true;
                }
            }
        }

        /// <summary>
        /// If the action is a composite, this determines which controls in the composite are shown.
        /// </summary>
        public CompositeMode Composite
        {
            get => m_composite;
            set
            {
                if (m_composite != value)
                {
                    m_composite = value;
                    m_isDirty = true;
                }
            }
        }

        /// <summary>
        /// Only show the sprite for the first available control.
        /// </summary>
        public bool FirstOnly
        {
            get => m_firstOnly;
            set => m_firstOnly = value;
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
        public UserInput[] InputOverrides
        {
            get => m_inputOverrides;
            set
            {
                if (m_inputOverrides != value)
                {
                    m_inputOverrides = value;

                    if (value != null)
                    {
                        if (m_menu != null)
                        {
                            m_menu.InputChanged -= OnMenuInputChanged;
                        }

                        SetActionAsset(null);

                        m_currentSchemes = m_inputOverrides
                            .Select(u => u.ControlScheme)
                            .ToArray();
                    }
                    else
                    {
                        if (m_menu != null)
                        {
                            m_menu.InputChanged += OnMenuInputChanged;
                            OnMenuInputChanged(m_menu.Input);
                        }
                        else
                        {
                            OnMenuInputChanged(InputManager.Global);
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
                m_menu.InputChanged += OnMenuInputChanged;
            }

            // TODO: REVIEW should global input scheme always be tracked? might catch a few issues...
            // The last control scheme will not correct until actuated, since we only listen to actions after awake

            // Don't use the menu input to start or else the default control scheme will not be initialzied
            // until after the screen has been opened.
            OnMenuInputChanged(InputManager.Global);
        }

        private void OnDestroy()
        {
            if (m_menu != null)
            {
                m_menu.InputChanged -= OnMenuInputChanged;
            }
        }

        private void OnEnable()
        {
            InputSystem.onDeviceChange += OnDeviceChanged;

            m_isDirty = true;
        }

        private void OnDisable()
        {
            InputSystem.onDeviceChange -= OnDeviceChanged;
        }

        private void OnMenuInputChanged(BaseInput input)
        {
            switch (input)
            {
                case UserInput userInput:
                {
                    // When a user is assigned, we know that the control scheme
                    // never changes from the one paried to the user.
                    SetActionAsset(null);

                    m_currentSchemes = new[]
                    {
                        userInput.ControlScheme,
                    };
                    break;
                }
                case GlobalInput globalInput:
                {
                    // Listen for all actions and set the current control scheme
                    // to be the one used to perform the most recent action.
                    var asset = globalInput.Actions.asset;
                    SetActionAsset(asset);

                    if (m_currentSchemes == null)
                    {
                        m_currentSchemes = new[]
                        {
                            asset.controlSchemes[0],
                        };
                    }
                    else if (m_currentSchemes.Length != 1)
                    {
                        Array.Resize(ref m_currentSchemes, 1);
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
                    m_isDirty = true;
                    break;
            }
        }

        private void LateUpdate()
        {
            var action = m_action != null ? m_action.action : null;

            // create the images for all controls that may be shown for the current action
            if (m_isDirty)
            {
                Initialize(action);
                m_isDirty = false;
            }

            // set which images are visible based on the current options and control states
            var foundFirst = false;
            var firstIsComposite = false;
            var compositeParent = default(InputBinding);

            foreach (var image in m_images)
            {
                var visible = false;

                if (action != null && image.Controls != null)
                {
                    foreach (var control in image.Controls)
                    {
                        // if the first shown control is a composite, only show the parts of that composite
                        if (m_firstOnly && foundFirst && (!firstIsComposite || !(control.CompositeParent.HasValue && control.CompositeParent.Value == compositeParent)))
                        {
                            continue;
                        }

                        // only show controls for parts of a composite that we want to show
                        if (!control.FilterComposite(m_composite))
                        {
                            continue;
                        }

                        switch (m_filter)
                        {
                            case FilterMode.ControlScheme:
                            {
                                if (!control.UsesAnyScheme(m_currentSchemes))
                                {
                                    continue;
                                }
                                break;
                            }
                        }

                        visible = true;

                        if (m_firstOnly && !foundFirst)
                        {
                            if (control.CompositeParent.HasValue)
                            {
                                firstIsComposite = true;
                                compositeParent = control.CompositeParent.Value;
                            }
                            foundFirst = true;
                        }

                        break;
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

        private void Initialize(InputAction action)
        {
            var spriteToControls = new Dictionary<Sprite, List<ControlInfo>>();

            if (action != null)
            {
                // display the action name
                if (m_description != null)
                {
                    m_description.text = action.name;
                }

                // find the set of unique sprites needed to represent the available controls
                foreach (var control in action.controls)
                {
                    if (!action.TryGetControlBinding(control, out var binding))
                    {
                        continue;
                    }

                    var path = GetPath(action, binding, control);
                    var sprite = GetSprite(path, control.device);

                    if (sprite == null)
                    {
                        continue;
                    }

                    if (!spriteToControls.TryGetValue(sprite, out var controls))
                    {
                        controls = new List<ControlInfo>();
                        spriteToControls.Add(sprite, controls);
                    }

                    var composite = GetComposite(binding);
                    var compositeParent = action.GetCompositeFromPart(binding);

                    var schemes = new List<InputControlScheme>();
                    action.TryGetControlSchemes(control, schemes);

                    controls.Add(new ControlInfo(composite, compositeParent, schemes));
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

            if (context.action.TryGetControlScheme(control, out var scheme) && scheme.name != "Global")
            {
                m_currentSchemes[0] = scheme;
            }
        }

        private static CompositeMode GetComposite(InputBinding binding)
        {
            if (binding.isPartOfComposite)
            {
                switch (binding.name)
                {
                    case "positive":
                        return CompositeMode.Positive;
                    case "negative":
                        return CompositeMode.Negative;
                    case "up":
                        return CompositeMode.Up;
                    case "down":
                        return CompositeMode.Down;
                    case "left":
                        return CompositeMode.Left;
                    case "right":
                        return CompositeMode.Right;
                }
            }
            return CompositeMode.None;
        }

        private string GetPath(InputAction action, InputBinding binding, InputControl control)
        {
            var path = control.path;

            if (action.type == InputActionType.Value && !binding.isPartOfComposite)
            {
                switch (action.expectedControlType)
                {
                    case "Dpad":
                    case "Stick":
                    case "Vector2":
                    {
                        switch (m_composite)
                        {
                            case CompositeMode.Up | CompositeMode.Down:
                                path += "-y";
                                break;
                            case CompositeMode.Left | CompositeMode.Right:
                                path += "-x";
                                break;
                            case CompositeMode.Up:
                                path += "-up";
                                break;
                            case CompositeMode.Down:
                                path += "-down";
                                break;
                            case CompositeMode.Left:
                                path += "-left";
                                break;
                            case CompositeMode.Right:
                                path += "-right";
                                break;
                        }
                        break;
                    }
                }
            }

            return path;
        }

        private static Sprite GetSprite(string controlPath, InputDevice device)
        {
            // check if we have cached the sprite for this control
            if (s_pathToSprite.TryGetValue(controlPath, out var sprite))
            {
                return sprite;
            }

            // The first part of the path includes the actual device name.
            // We want to check if there is a specific image to use for this
            // device first (ie XInputControllerWindows), then fall back to the
            // general device class (ie Gamepad).

            // there is a leading slash we should remove
            var path = controlPath.Substring(1);

            // we have one sprite sheet per device class
            var sheet = path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();

            // the item will be the path without the device and the slashes replaced as dashes
            var item = path.Substring(path.IndexOf('/') + 1).Replace('/', '-');

            sprite = GetSprite(sheet, item);

            if (sprite == null)
            {
                // try the loading the sprite using the appropriate class of device
                sheet = device.description.deviceClass;

                // The device class seems to be null for at least some gamepads.
                // We should try to find out what device class is suitable ourselves
                // in these cases.
                if (string.IsNullOrEmpty(sheet))
                {
                    switch (device)
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

            s_pathToSprite.Add(controlPath, sprite);
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
