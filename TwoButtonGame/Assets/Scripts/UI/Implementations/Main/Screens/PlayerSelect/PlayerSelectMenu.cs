using System.Collections.Generic;
using System.Linq;

using BoostBlasters.Input;
using BoostBlasters.Races;

using UnityEngine;
using UnityEngine.UI;

namespace BoostBlasters.UI.MainMenu
{
    public class PlayerSelectMenu : MenuScreen
    {
        [Header("UI Elements")]

        [SerializeField] private GameObject m_continueBar = null;
        [SerializeField] private Image m_continueBanner = null;
        [SerializeField] private Control m_continueControls = null;


        private readonly List<PlayerSelectPanel> m_panels = new List<PlayerSelectPanel>();
        private bool m_canContine;
        private float m_continueTime;
        private Color m_bannerCol;


        protected override void OnInitialize()
        {
            GetComponentsInChildren(true, m_panels);

            m_bannerCol = m_continueBanner.color;
        }

        public void Open(RaceParameters raceParams, TransitionSound sound)
        {
            Menu.SwitchTo(this, sound);

            foreach (var panel in m_panels)
            {
                panel.Open(raceParams);
            }
        }

        protected override void OnShow()
        {
            InputManager.UserAdded += OnUserAdded;
            InputManager.UserRemoved += OnUserRemoved;

            InputManager.JoiningEnabled = true;

            m_canContine = false;
            m_continueTime = 0;
        }

        protected override void OnHide()
        {
            InputManager.UserAdded -= OnUserAdded;
            InputManager.UserRemoved -= OnUserRemoved;

            InputManager.JoiningEnabled = false;
        }

        private void OnUserAdded(UserInput user)
        {
            var index = user.Player.playerIndex;
            var panel = m_panels[index];

            panel.AssignUser(user);
        }

        private void OnUserRemoved(UserInput user)
        {
            var index = user.Player.playerIndex;
            var panel = m_panels[index];

            panel.Leave();
        }

        public override void Back()
        {
            base.Back();

            foreach (var panel in m_panels)
            {
                panel.Leave();
                panel.Close();
            }
        }

        protected override void OnUpdate()
        {
            //var canContinue = m_playerSelectPanels.All(p => p.CanContinue) && ReadyPlayers.Count > 0;
            //if (m_canContine != canContinue)
            //{
            //    m_canContine = canContinue;
            //    m_continueTime = Time.unscaledTime;
            //}
            //else if (m_canContine)
            //{
            //    m_continueControls.UpdateUI("Continue", (Menu as MainMenu).ReservedInputs.SelectMany(i => i.SpriteAccept).ToList());

            //    if (m_playerSelectPanels.Any(p => p.Continue))
            //    {
            //        Menu.SwitchTo<LevelSelectMenu>(TransitionSound.Next);
            //    }
            //}
        }

        protected override void OnUpdateVisuals()
        {
            m_continueBar.SetActive(m_canContine);
            m_continueBanner.color = Color.Lerp(Color.white, m_bannerCol, (Time.unscaledTime - m_continueTime) / 0.5f);
        }
    }
}
