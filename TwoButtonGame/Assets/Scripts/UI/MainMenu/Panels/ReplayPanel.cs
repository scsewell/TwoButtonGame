using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace BoostBlasters.MainMenus
{
    public class ReplayPanel : MonoBehaviour, IMoveHandler
    {
        private Text m_name;
        private Button m_button;

        private ReplayMenu m_menu;

        private ReplayInfo m_info;
        public ReplayInfo ReplayInfo { get { return m_info; } }

        private void Awake()
        {
            m_name = GetComponentInChildren<Text>();
            m_button = GetComponentInChildren<Button>();
            m_button.onClick.AddListener(() => OnClick());

            m_menu = GetComponentInParent<ReplayMenu>();

            m_name.alignment = TextAnchor.MiddleLeft;
        }

        public void SetRecording(ReplayInfo info)
        {
            m_info = info;

            gameObject.SetActive(m_info != null);
        }

        public void UpdateGraphics()
        {
            if (m_info != null)
            {
                UIUtils.FixText(m_name, m_info.Name);
            }
        }

        public void OnMove(AxisEventData eventData)
        {
            switch (eventData.moveDir)
            {
                case MoveDirection.Left: m_menu.ViewPreviousReplays(); break;
                case MoveDirection.Right: m_menu.ViewNextReplays(); break;
            }
        }

        private void OnClick()
        {
            GetComponentInParent<MainMenu>().LaunchReplay(m_info);
        }
    }
}
