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

        protected override void OnInitialize()
        {
            m_leaveButton.onClick.AddListener(() => Leave());
            m_againButton.onClick.AddListener(() => Restart());
            m_replayButton.onClick.AddListener(() => ViewReplay());

            UIHelper.SetNavigationVertical(new NavConfig()
            {
                parent = m_leaveButton.transform.parent,
                allowDisabled = true,
            });
        }

        private void Leave()
        {
            Main.Instance.RaceManager.Quit();
           // Menu.SwitchTo(null);
        }

        private void Restart()
        {
            Main.Instance.RaceManager.Resume();
            Main.Instance.RaceManager.RestartRace();
           // Menu.SwitchTo(null);
        }

        private void ViewReplay()
        {

        }
    }
}
