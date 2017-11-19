using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace BoostBlasters.MainMenus
{
    public class PlayerSelectMenu : MenuScreen
    {
        [Header("UI Elements")]
        [SerializeField] private RectTransform m_playerSelectContent;
        [SerializeField] private PlayerSelectPanel m_playerSelectPrefab;
        [SerializeField] private GameObject m_continueBar;
        [SerializeField] private Image m_continueBanner;
        [SerializeField] private ControlPanel m_continueControls;

        private List<PlayerSelectPanel> m_playerSelectPanels;
        private List<PlayerSelectPanel> m_readyPlayers;
        private bool m_canContine;
        private float m_continueTime;
        private Color m_bannerCol;
        
        public List<PlayerBaseInput> ActiveInputs
        {
            get { return m_playerSelectPanels.Select(p => p.Input).Where(i => i != null).ToList(); }
        }

        public List<PlayerConfig> SelectedConfigs
        {
            get { return m_readyPlayers.Select(p => p.SelectedConfig).ToList(); }
        }

        public List<int> PlayerIndices
        {
            get { return m_readyPlayers.Select((p, i) => i).ToList(); }
        }

        protected override void Awake()
        {
            base.Awake();

            m_playerSelectPanels = new List<PlayerSelectPanel>();
            m_readyPlayers = new List<PlayerSelectPanel>();

            for (int i = 0; i < 4; i++)
            {
                PlayerSelectPanel p = Instantiate(m_playerSelectPrefab, m_playerSelectContent);
                p.Init(i);
                m_playerSelectPanels.Add(p);
            }
        }

        public override void InitMenu(RaceParameters lastRace)
        {
            m_bannerCol = m_continueBanner.color;

            if (lastRace != null)
            {
                for (int i = 0; i < lastRace.PlayerIndicies.Count; i++)
                {
                    m_playerSelectPanels[lastRace.PlayerIndicies[i]].FromConfig(lastRace.Inputs[i], lastRace.PlayerConfigs[i]);
                }
            }
        }
        
        protected override void OnDisableMenu()
        {
            m_playerSelectPanels.ForEach(p => p.SetCameraActive(false));
        }

        protected override void OnResetMenu(bool fullReset)
        {
            m_playerSelectPanels.ForEach(p => p.ResetState(fullReset));
            m_canContine = false;
            m_continueTime = 0;
        }

        protected override void OnUpdate()
        {
            int playerNum = 0;
            for (int i = 0; i < 4; i++)
            {
                m_playerSelectPanels[i].UpdatePanel(playerNum, MainMenu);

                if (m_playerSelectPanels[i].IsJoined)
                {
                    playerNum++;
                }
            }

            m_readyPlayers = m_playerSelectPanels.Where(p => p.IsReady).ToList();

            bool canContinue = m_playerSelectPanels.All(p => p.CanContinue) && m_readyPlayers.Count > 0;
            if (m_canContine != canContinue)
            {
                m_canContine = canContinue;
                m_continueTime = Time.unscaledTime;
            }
            else if (m_canContine)
            {
                m_continueControls.UpdateUI("Continue", MainMenu.ActiveInputs.SelectMany(i => i.SpriteAccept).ToList());

                if (m_playerSelectPanels.Any(p => p.Continue))
                {
                    MainMenu.SetMenu(Menu.LevelSelect);
                }
            }
        }

        protected override void OnUpdateGraphics()
        {
            m_continueBar.SetActive(m_canContine);
            m_continueBanner.color = Color.Lerp(Color.white, m_bannerCol, (Time.unscaledTime - m_continueTime) / 0.5f);

            m_playerSelectPanels.ForEach(p => p.UpdateGraphics(MainMenu));
        }
    }
}
