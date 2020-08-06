﻿using System;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace BoostBlasters.UI
{

    // TO BE REMOVED
    public class Navigable : MonoBehaviour, IMoveHandler
    {
        private Text m_name = null;
        private Button m_button = null;
        private Text m_valueName = null;
        private Image m_leftArrow = null;
        private Image m_rightArrow = null;
        private SoundPlayer m_sounds = null;

        private int m_index = 0;
        private int m_maxIndex = 0;
        private Func<int, string> m_serialize = null;
        private Action m_onChange = null;

        public int Index
        {
            get => m_index;
            set
            {
                if (m_index != value)
                {
                    m_index = value;
                    UpdateDisplay();
                }
            }
        }

        private void Awake()
        {
            m_sounds = GetComponentInParent<SoundPlayer>();
            m_button = GetComponentInChildren<Button>();

            m_name          = transform.Find("Label").GetComponent<Text>();
            m_valueName     = transform.Find("Highlight").GetComponentInChildren<Text>();
            m_leftArrow     = transform.Find("LeftArrow").GetComponent<Image>();
            m_rightArrow    = transform.Find("RightArrow").GetComponent<Image>();
        }

        public Navigable Init(string name, int maxIndex, Func<int, string> serialize, Action onChange)
        {
            m_name.text = name + ":";
            m_maxIndex = maxIndex;
            m_serialize = serialize;
            m_onChange = onChange;

            UpdateDisplay();

            return this;
        }

        public void SetMaxIndex(int maxIndex)
        {
            m_maxIndex = maxIndex;
            m_index = Mathf.Clamp(m_index, 0, m_maxIndex);
            UpdateDisplay();
        }

        public void UpdateGraphics()
        {
            bool selected = EventSystem.current.currentSelectedGameObject == m_button.gameObject;

            if (m_leftArrow)
            {
                m_leftArrow.enabled = selected;
            }

            if (m_leftArrow)
            {
                m_rightArrow.enabled = selected;
            }
        }

        public void OnMove(AxisEventData eventData)
        {
            switch (eventData.moveDir)
            {
                case MoveDirection.Left: ChangeValue(-1); break;
                case MoveDirection.Right: ChangeValue(1); break;
            }
        }

        public void ChangeValue(int offset)
        {
            int count = m_maxIndex + 1;
            m_index = (m_index + count + offset) % count;

            m_onChange?.Invoke();

            m_sounds.PlaySelectSound();
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            m_valueName.text = m_serialize(m_index);
        }
    }
}
