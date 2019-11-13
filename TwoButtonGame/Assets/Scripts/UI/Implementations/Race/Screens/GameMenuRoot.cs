using UnityEngine;
using UnityEngine.UI;

using Framework.UI;

namespace BoostBlasters.UI.RaceMenus
{
    public class GameMenuRoot : MenuScreen<InRaceMenu>
    {
        [Header("UI Elements")]

        [SerializeField] private Button m_resumeButton = null;
        [SerializeField] private Button m_restartButton = null;
        [SerializeField] private Button m_quitButton = null;

        public override void InitMenu()
        {
            m_resumeButton.onClick.AddListener(() => Resume());
            m_restartButton.onClick.AddListener(() => Restart());
            m_quitButton.onClick.AddListener(() => Quit());

            
            UIHelper.SetNavigationVertical(new NavConfig()
            {
                parent = m_resumeButton.transform.parent,
                allowDisabled = true,
            });
        }

        private void Resume()
        {
            Main.Instance.RaceManager.Resume();
            Menu.SetMenu(null, TransitionSound.Back);
        }

        private void Restart()
        {
            Main.Instance.RaceManager.Resume();
            Main.Instance.RaceManager.RestartRace();
            Menu.SetMenu(null);
        }

        private void Quit()
        {
            Main.Instance.RaceManager.Quit();
            Menu.SetMenu(null);
        }
    }
}
