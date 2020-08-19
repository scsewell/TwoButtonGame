using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Users;

namespace BoostBlasters.Input
{
    /// <summary>
    /// A class responsible for managing player input.
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        private static readonly List<InputControl> s_joinCandidates = new List<InputControl>();
        private static readonly List<UserInput> s_users = new List<UserInput>();
        private static InputActionReference s_joinAction;
        private static int s_listenForJoiningUsers;

        /// <summary>
        /// The global input source whose actions are performed using any enabled device.
        /// </summary>
        public static GlobalInput GlobalInput { get; private set; }

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
            InputUser.onUnpairedDeviceUsed -= OnUnpairedDeviceUsed;

            s_users.Clear();
            GlobalInput = null;

            UserAdded = null;
            UserRemoved = null;
        }


        [SerializeField]
        [Tooltip("The action which must be performed by an unpaired device to add a new user.")]
        private InputActionReference m_joinAction;

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

        internal void RegisterInput(BaseInput input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

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
                    GlobalInput = globalInput;
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
                    GlobalInput = null;
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

            PlayerInputManager.instance.joinBehavior = PlayerJoinBehavior.JoinPlayersManually;
            PlayerInputManager.instance.EnableJoining();

            InputUser.listenForUnpairedDeviceActivity++;
            InputUser.onUnpairedDeviceUsed += OnUnpairedDeviceUsed;

            InputSystem.onAfterUpdate += OnAfterUpdate;
        }

        private static void DisableJoining()
        {
            PlayerInputManager.instance.DisableJoining();

            InputUser.listenForUnpairedDeviceActivity--;
            InputUser.onUnpairedDeviceUsed -= OnUnpairedDeviceUsed;

            InputSystem.onAfterUpdate -= OnAfterUpdate;
        }

        private static void OnUnpairedDeviceUsed(InputControl control, InputEventPtr eventPtr)
        {
            // This event is triggered during InputSystem.onEvent, which occurs before all the
            // control state has been processed and updated, so we queue controls to be checked 
            // after the input system has updated.
            if (control is ButtonControl)
            {
                s_joinCandidates.Add(control);
            }
        }

        private static void OnAfterUpdate()
        {
            for (var i = 0; i < s_joinCandidates.Count; i++)
            {
                var control = s_joinCandidates[i];

                // only consider buttons that were just pressed
                if (!(control is ButtonControl button && button.wasPressedThisFrame))
                {
                    continue;
                }

                // pair to the control scheme that the control belongs to
                if (s_joinAction.action.TryGetControlScheme(control, out var scheme))
                {
                    PlayerInputManager.instance.JoinPlayer(-1, -1, scheme.name, control.device);
                    break;
                }
            }

            s_joinCandidates.Clear();
        }
    }
}
