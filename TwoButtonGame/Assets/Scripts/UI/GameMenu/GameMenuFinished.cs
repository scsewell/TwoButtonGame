using System;
using System.Collections.Generic;
using System.Linq;
using Framework.UI;
using UnityEngine;
using UnityEngine.UI;

namespace BoostBlasters.Menus
{
    public class GameMenuFinished : MenuScreen
    {
        [Header("UI Elements")]
        [SerializeField] private Button m_leaveButton;
        [SerializeField] private Button m_againButton;
        [SerializeField] private Button m_replayButton;

        private InRaceMenu m_menu;

        public override void InitMenu()
        {
            m_menu = (InRaceMenu)Menu;

            m_leaveButton.onClick.AddListener(() => Leave());
            m_againButton.onClick.AddListener(() => Restart());
            m_replayButton.onClick.AddListener(() => ViewReplay());

            UIHelper.SetNavigationVertical(m_leaveButton.transform.parent, null, null, null, null, true);
        }

        private void Leave()
        {
            Main.Instance.RaceManager.Quit();
            m_menu.SetMenu(null);
        }

        private void Restart()
        {
            Main.Instance.RaceManager.Resume();
            Main.Instance.RaceManager.RestartRace();
            m_menu.SetMenu(null);
        }

        private void ViewReplay()
        {

        }
    }
}
