using UnityEngine;
using UnityEngine.InputSystem.UI;

namespace BoostBlasters.Input
{
    /// <summary>
    /// The base class for input sources that can be used by the UI.
    /// </summary>
    public class BaseInput : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The selection used for primary navigation and submit/cancel events.")]
        protected InputSystemUIInputModule m_primaryInput;

        [SerializeField]
        [Tooltip("The input modeule used for secondary navigation events (ex. L/R bumpers).")]
        protected InputSystemUIInputModule m_secondaryInput;


        /// <inheritdoc/>
        public MultiplayerEventSystem PrimaryEventSystem { get; private set; }
        /// <inheritdoc/>
        public MultiplayerEventSystem SecondaryEventSystem { get; private set; }


        protected virtual void OnEnable()
        {
            PrimaryEventSystem = m_primaryInput.GetComponent<MultiplayerEventSystem>();
            SecondaryEventSystem = m_secondaryInput.GetComponent<MultiplayerEventSystem>();

            var manager = GetComponentInParent<InputManager>();
            if (manager != null)
            {
                manager.AddInput(this);
            }
        }

        protected virtual void OnDisable()
        {
            // Deselect UI stuff that is currently selected, or else they
            // will still behave as if they are selected.
            PrimaryEventSystem.SetSelectedGameObject(null);
            SecondaryEventSystem.SetSelectedGameObject(null);

            var manager = GetComponentInParent<InputManager>();
            if (manager != null)
            {
                manager.RemoveInput(this);
            }
        }
    }
}
