using System;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BoostBlasters.UI
{
    public class Tab : MonoBehaviour, ISelectHandler
    {
        [SerializeField]
        private Image m_image = null;

        private Action m_onSelect = null;

        /// <summary>
        /// Prepares the tab.
        /// </summary>
        /// <param name="icon">The icon to display for this tab.</param>
        /// <param name="onSelect">The action completed when this tab is selected.</param>
        public void Init(Sprite icon, Action onSelect)
        {
            m_image.sprite = icon;
            m_onSelect = onSelect;
        }

        public void OnSelect(BaseEventData eventData)
        {
            m_onSelect?.Invoke();
        }
    }
}
