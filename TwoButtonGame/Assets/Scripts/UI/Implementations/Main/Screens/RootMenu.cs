using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace BoostBlasters.UI.MainMenus
{
    public class RootMenu : MenuScreen
    {
        private static bool m_showStartMenu;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Init()
        {
            m_showStartMenu = true;
        }


        [Header("UI Elements")]

        [SerializeField] private Button m_playButton = null;
        [SerializeField] private Button m_tutorialButton = null;
        [SerializeField] private Button m_profilesButton = null;
        [SerializeField] private Button m_replaysButton = null;
        [SerializeField] private Button m_settingsButton = null;
        [SerializeField] private Button m_creditsButton = null;
        [SerializeField] private Button m_quitButton = null;
        [SerializeField] private TextMeshProUGUI m_version = null;
        [SerializeField] private Animator m_animator = null;


        protected override void OnInitialize()
        {
            m_version.text = $"v{Application.version}";

            // show the start menu if the game was just launched
            m_animator.Play(m_showStartMenu ? "Start" : "Main");

            m_playButton.onClick.AddListener(() => Menu.SwitchTo<PlayerSelectMenu>(TransitionSound.Next));
            m_profilesButton.onClick.AddListener(() => Menu.SwitchTo<ProfilesMenu>(TransitionSound.Next));
            m_replaysButton.onClick.AddListener(() => Menu.SwitchTo<ReplayMenu>(TransitionSound.Next));
            m_settingsButton.onClick.AddListener(() => Menu.SwitchTo<SettingsMenu>(TransitionSound.Next));
            m_creditsButton.onClick.AddListener(() => Menu.SwitchTo<CreditsMenu>(TransitionSound.Next));
            m_quitButton.onClick.AddListener(() => Application.Quit());
        }

        protected override void OnUpdate()
        {
            // close the start menu once a button has been pressed
            if (m_showStartMenu && InputUtils.GetCurrentDevice() != null)
            {
                m_showStartMenu = false;

                m_animator.SetTrigger("Activate");
                Menu.Sound.PlayNextMenuSound();
            }
        }
    }
}
