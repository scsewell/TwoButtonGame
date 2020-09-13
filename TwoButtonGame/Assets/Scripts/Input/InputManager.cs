using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;

namespace BoostBlasters.Input
{
    /// <summary>
    /// A class responsible for managing player input.
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        private static readonly List<InputControl> s_joinCandidates = new List<InputControl>();
        private static readonly List<BaseInput> s_all = new List<BaseInput>();
        private static readonly List<UserInput> s_users = new List<UserInput>();
        private static BaseInput s_solo;
        private static InputActionReference s_joinAction;
        private static int s_listenForJoiningUsers;

        /// <summary>
        /// All input sources registered to the manager.
        /// </summary>
        public static IReadOnlyList<BaseInput> All => s_all;

        /// <summary>
        /// The global input source whose actions are performed using any enabled device.
        /// </summary>
        public static GlobalInput Global { get; private set; }

        /// <summary>
        /// The input sources whose actions are performed by a specific player.
        /// </summary>
        public static IReadOnlyList<UserInput> Users => s_users;

        /// <summary>
        /// An event invoked when a new user input is enabled.
        /// </summary>
        public static event Action<UserInput> UserAdded;

        /// <summary>
        /// An event invoked when a user input is destroyed.
        /// </summary>
        public static event Action<UserInput> UserRemoved;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Init()
        {
            s_joinCandidates.Clear();
            s_joinAction = null;
            s_listenForJoiningUsers = 0;
            InputSystem.onEvent -= OnEvent;
            InputSystem.onAfterUpdate -= OnAfterUpdate;

            s_all.Clear();
            s_users.Clear();
            Global = null;
            s_solo = null;

            UserAdded = null;
            UserRemoved = null;
        }


        [SerializeField]
        [Tooltip("The action which must be performed by an unpaired device to add a new user.")]
        private InputActionReference m_joinAction = null;

        private void OnEnable()
        {
            s_joinAction = m_joinAction;
        }


        /// <summary>
        /// Will the input system add a user if a new deivce is used.
        /// </summary>
        /// <remarks>
        /// This is an integer rather than a bool to allow multiple systems to concurrently listen for unpaired
        /// device activity without treading on each other when enabling/disabling the code path.
        /// </remarks>
        public static int ListenForJoiningUsers
        {
            get => s_listenForJoiningUsers;
            set
            {
                var v = Mathf.Max(0, value);

                if (s_listenForJoiningUsers != v)
                {
                    s_listenForJoiningUsers = v;

                    if (value > 0)
                    {
                        EnableJoining();
                    }
                    else
                    {
                        DisableJoining();
                    }
                }
            }
        }

        /// <summary>
        /// When not null, the only enabled input.
        /// </summary>
        /// <remarks>
        /// Assiging an input will disable all other inputs until set back to null.
        /// </remarks>
        public static BaseInput Solo
        {
            get => s_solo;
            set
            {
                if (s_solo != value)
                {
                    if (s_solo != null)
                    {
                        foreach (var input in All)
                        {
                            input.Actions.Enable();
                        }
                    }

                    s_solo = value;

                    if (s_solo != null)
                    {
                        foreach (var input in All)
                        {
                            if (input != value)
                            {
                                input.Actions.Disable();
                            }
                        }
                    }
                }
            }
        }

        internal void RegisterInput(BaseInput input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            s_all.Add(input);

            switch (input)
            {
                case UserInput userInput:
                {
                    s_users.Add(userInput);
                    UserAdded?.Invoke(userInput);
                    break;
                }
                case GlobalInput globalInput:
                {
                    Global = globalInput;
                    break;
                }
            }
        }

        internal void DeregisterInput(BaseInput input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            s_all.Remove(input);

            switch (input)
            {
                case UserInput userInput:
                {
                    s_users.Remove(userInput);
                    UserRemoved?.Invoke(userInput);
                    break;
                }
                case GlobalInput globalInput:
                {
                    Global = null;
                    break;
                }
            }
        }

        private static void EnableJoining()
        {
            if (s_joinAction == null)
            {
                throw new InvalidOperationException("Can't pair devices until a join action has been assigned.");
            }

            if (PlayerInputManager.instance != null)
            {
                PlayerInputManager.instance.joinBehavior = PlayerJoinBehavior.JoinPlayersManually;
                PlayerInputManager.instance.EnableJoining();
            }
            else
            {
                Debug.LogError($"{nameof(PlayerInputManager)} is null!");
            }

            InputSystem.onEvent += OnEvent;
            InputSystem.onAfterUpdate += OnAfterUpdate;
        }

        private static void DisableJoining()
        {
            if (PlayerInputManager.instance != null)
            {
                PlayerInputManager.instance.DisableJoining();
            }

            InputSystem.onEvent -= OnEvent;
            InputSystem.onAfterUpdate -= OnAfterUpdate;
        }

        private static unsafe void OnEvent(InputEventPtr eventPtr, InputDevice device)
        {
            Debug.Assert(s_listenForJoiningUsers != 0, "This should only be called while listening for joining users!");

            // onEvent occurs before all the control state has been processed and updated,
            // so we find controls we might want to join using and queue them to be checked
            // after the input system has updated.

            if (!eventPtr.IsA<StateEvent>() && !eventPtr.IsA<DeltaStateEvent>())
            {
                return;
            }

            var controls = device.allControls;
            for (var i = 0; i < controls.Count; ++i)
            {
                var control = controls[i];

                if (control.noisy || control.synthetic)
                {
                    continue;
                }

                // ignore non-leaf controls
                if (control.children.Count > 0)
                {
                    continue;
                }

                // ignore controls that aren't part of the event
                var statePtr = control.GetStatePtrFromStateEvent(eventPtr);
                if (statePtr == null)
                {
                    continue;
                }

                // Check for default state. Cheaper check than magnitude evaluation
                // which may involve several virtual method calls.
                if (control.CheckStateIsAtDefault(statePtr))
                {
                    continue;
                }

                // We already know the control has moved away from its default state
                // so in case it does not support magnitudes, we assume that the
                // control has changed value, too.
                var magnitude = control.EvaluateMagnitude(statePtr);
                if (magnitude > 0 || magnitude == -1)
                {
                    if (control is ButtonControl)
                    {
                        s_joinCandidates.Add(control);
                    }
                }
            }
        }

        private static void OnAfterUpdate()
        {
            Debug.Assert(s_listenForJoiningUsers != 0, "This should only be called while listening for joining users!");

            for (var i = 0; i < s_joinCandidates.Count; i++)
            {
                var control = s_joinCandidates[i];

                // only consider buttons that were just pressed
                if (!(control is ButtonControl button && button.wasPressedThisFrame))
                {
                    continue;
                }

                // we need a control scheme to pair
                if (!s_joinAction.action.TryGetControlScheme(control, out var scheme))
                {
                    continue;
                }

                // do not consider a device/control scheme combo already in use
                var alreadyUsed = false;

                foreach (var user in Users)
                {
                    if (user.Matches(control.device, scheme))
                    {
                        alreadyUsed = true;
                        break;
                    }
                }

                if (alreadyUsed)
                {
                    continue;
                }

                // create a new player input paired to this device and control scheme
                PlayerInputManager.instance.JoinPlayer(-1, -1, scheme.name, control.device);
                break;
            }

            s_joinCandidates.Clear();
        }
    }
}
