using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using BoostBlasters.Replays;

namespace BoostBlasters.UI.MainMenus
{
    public class ReplayPanel : MonoBehaviour, IMoveHandler
    {
        private Text m_name = null;
        private Button m_button = null;

        private ReplayMenu m_menu = null;

        private ReplayInfo m_info = null;
        public ReplayInfo ReplayInfo => m_info;

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
                UIUtils.FitText(m_name, m_info.Name);
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
