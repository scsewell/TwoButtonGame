using System;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BoostBlasters.UI
{
    /// <summary>
    /// A UI widget that displays an icon.
    /// </summary>
    public class Tab : MonoBehaviour, ISelectHandler
    {
        [SerializeField]
        [Tooltip("The image used to show the tab icon.")]
        private Image m_image = null;

        /// <summary>
        /// The icon shown in the tab.
        /// </summary>
        public Sprite Icon
        {
            get => m_image.sprite;
            set => m_image.sprite = value;
        }

        /// <summary>
        /// The event invoked when the tab is selected.
        /// </summary>
        public event Action Selected;

        public void OnSelect(BaseEventData eventData)
        {
            Selected?.Invoke();
        }
    }
}
