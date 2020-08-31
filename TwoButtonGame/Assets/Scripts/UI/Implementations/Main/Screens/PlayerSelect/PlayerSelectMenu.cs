using System.Collections.Generic;
using System.Linq;

using BoostBlasters.Input;
using BoostBlasters.Races;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace BoostBlasters.UI.MainMenu
{
    public class PlayerSelectMenu : MenuScreen
    {
        [Header("UI Elements")]

        [SerializeField] private GameObject m_continueBar = null;
        [SerializeField] private Image m_continueBanner = null;
        [SerializeField] private Control m_continueControls = null;


        private readonly List<PlayerSelectPanel> m_panels = new List<PlayerSelectPanel>();
        private readonly List<InputActionMap> m_backActions = new List<InputActionMap>();
        private bool m_inputEnabled;


        protected override void OnInitialize()
        {
            GetComponentsInChildren(true, m_panels);
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
        }

        protected override void OnHide()
        {
            InputManager.UserAdded -= OnUserAdded;
            InputManager.UserRemoved -= OnUserRemoved;
        }

        private void OnUserAdded(UserInput user)
        {
            var panel = m_panels[user.PlayerIndex];

            panel.JoinUser(user);

            CreateBackActions();
        }

        private void OnUserRemoved(UserInput user)
        {
            var panel = m_panels[user.PlayerIndex];

            panel.Leave();

            CreateBackActions();
        }

        public override void Back()
        {
            base.Back();

            foreach (var panel in m_panels)
            {
                panel.Close();
            }
        }

        protected override void OnUpdate()
        {
            // when global input is enabled, player joining and going back should be active 
            if (Input != null && Input.Actions.UI.enabled)
            {
                EnableInput();
            }
            else
            {
                DisableInput();
            }

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
            m_continueBar.SetActive(false);
            //m_continueBanner.color = Color.Lerp(Color.white, m_bannerCol, (Time.unscaledTime - m_continueTime) / 0.5f);
        }

        private void EnableInput()
        {
            if (m_inputEnabled)
            {
                return;
            }

            m_inputEnabled = true;

            InputManager.ListenForJoiningUsers++;
            CreateBackActions();
        }

        private void DisableInput()
        {
            if (!m_inputEnabled)
            {
                return;
            }

            m_inputEnabled = false;

            InputManager.ListenForJoiningUsers--;
            DestroyBackActions();
        }

        private void CreateBackActions()
        {
            if (!m_inputEnabled)
            {
                return;
            }

            DestroyBackActions();

            // This is the one menu screen in the game where the global input and user input both
            // need to be active simultaniously (user input to do their character selection, and global
            // input is used by unpaired devices/control schemes to back out of the whole menu). Since the
            // input system cannot use different control schemes for each device assigned to an action, 
            // we encounter the problem that any actions taken by a joined user will also be done by the global
            // input. in this case, when users press the back button duing player selection the global input
            // also is performed and backs out from the menu.
            //
            // To work around this, we need to create a temporary back action for all unassigned device/control
            // schemes. A bit gross, but at least it contains the workaround to this one file.
            var actions = InputManager.Global.Actions;
            var users = InputManager.Users;
            var back = actions.UI.Back;

            var schemes = new List<InputControlScheme>();

            foreach (var device in InputSystem.devices)
            {
                foreach (var control in device.allControls)
                {
                    // Only consider controls that are bound to the back action
                    // and have a control scheme assigned.
                    if (!back.TryGetControlBinding(control, out var binding) ||
                        !back.TryGetControlSchemes(control, schemes))
                    {
                        continue;
                    }

                    foreach (var scheme in schemes)
                    {
                        // check that no user is using this device and control scheme
                        if (users.Any(u => u.Matches(device, scheme)))
                        {
                            continue;
                        }

                        var map = new InputActionMap(nameof(PlayerSelectMenu))
                        {
                            devices = new[] { device }
                        };

                        var action = map.AddAction(
                            back.name,
                            back.type,
                            binding.path,
                            back.interactions,
                            back.processors,
                            back.expectedControlType
                        );
                        action.performed += (context) => Back();

                        map.Enable();
                        m_backActions.Add(map);
                    }
                }
            }
        }

        private void DestroyBackActions()
        {
            foreach (var map in m_backActions)
            {
                map.Disable();
                map.Dispose();
            }
            m_backActions.Clear();
        }
    }
}
