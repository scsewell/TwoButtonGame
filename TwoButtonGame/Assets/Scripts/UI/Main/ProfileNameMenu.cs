﻿using System;

using UnityEngine;
using UnityEngine.UI;

using BoostBlasters.Players;

namespace BoostBlasters.UI.MainMenus
{
    public class ProfileNameMenu : MenuScreen
    {
        private static readonly int MAX_NAME_LENGTH = 16;

        [Header("UI Elements")]

        [SerializeField] private Text m_titleText = null;
        [SerializeField] private Text m_nameText = null;
        [SerializeField] private Button m_acceptButton = null;
        [SerializeField] private Button m_cancelButton = null;

        private PlayerProfile m_editProfile = null;
        private bool m_isNew = false;
        private MenuScreen m_returnToMenu = null;
        private Action<PlayerProfile> m_onComplete = null;
        private string m_currentName = null;
        private bool m_delete = false;
        private bool m_deleteRepeat = false;
        private float m_deleteWait = 0f;

        public override void InitMenu()
        {
            m_acceptButton.onClick.AddListener(() => Accept());
            m_cancelButton.onClick.AddListener(() => Cancel());
        }

        protected override void OnResetMenu(bool fullReset)
        {
            m_delete = false;
            m_deleteRepeat = false;
            m_deleteWait = 0f;
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
                    if (m_currentName.Length < MAX_NAME_LENGTH && char.IsLetterOrDigit(c))
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
                PlayerProfileManager.RenameProfile(m_editProfile, trimmed);
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
                PlayerProfileManager.DeleteProfile(m_editProfile);
            }
            Menu.SetMenu(m_returnToMenu, MenuBase.TransitionSound.Back);

            if (m_onComplete != null)
            {
                m_onComplete(null);
            }
        }
    }
}
