using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace BoostBlasters.MainMenus
{
    public class PlayerProfilePanel : MonoBehaviour, IMoveHandler
    {
        private Text m_name;
        private Button m_button;

        private ProfilesMenu m_menu;
        private bool m_addNew;

        private PlayerProfile m_profile;
        public PlayerProfile Profile { get { return m_profile; } }

        private void Awake()
        {
            m_name = GetComponentInChildren<Text>();
            m_button = GetComponentInChildren<Button>();
            m_button.onClick.AddListener(() => OnClick());

            m_menu = GetComponentInParent<ProfilesMenu>();

            m_name.alignment = TextAnchor.MiddleCenter;
        }

        public void SetProfile(PlayerProfile profile, bool isAddNew)
        {
            m_profile = profile;
            m_addNew = isAddNew;

            gameObject.SetActive(m_profile != null || isAddNew);
        }

        public void UpdateGraphics()
        {
            if (m_addNew)
            {
                m_name.fontStyle = FontStyle.Bold;
                UIUtils.FixText(m_name, "Create New");
            }
            else if (m_profile != null)
            {
                m_name.fontStyle = FontStyle.Normal;
                UIUtils.FixText(m_name, m_profile.Name);
            }
        }

        public void OnMove(AxisEventData eventData)
        {
            switch (eventData.moveDir)
            {
                case MoveDirection.Left: m_menu.ViewPreviousPage(); break;
                case MoveDirection.Right: m_menu.ViewNextPage(); break;
            }
        }

        private void OnClick()
        {
            m_menu.OnSelect(this, m_profile, m_addNew);
        }
    }
}
