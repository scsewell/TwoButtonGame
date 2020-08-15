using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;

namespace BoostBlasters.Input
{
    /// <summary>
    /// A class responsible for managing player input.
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        /// <summary>
        /// The global input source whose actions are performed using any enabled device.
        /// </summary>
        public static bool JoiningEnabled
        {
            get => PlayerInputManager.instance.joiningEnabled;
            set
            {
                if (value)
                {
                    PlayerInputManager.instance.EnableJoining();
                }
                else
                {
                    PlayerInputManager.instance.DisableJoining();
                }
            }
        }

        private static readonly List<UserInput> m_users = new List<UserInput>();

        /// <summary>
        /// The global input source whose actions are performed using any enabled device.
        /// </summary>
        public static GlobalInput GlobalInput { get; private set; }

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
            m_users.Clear();
            GlobalInput = null;

            UserAdded = null;
            UserRemoved = null;
        }


        /// <summary>
        /// The input sources whose actions are performed by a specific player.
        /// </summary>
        public static IReadOnlyList<UserInput> Users => m_users;


        internal void AddInput(BaseInput input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            switch (input)
            {
                case UserInput userInput:
                {
                    m_users.Add(userInput);
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

        internal void RemoveInput(BaseInput input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            switch (input)
            {
                case UserInput userInput:
                {
                    m_users.Remove(userInput);
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
    }
}
