using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

using Framework.UI;

namespace BoostBlasters.UI.RaceMenus
{
    public class GameMenuFinished : MenuScreen
    {
        [Header("UI Elements")]

        [SerializeField] private Button m_leaveButton = null;
        [SerializeField] private Button m_againButton = null;
        [SerializeField] private Button m_replayButton = null;

        private InRaceMenu m_menu = null;

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
