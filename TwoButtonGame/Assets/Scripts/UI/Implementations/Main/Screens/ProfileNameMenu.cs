using System;

using BoostBlasters.Players;

using UnityEngine;
using UnityEngine.UI;

namespace BoostBlasters.UI.MainMenus
{
    public class ProfileNameMenu : MenuScreen
    {
        [Header("UI Elements")]

        [SerializeField] private Text m_titleText = null;
        [SerializeField] private Text m_nameText = null;
        [SerializeField] private Button m_acceptButton = null;
        [SerializeField] private Button m_cancelButton = null;

        private Profile m_profile = null;
        private bool m_isNew = false;
        private Action<Profile> m_onComplete = null;
        private MenuScreen m_returnToMenu = null;
        private string m_currentName = null;
        private bool m_delete = false;
        private bool m_deleteRepeat = false;
        private float m_deleteWait = 0f;


        protected override void OnInitialize()
        {
            m_acceptButton.onClick.AddListener(() => Accept());
            m_cancelButton.onClick.AddListener(() => Cancel());
        }

        //protected override void OnResetMenu(bool fullReset)
        //{
        //    m_delete = false;
        //    m_deleteRepeat = false;
        //    m_deleteWait = 0f;
        //}

        public void EditProfile(Profile profile, bool isNew, Action<Profile> onComplete, MenuScreen returnToMenu)
        {
            Menu.SwitchTo(this, TransitionSound.Next);

            m_profile = profile;
            m_isNew = isNew;
            m_returnToMenu = returnToMenu;
            m_onComplete = onComplete;

            m_currentName = profile.Name;
        }

        protected override void OnShow()
        {
            //InputManager.Instance.SetKeyboardMuting(InputMuting.TypingKeys);
        }

        protected override void OnHide()
        {
            //InputManager.Instance.SetKeyboardMuting(InputMuting.None);
            m_profile = null;
        }

        protected override void OnUpdate()
        {
            m_delete = false;

            if (m_profile != null)
            {
                if (Input.GetKey(KeyCode.Delete) || Input.GetKey(KeyCode.Backspace))
                {
                    if (m_deleteWait <= 0f)
                    {
                        m_delete = true;
                        m_deleteWait = m_deleteRepeat ? CustomInputModule.REPEAT_DELAY : CustomInputModule.REPEAT_WAIT;
                        m_deleteRepeat = true;
                    }
                    else
                    {
                        m_deleteWait -= Time.deltaTime;
                    }
                }
                else
                {
                    m_deleteRepeat = false;
                    m_deleteWait = 0;
                }

                var typeSound = false;
                var deleteSound = false;
                var invalidSound = false;

                if (m_delete)
                {
                    if (m_currentName.Length > 0)
                    {
                        m_currentName = m_currentName.Substring(0, m_currentName.Length - 1);
                        deleteSound = true;
                    }
                    else
                    {
                        invalidSound = true;
                    }
                }

                foreach (var c in Input.inputString)
                {
                    if (m_currentName.Length < Consts.MAX_PROFILE_NAME_LENGTH && char.IsLetterOrDigit(c))
                    {
                        m_currentName += c;
                        typeSound = true;
                    }
                    else
                    {
                        invalidSound = true;
                    }
                }

                if (typeSound)
                {
                    Menu.Sound.PlaySubmitSound();
                }
                else if (deleteSound)
                {
                    Menu.Sound.PlayCancelSound();
                }
                else if (invalidSound)
                {
                    Menu.Sound.PlayCancelSound();
                }
            }
        }

        protected override void OnUpdateVisuals()
        {
            m_titleText.text = m_isNew ? "Enter Profile Name" : "Rename Profile";
            m_nameText.text = m_currentName;
        }

        public override void Back()
        {
            Cancel();
        }

        private void Accept()
        {
            Complete();

            var trimmed = m_currentName.Trim();

            if (trimmed.Length > 0)
            {
                ProfileManager.RenameProfile(m_profile, trimmed);
                Menu.SwitchTo(m_returnToMenu, TransitionSound.None);
                Menu.Sound.PlaySubmitSound();
            }
            else
            {
                Menu.Sound.PlayCancelSound();
            }

            m_onComplete?.Invoke(m_profile);
        }

        private void Cancel()
        {
            Complete();

            if (m_isNew)
            {
                ProfileManager.DeleteProfile(m_profile);
            }
            Menu.SwitchTo(m_returnToMenu, TransitionSound.Back);

            m_onComplete?.Invoke(null);
        }

        private void Complete()
        {
            if (m_returnToMenu != null)
            {
                Menu.SwitchTo(m_returnToMenu, TransitionSound.None);
            }
            else
            {
                Menu.Close(this, TransitionSound.None);
            }
        }
    }
}
