using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

using BoostBlasters.Players;
using BoostBlasters.Characters;
using BoostBlasters.Races;

namespace BoostBlasters.UI.MainMenus
{
    public class PlayerSelectMenu : MenuScreen
    {
        [Header("UI Elements")]

        [SerializeField] private RectTransform m_playerSelectContent = null;
        [SerializeField] private GameObject m_continueBar = null;
        [SerializeField] private Image m_continueBanner = null;
        [SerializeField] private ControlPanel m_continueControls = null;

        private List<PlayerSelectPanel> m_playerSelectPanels;
        private bool m_canContine;
        private float m_continueTime;
        private Color m_bannerCol;

        public List<PlayerBaseInput> ActiveInputs => m_playerSelectPanels.Select(p => p.Input).Where(i => i != null).ToList();

        public List<PlayerProfile> PlayerProfiles => m_playerSelectPanels.Select(p => p.Profile).Where(p => p != null).ToList();

        public List<Character> CharacterConfigs => ReadyPlayers.Select(p => p.CharacterConfig).ToList();

        public List<int> PlayerIndices => ReadyPlayers.Select((p, i) => i).ToList();

        public List<PlayerSelectPanel> ReadyPlayers => m_playerSelectPanels.Where(p => p.IsReady).ToList();

        protected override void Awake()
        {
            base.Awake();

            m_playerSelectPanels = new List<PlayerSelectPanel>();

            PlayerSelectPanel panel = m_playerSelectContent.GetComponentInChildren<PlayerSelectPanel>();
            m_playerSelectPanels.Add(panel);

            for (int i = 1; i < 4; i++)
            {
                PlayerSelectPanel p = Instantiate(panel, m_playerSelectContent);
                m_playerSelectPanels.Add(p);
            }

            for (int i = 0; i < 4; i++)
            {
                m_playerSelectPanels[i].Init(this, i);
            }
        }

        public override void InitMenu()
        {
            m_bannerCol = m_continueBanner.color;

            RaceParameters lastRace = Main.Instance.LastRaceParams;
            if (lastRace != null)
            {
                for (int i = 0; i < lastRace.playerIndicies.Count; i++)
                {
                    int index = lastRace.playerIndicies[i];
                    PlayerProfile profile = lastRace.profiles[i];
                    PlayerBaseInput input = lastRace.inputs[i];
                    Character config = lastRace.characters[i];

                    m_playerSelectPanels[index].FromConfig(profile, input, config);
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

        protected override void OnBack()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                base.OnBack();
            }
        }

        protected override void OnUpdate()
        {
            int playerNum = 0;
            for (int i = 0; i < 4; i++)
            {
                m_playerSelectPanels[i].UpdatePanel();

                if (m_playerSelectPanels[i].IsJoined)
                {
                    playerNum++;
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
                m_continueControls.UpdateUI("Continue", ((MainMenu)Menu).ReservedInputs.SelectMany(i => i.SpriteAccept).ToList());

                if (m_playerSelectPanels.Any(p => p.Continue))
                {
                    Menu.SetMenu(((MainMenu)Menu).LevelSelect);
                }
            }
        }

        protected override void OnUpdateGraphics()
        {
            m_continueBar.SetActive(m_canContine);
            m_continueBanner.color = Color.Lerp(Color.white, m_bannerCol, (Time.unscaledTime - m_continueTime) / 0.5f);

            m_playerSelectPanels.ForEach(p => p.UpdateGraphics((MainMenu)Menu));
        }
    }
}
