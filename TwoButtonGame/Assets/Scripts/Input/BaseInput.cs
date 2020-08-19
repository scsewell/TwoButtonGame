using UnityEngine;
using UnityEngine.InputSystem.UI;

namespace BoostBlasters.Input
{
    /// <summary>
    /// A class that owns a set of action maps used for game input.
    /// </summary>
    public class BaseInput : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The selection used for primary navigation and submit/cancel events.")]
        protected InputSystemUIInputModule m_primaryInput;

        [SerializeField]
        [Tooltip("The input modeule used for secondary navigation events (ex. L/R bumpers).")]
        protected InputSystemUIInputModule m_secondaryInput;


        /// <summary>
        /// The event system used to generate primary navigation events used by the UI.
        /// </summary>
        public MultiplayerEventSystem PrimaryEventSystem { get; private set; }

        /// <summary>
        /// The event system used to generate secondary navigation events used by the UI.
        /// </summary>
        /// <remarks>
        /// This is used for meus that have multiple selected elements, such as menu tabs
        /// that the user can switch between indepentantly of the main selection.
        /// </remarks>
        public MultiplayerEventSystem SecondaryEventSystem { get; private set; }

        /// <summary>
        /// The actions owned by this input source.
        /// </summary>
        public InputActions Actions { get; private set; }


        protected virtual void Awake()
        {
            Actions = new InputActions();

            m_primaryInput.actionsAsset = Actions.asset;
            m_secondaryInput.actionsAsset = Actions.asset;

            PrimaryEventSystem = m_primaryInput.GetComponent<MultiplayerEventSystem>();
            SecondaryEventSystem = m_secondaryInput.GetComponent<MultiplayerEventSystem>();
        }

        protected virtual void Start()
        {
            var manager = GetComponentInParent<InputManager>();
            if (manager != null)
            {
                manager.RegisterInput(this);
            }
        }

        protected virtual void OnDestroy()
        {
            // Deselect UI stuff that is currently selected, or else they
            // will still behave as if they are selected.
            PrimaryEventSystem.SetSelectedGameObject(null);
            SecondaryEventSystem.SetSelectedGameObject(null);

            var manager = GetComponentInParent<InputManager>();
            if (manager != null)
            {
                manager.DeregisterInput(this);
            }

            Actions.Dispose();
            Actions = null;
        }
    }
}
