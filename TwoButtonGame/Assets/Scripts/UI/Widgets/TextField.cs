using System;
using System.Collections.Generic;
using System.Reflection;

using Framework;

using TMPro;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace BoostBlasters.UI
{
    /// <summary>
    /// A text field that has extra logic to manage input system interactions.
    /// </summary>
    [RequireComponent(typeof(SoundListener))]
    public class TextField : TMP_InputField, ICancelHandler
    {
        private static Action<BaseEventData> s_OnSelect;
        private static FieldInfo s_WasCanceled;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Init()
        {
            s_OnSelect = null;
            s_WasCanceled = null;
        }


        private SoundListener m_sound;
        private Animator m_animator;
        private MenuScreen m_screen;
        private readonly HashSet<InputDevice> m_disabledDevices = new HashSet<InputDevice>();

        /// <summary>
        /// Focuses the text for editing when this field is selected.
        /// </summary>
        public bool FocusOnSelect { get; set; } = false;

        /// <summary>
        /// Called once the text field has restored keyboard input after editing has
        /// been completed.
        /// </summary>
        public event Action EditEnd;


        protected override void Awake()
        {
            base.Awake();

            m_sound = GetComponent<SoundListener>();
            m_animator = GetComponent<Animator>();
            m_screen = GetComponentInParent<MenuScreen>();
        }

        protected override void OnEnable()
        {
            onSubmit.AddListener(OnSubmitEvent);

            base.OnEnable();
        }

        protected override void OnDisable()
        {
            if (isFocused)
            {
                EndEdit(false);
            }

            base.OnDisable();

            onSubmit.RemoveListener(OnSubmitEvent);
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();

            // When the field is first focused, mute the keyboard input
            // and if there is an on screen keyboard mute all devices, since
            // those devices might actuate the on screen keyboard.
            if (isFocused && m_disabledDevices.Count == 0)
            {
                if (m_animator != null)
                {
                    m_animator.SetBool("Focused", true);
                    m_animator.SetTrigger("Selected");
                }

                foreach (var device in InputSystem.devices)
                {
                    if (device is Keyboard || m_SoftKeyboard != null)
                    {
                        InputSystem.DisableDevice(device);
                        m_disabledDevices.Add(device);
                    }
                }
            }
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            if (isFocused)
            {
                EndEdit(false);
            }

            base.OnDeselect(eventData);
        }

        public override void OnSelect(BaseEventData eventData)
        {
            if (FocusOnSelect)
            {
                base.OnSelect(eventData);
            }
            else
            {
                // Even if we don't want to start editing the text when selected, we still need to do
                // the base selectable selection behaviour. This lets us skip directly to the selectable
                // base implementation while ignoring the InputField implementation.
                if (s_OnSelect == null)
                {
                    var method = typeof(Selectable).GetMethod(nameof(OnSelect), BindingFlags.Public | BindingFlags.Instance);
                    var methodPtr = method.MethodHandle.GetFunctionPointer();
                    s_OnSelect = (Action<BaseEventData>)Activator.CreateInstance(typeof(Action<BaseEventData>), this, methodPtr);
                }

                s_OnSelect(eventData);
            }
        }

        public override void OnUpdateSelected(BaseEventData eventData)
        {
            base.OnUpdateSelected(eventData);

            // We do this so navigation events are still processed
            eventData.Reset();
        }

        public virtual void OnCancel(BaseEventData eventData)
        {
            if (IsActive() && IsInteractable())
            {
                if (isFocused)
                {
                    EndEdit(true);
                }
                else
                {
                    m_screen.Back();
                }
            }
        }

        private void OnSubmitEvent(string text)
        {
            if (isFocused)
            {
                EndEdit(false);
            }
        }

        private async void EndEdit(bool canceled)
        {
            if (canceled)
            {
                if (s_WasCanceled == null)
                {
                    s_WasCanceled = typeof(TMP_InputField).GetField("m_WasCanceled", BindingFlags.NonPublic | BindingFlags.Instance);
                }
                s_WasCanceled.SetValue(this, true);

                m_sound.OnCancel(null);
            }
            else
            {
                m_sound.OnSubmit(null);
            }

            DeactivateInputField();

            if (m_animator != null)
            {
                m_animator.SetBool("Focused", false);
                m_animator.SetTrigger("Selected");
            }

            // Wait before restoring input to prevent any key press events
            // from being picked up redundantly by the InputSystem.
            await CoroutineUtils.Wait(4);

            foreach (var device in m_disabledDevices)
            {
                InputSystem.EnableDevice(device);
            }
            m_disabledDevices.Clear();

            EditEnd?.Invoke();
        }
    }
}
