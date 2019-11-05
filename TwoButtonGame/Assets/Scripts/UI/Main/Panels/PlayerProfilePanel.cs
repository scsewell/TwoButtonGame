﻿using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using BoostBlasters.Players;

namespace BoostBlasters.UI.MainMenus
{
    public class PlayerProfilePanel : MonoBehaviour, IMoveHandler
    {
        private Text m_name = null;
        private Button m_button = null;

        private Color m_normalColor;
        private Color m_highlightedColor;
        private Mode m_mode;
        private Action<PlayerProfilePanel, Profile, Mode> m_onClick = null;
        private Action<AxisEventData> m_onMove = null;

        private Profile m_profile = null;
        public Profile Profile => m_profile;

        public enum Mode
        {
            Profile,
            AddNew,
            Guest,
        }

        private void Awake()
        {
            m_name = GetComponentInChildren<Text>();
            m_button = GetComponentInChildren<Button>();
            m_button.onClick.AddListener(() => OnClick());
            
            ColorBlock colors = m_button.colors;
            m_normalColor = colors.normalColor;
            m_highlightedColor = colors.highlightedColor;

            colors.normalColor = Color.white;
            colors.highlightedColor = Color.white;
            m_button.colors = colors;

            m_name.alignment = TextAnchor.MiddleCenter;
        }

        public void SetProfile(
            Profile profile, Mode mode, 
            Action<PlayerProfilePanel, Profile, Mode> onClick,
            Action<AxisEventData> onMove
        )
        {
            m_profile = profile;
            m_mode = mode;
            m_onClick = onClick;
            m_onMove = onMove;

            gameObject.SetActive(m_profile != null || m_mode != Mode.Profile);
        }

        public void UpdateGraphics(bool selected, bool faded)
        {
            if (m_mode == Mode.AddNew)
            {
                m_name.fontStyle = FontStyle.Bold;
                UIUtils.FitText(m_name, "Create New");
            }
            else if(m_mode == Mode.Guest)
            {
                m_name.fontStyle = FontStyle.Bold;
                UIUtils.FitText(m_name, "Guest");
            }
            else if (m_profile != null)
            {
                m_name.fontStyle = FontStyle.Normal;
                UIUtils.FitText(m_name, m_profile.Name);
            }
            
            m_name.color = faded ? (selected ? new Color(0.05f, 0.05f, 0.05f, 1f) : new Color(0.65f, 0.65f, 0.65f, 0.5f)) : Color.white;
            m_name.GetComponent<Outline>().enabled = !(selected && faded);

            m_button.targetGraphic.color = selected ? m_highlightedColor : m_normalColor;
        }

        public void OnMove(AxisEventData eventData)
        {
            if (isActiveAndEnabled && m_onMove != null)
            {
                m_onMove(eventData);
            }
        }

        private void OnClick()
        {
            if (isActiveAndEnabled && m_onClick != null)
            {
                m_onClick(this, m_profile, m_mode);
            }
        }
    }
}
