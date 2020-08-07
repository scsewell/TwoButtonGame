using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

using BoostBlasters.Players;
using BoostBlasters.Races;

namespace BoostBlasters.UI.MainMenus
{
    public class PlayerSelectMenu : MenuScreen
    {
        private static readonly List<int> s_playerPanelIndices = new List<int>();


        [Header("UI Elements")]

        [SerializeField] private RectTransform m_playerSelectContent = null;
        [SerializeField] private GameObject m_continueBar = null;
        [SerializeField] private Image m_continueBanner = null;
        [SerializeField] private Control m_continueControls = null;

        private readonly List<PlayerSelectPanel> m_playerSelectPanels = new List<PlayerSelectPanel>();
        private bool m_canContine;
        private float m_continueTime;
        private Color m_bannerCol;

        public List<PlayerBaseInput> ActiveInputs => m_playerSelectPanels.Select(p => p.Input).Where(i => i != null).ToList();

        /// <summary>
        /// The configurations of ready players.
        /// </summary>
        public List<RacerConfig> Configs => ReadyPlayers.Select(p => p.Config).ToList();

        public List<Profile> PlayerProfiles => m_playerSelectPanels.Select(p => p.Profile).Where(p => p != null).ToList();

        public List<PlayerSelectPanel> ReadyPlayers => m_playerSelectPanels.Where(p => p.IsReady).ToList();

        protected override void Awake()
        {
            base.Awake();

            // WHy not INitMenu?
            m_playerSelectContent.GetComponentsInChildren(true, m_playerSelectPanels);
        }

        protected override void OnInitialize()
        {
            for (int i = 0; i < 4; i++)
            {
                m_playerSelectPanels[i].Init(this, i);
            }

            m_bannerCol = m_continueBanner.color;

            RaceParameters lastRace = Main.Instance.LastRaceParams;
            if (lastRace != null)
            {
                for (int i = 0; i < s_playerPanelIndices.Count; i++)
                {
                    int index = s_playerPanelIndices[i];
                    m_playerSelectPanels[index].FromConfig(lastRace.Racers[i]);
                }
            }
        }
        
        protected override void OnHide()
        {
            m_playerSelectPanels.ForEach(p => p.SetCameraActive(false));
        }

        //protected override void OnResetMenu(bool fullReset)
        //{
        //    m_playerSelectPanels.ForEach(p => p.ResetState(fullReset));
        //    m_canContine = false;
        //    m_continueTime = 0;
        //}

        public override void Back()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                base.Back();
            }
        }

        protected override void OnUpdate()
        {
            s_playerPanelIndices.Clear();

            for (int i = 0; i < 4; i++)
            {
                PlayerSelectPanel panel = m_playerSelectPanels[i];
                panel.UpdatePanel();

                if (panel.IsReady)
                {
                    s_playerPanelIndices.Add(i);
                }
            }
            
            bool canContinue = m_playerSelectPanels.All(p => p.CanContinue) && ReadyPlayers.Count > 0;
            if (m_canContine != canContinue)
            {
                m_canContine = canContinue;
                m_continueTime = Time.unscaledTime;
            }
            else if (m_canContine)
            {
                m_continueControls.UpdateUI("Continue", (Menu as MainMenu).ReservedInputs.SelectMany(i => i.SpriteAccept).ToList());

                if (m_playerSelectPanels.Any(p => p.Continue))
                {
                    Menu.SwitchTo<LevelSelectMenu>(TransitionSound.Next);
                }
            }
        }

        protected override void OnUpdateVisuals()
        {
            m_continueBar.SetActive(m_canContine);
            m_continueBanner.color = Color.Lerp(Color.white, m_bannerCol, (Time.unscaledTime - m_continueTime) / 0.5f);

            m_playerSelectPanels.ForEach(p => p.UpdateGraphics(Menu as MainMenu));
        }
    }
}
