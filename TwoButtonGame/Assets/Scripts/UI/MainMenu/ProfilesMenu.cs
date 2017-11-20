using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Framework.UI;

namespace BoostBlasters.MainMenus
{
    public class ProfilesMenu : MenuScreen
    {
        [Header("UI Elements")]
        [SerializeField] private RectTransform m_selectContent;
        [SerializeField] private Image m_arrowLeft;
        [SerializeField] private Image m_arrowRight;
        [SerializeField] private Text m_pageText;
        [SerializeField] private Button m_backButton;

        [Header("Options")]
        [SerializeField]
        private int m_selectPanelCount = 15;

        private int m_page;

        protected override void Awake()
        {
            base.Awake();

            m_backButton.onClick.AddListener(() => MainMenu.SetMenu(Menu.Root, true));
        }

        public override void InitMenu(RaceParameters lastRace)
        {
        }
    }
}
