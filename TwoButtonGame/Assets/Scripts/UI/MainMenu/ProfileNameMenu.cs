using System;
using UnityEngine;
using UnityEngine.UI;

namespace BoostBlasters.Menus
{
    public class ProfileNameMenu : MenuScreen
    {
        private static readonly int MAX_NAME_LENGTH = 16;

        [Header("UI Elements")]
        [SerializeField] private Text m_titleText;
        [SerializeField] private Text m_nameText;
        [SerializeField] private Button m_acceptButton;
        [SerializeField] private Button m_cancelButton;

        private PlayerProfile m_editProfile;
        private bool m_isNew;
        private MenuScreen m_returnToMenu;
        private Action<PlayerProfile> m_onComplete;
        private string m_currentName;
        private bool m_delete;
        private bool m_deleteRepeat;
        private float m_deleteWait;

        public override void InitMenu()
        {
            m_acceptButton.onClick.AddListener(() => Accept());
            m_cancelButton.onClick.AddListener(() => Cancel());
        }

        protected override void OnResetMenu(bool fullReset)
        {
            m_delete = false;
            m_deleteRepeat = false;
            m_deleteWait = 0;
        }

        protected override void OnEnableMenu()
        {
            InputManager.Instance.SetKeyboardMuting(InputMuting.TypingKeys);
        }

        protected override void OnDisableMenu()
        {
            InputManager.Instance.SetKeyboardMuting(InputMuting.None);
            m_editProfile = null;
        }

        protected override void OnUpdate()
        {
            m_delete = false;

            if (m_editProfile != null)
            {
                if (Input.GetKey(KeyCode.Delete) || Input.GetKey(KeyCode.Backspace))
                {
                    if (m_deleteWait <= 0)
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
                
                bool typeSound = false;
                bool deleteSound = false;
                bool invalidSound = false;

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

                foreach (char c in Input.inputString)
                {
                    if (m_currentName.Length < MAX_NAME_LENGTH && UIUtils.IsAlphaNumeric(c))
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
                    Menu.PlaySubmitSound();
                }
                else if (deleteSound)
                {
                    Menu.PlayCancelSound();
                }
                else if (invalidSound)
                {
                    Menu.PlayCancelSound();
                }
            }
        }

        protected override void OnUpdateGraphics()
        {
            m_titleText.text = m_isNew ? "Enter Profile Name" : "Rename Profile";
            m_nameText.text = m_currentName;
        }

        protected override void OnBack()
        {
            Cancel();
        }

        public void EditProfile(PlayerProfile editProfile, bool isNew, MenuScreen returnToMenu, Action<PlayerProfile> onComplete)
        {
            m_editProfile = editProfile;
            m_isNew = isNew;
            m_returnToMenu = returnToMenu;
            m_onComplete = onComplete;

            m_currentName = editProfile.Name;

            Menu.SetMenu(((MainMenu)Menu).ProfileName);
        }
        
        private void Accept()
        {
            string trimmed = m_currentName.Trim();

            if (trimmed.Length > 0)
            {
                PlayerProfileManager.Instance.RenameProfile(m_editProfile, trimmed);
                Menu.SetMenu(m_returnToMenu);
            }
            else
            {
                Menu.PlayCancelSound();
            }

            if (m_onComplete != null)
            {
                m_onComplete(m_editProfile);
            }
        }

        private void Cancel()
        {
            if (m_isNew)
            {
                PlayerProfileManager.Instance.DeleteProfile(m_editProfile);
            }
            Menu.SetMenu(m_returnToMenu, MenuBase.TransitionSound.Back);

            if (m_onComplete != null)
            {
                m_onComplete(null);
            }
        }
    }
}
