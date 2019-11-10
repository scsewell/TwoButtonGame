using UnityEngine;
using UnityEngine.UI;

using Framework.UI;

namespace BoostBlasters.UI.RaceMenus
{
    public class GameMenuFinished : MenuScreen<InRaceMenu>
    {
        [Header("UI Elements")]

        [SerializeField] private Button m_leaveButton = null;
        [SerializeField] private Button m_againButton = null;
        [SerializeField] private Button m_replayButton = null;

        public override void InitMenu()
        {
            m_leaveButton.onClick.AddListener(() => Leave());
            m_againButton.onClick.AddListener(() => Restart());
            m_replayButton.onClick.AddListener(() => ViewReplay());

            UIHelper.SetNavigationVertical(m_leaveButton.transform.parent, null, null, null, null, true);
        }

        private void Leave()
        {
            Main.Instance.RaceManager.Quit();
            Menu.SetMenu(null);
        }

        private void Restart()
        {
            Main.Instance.RaceManager.Resume();
            Main.Instance.RaceManager.RestartRace();
            Menu.SetMenu(null);
        }

        private void ViewReplay()
        {

        }
    }
}
