using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

using Framework.UI;

namespace BoostBlasters.UI.RaceMenus
{
    public class GameMenuRoot : MenuScreen
    {
        [Header("UI Elements")]

        [SerializeField] private Button m_resumeButton = null;
        [SerializeField] private Button m_restartButton = null;
        [SerializeField] private Button m_quitButton = null;

        private InRaceMenu m_menu = null;

        public override void InitMenu()
        {
            m_menu = (InRaceMenu)Menu;

            m_resumeButton.onClick.AddListener(() => Resume());
            m_restartButton.onClick.AddListener(() => Restart());
            m_quitButton.onClick.AddListener(() => Quit());

            UIHelper.SetNavigationVertical(m_resumeButton.transform.parent, null, null, null, null, true);
        }

        private void Resume()
        {
            Main.Instance.RaceManager.Resume();
            m_menu.SetMenu(null, MenuBase.TransitionSound.Back);
        }

        private void Restart()
        {
            Main.Instance.RaceManager.Resume();
            Main.Instance.RaceManager.RestartRace();
            m_menu.SetMenu(null);
        }

        private void Quit()
        {
            Main.Instance.RaceManager.Quit();
            m_menu.SetMenu(null);
        }
    }
}
