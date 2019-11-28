using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

using TMPro;

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
        [SerializeField] private TextMeshProUGUI m_version = null;
        [SerializeField] private Animator m_animator = null;

        private static bool m_showStartMenu;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Init()
        {
            m_showStartMenu = true;
        }

        public override void InitMenu()
        {
            m_version.text = $"v{Application.version}";

            // show the start menu if the game was just launched
            m_animator.Play(m_showStartMenu ? "Start" : "Main");

            m_playButton.onClick.AddListener(           () => Menu.SetMenu(Menu.PlayerSelect));
            m_profilesButton.onClick.AddListener(       () => Menu.SetMenu(Menu.Profiles));
            m_replaysButton.onClick.AddListener(        () => Menu.SetMenu(Menu.Replays));
            m_settingsButton.onClick.AddListener(       () => Menu.SetMenu(Menu.Settings));
            m_creditsButton.onClick.AddListener(        () => Menu.SetMenu(Menu.Credits));
            m_quitButton.onClick.AddListener(           () => Application.Quit());
        }

        protected override void OnUpdate()
        {
            if (m_showStartMenu)
            {
                // check if there is any button pressed
                bool anyKey = Keyboard.current.anyKey.isPressed;

                foreach (var control in Gamepad.current.allControls)
                {
                    if (control.IsActuated(0.5f))
                    {
                        anyKey = true;
                        break;
                    }
                }

                // close the start menu if a button is pressed
                if (anyKey)
                {
                    m_animator.SetTrigger("Activate");
                    Menu.Sound.PlayNextMenuSound();

                    m_showStartMenu = false;
                }
            }
        }
    }
}
