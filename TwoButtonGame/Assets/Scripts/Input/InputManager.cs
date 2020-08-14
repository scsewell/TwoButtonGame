using System.Collections.Generic;

using UnityEngine;

using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

namespace BoostBlasters.Input
{
    public class InputManager : MonoBehaviour
    {
        private static readonly List<UserInput> m_userInputs = new List<UserInput>();

        /// <summary>
        /// The global input source.
        /// </summary>
        public static GlobalInput GlobalInput { get; private set; }

        /// <summary>
        /// The inputs for all joined players.
        /// </summary>
        public static IReadOnlyList<UserInput> UserInputs => m_userInputs;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Init()
        {
            m_userInputs.Clear();
            GlobalInput = null;
        }


        ///// <summary>
        ///// An event invoked when a new input is enabled.
        ///// </summary>
        //public static event Action<BaseInput> InputAdded;

        ///// <summary>
        ///// An event invoked when an input is destroyed.
        ///// </summary>
        //public static event Action<BaseInput> InputRemoved;

        //[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        //private static void Init()
        //{
        //    InputAdded = null;
        //    InputRemoved = null;
        //}

        internal void AddInput(BaseInput input)
        {
            switch (input)
            {
                case UserInput userInput:
                    m_userInputs.Add(userInput);
                    break;
                case GlobalInput globalInput:
                    GlobalInput = globalInput;
                    break;
            }
        }

        internal void RemoveInput(BaseInput input)
        {
            switch (input)
            {
                case UserInput userInput:
                    m_userInputs.Remove(userInput);
                    break;
                case GlobalInput globalInput:
                    GlobalInput = null;
                    break;
            }
        }
    }
}
