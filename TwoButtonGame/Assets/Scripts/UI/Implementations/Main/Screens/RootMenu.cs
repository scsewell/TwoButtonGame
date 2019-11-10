using UnityEngine;
using UnityEngine.UI;

namespace BoostBlasters.UI.MainMenus
{
    public class RootMenu : MenuScreen<MainMenu>
    {
        [Header("UI Elements")]

        [SerializeField] private Button m_playButton = null;
        [SerializeField] private Button m_tutorialButton = null;
        [SerializeField] private Button m_profilesButton = null;
        [SerializeField] private Button m_replaysButton = null;
        [SerializeField] private Button m_settingsButton = null;
        [SerializeField] private Button m_creditsButton = null;
        [SerializeField] private Button m_quitButton = null;

        public override void InitMenu()
        {
            m_playButton.onClick.AddListener(           () => Menu.SetMenu(Menu.PlayerSelect));
            //m_showHowToButton.onClick.AddListener(      () => MainMenu.SetMenu(Menu.HowToPlay));
            m_profilesButton.onClick.AddListener(       () => Menu.SetMenu(Menu.Profiles));
            m_replaysButton.onClick.AddListener(        () => Menu.SetMenu(Menu.Replays));
            m_settingsButton.onClick.AddListener(       () => Menu.SetMenu(Menu.Settings));
            m_creditsButton.onClick.AddListener(        () => Menu.SetMenu(Menu.Credits));
            m_quitButton.onClick.AddListener(           () => Application.Quit());
        }
    }
}
