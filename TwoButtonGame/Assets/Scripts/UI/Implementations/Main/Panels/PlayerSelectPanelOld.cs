﻿using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

using BoostBlasters.Profiles;
using BoostBlasters.Characters;
using BoostBlasters.Races;

namespace BoostBlasters.UI.MainMenu
{
    public class PlayerSelectPanelOld : MonoBehaviour
    {
        [Header("Prefabs")]

        [SerializeField] private GameObject m_profileSelectPrefab = null;
        [SerializeField] private GameObject m_characterPreviewPrefab = null;

        [Header("UI Elements")]

        [SerializeField] private GameObject m_joinTab = null;
        [SerializeField] private GameObject m_profileTab = null;
        [SerializeField] private GameObject m_characterSelectTab = null;
        [SerializeField] private Text m_playerName = null;
        [SerializeField] private Text m_readyText = null;
        [SerializeField] private RectTransform m_profileContent = null;
        [SerializeField] private Control m_joinControls = null;
        [SerializeField] private Control m_controls1 = null;
        [SerializeField] private Control m_controls2 = null;
        [SerializeField] private Control m_controls3 = null;
        [SerializeField] private RawImage m_characterPreview = null;
        [SerializeField] private Text m_characterName = null;
        [SerializeField] private GameObject m_characterStats = null;
        [SerializeField] private Text m_characterSpeed = null;
        [SerializeField] private Text m_characterAgility = null;
        [SerializeField] private Text m_characterDesc = null;
        [SerializeField] private Image m_characterBackground = null;
        [SerializeField] private Image m_characterHighlight = null;

        [Header("Options")]

        [SerializeField]
        private int m_profilePanelCount = 14;
        [SerializeField]
        [Range(0.25f, 1f)]
        private float m_resolutionScale = 1.0f;
        [SerializeField]
        private Color m_previewBgColor = new Color(0.05f, 0.05f, 0.05f);
        [SerializeField]
        [Range(0f, 1f)]
        private float m_previewBgColorFac = 0.1f;
        [SerializeField]
        [Range(0f, 5f)]
        private float m_rotateWait = 2.0f;
        [SerializeField]
        [Range(0f, 180f)]
        private float m_rotateSpeed = 30.0f;
        [SerializeField]
        private Gradient m_ratingGradient = null;

        private enum State
        {
            Join,
            Profile,
            Select,
            Ready,
        }

        private MenuBase m_menu = null;
        private PlayerSelectMenu m_selectMenu = null;
        private List<GameObject> m_profilePanels = null;
        private Camera m_previewCam = null;
        private RenderTexture m_previewTex = null;
        private Dictionary<Character, GameObject> m_configToPreview = null;
        private List<GameObject> m_previewObjects = null;
        private State m_state = State.Join;
        private int m_playerNum = 0;
        private float m_selectTime = 0f;

        private PlayerBaseInput m_input;
        public PlayerBaseInput Input => m_input;

        private int m_profileWindow;
        private int m_selectedProfile;
        private Profile m_profile;

        private int SelectedProfile
        {
            get => m_selectedProfile;
            set
            {
                if (m_selectedProfile != value)
                {
                    m_selectedProfile = value;

                    int centeringWindow = m_selectedProfile - ((m_profilePanelCount - 1) / 2);
                    int maxIndex = ProfileManager.Profiles.Count + 2;
                    m_profileWindow = Mathf.Clamp(centeringWindow, 0, maxIndex - m_profilePanelCount);
                }
            }
        }

        public Profile Profile
        {
            get => m_profile;
            private set
            {
                if (m_profile != null && m_profile.IsTemporary)
                {
                    ProfileManager.ReleaseTemporaryProfile(m_profile);
                }
                m_profile = value;
            }
        }

        private int m_selectedCharacter;
        public Character Character => CharacterManager.Characters[m_selectedCharacter];

        public RacerConfig Config => RacerConfig.CreatePlayer(Character, Profile, Input);

        public bool IsJoined => m_state != State.Join;
        public bool IsReady => m_state == State.Ready;
        public bool CanContinue => m_state == State.Ready || m_state == State.Join;

        public bool Continue { get; private set; } = false;

        private void OnDestroy()
        {
            FreeTexture();
        }

        public void Init(PlayerSelectMenu screen, int index)
        {
            m_menu = screen.Menu;
            m_selectMenu = screen;
            m_playerNum = index;

            // create panels used for selecting from available player profiles
            m_profilePanels = new List<GameObject>();

            for (int i = 0; i < m_profilePanelCount; i++)
            {
                m_profilePanels.Add(Instantiate(m_profileSelectPrefab, m_profileContent));
            }

            // create a camera for rendering the character preview
            int previewLayer = (index + 8);

            m_previewCam = Instantiate(m_characterPreviewPrefab).GetComponentInChildren<Camera>();
            m_previewCam.cullingMask = (1 << previewLayer);

            // load all the character models
            m_configToPreview = new Dictionary<Character, GameObject>();
            m_previewObjects = new List<GameObject>();

            foreach (Character config in CharacterManager.Characters)
            {
                GameObject pivot = new GameObject("CharacterPivot");

                GameObject previewObject = Instantiate(config.Graphics.Rig, pivot.transform);
                previewObject.transform.localPosition = config.Graphics.Offset + Vector3.up;
                previewObject.GetComponentsInChildren<Transform>(true).ToList().ForEach(r => r.gameObject.layer = previewLayer);

                m_configToPreview.Add(config, pivot);
                m_previewObjects.Add(pivot);
            }

            ResetState(true);
        }

        public void ResetState(bool fullReset)
        {
            if (fullReset)
            {
                m_input = null;
                Profile = null;
                SelectedProfile = 0;
                m_selectedCharacter = 0;
                m_state = State.Join;
            }

            SetProfiles();

            m_characterHighlight.color = new Color(1f, 1f, 1f, 0f);
        }

        public void FromConfig(RacerConfig config)
        {
            m_state = State.Ready;
            Profile = config.Profile;
            m_input = config.Input;
            SelectCharacter(Array.IndexOf(CharacterManager.Characters, config.Character));
        }

        public void SetCameraActive(bool isActive)
        {
            if (m_previewCam != null)
            {
                m_previewCam.enabled = isActive;
            }
        }
    
        public void UpdatePanel()
        {
            Continue = false;

            if (m_state == State.Join)
            {
                Profile = null;
                SelectedProfile = 0;

                //foreach (PlayerBaseInput input in m_screen.UnreservedInputs)
                //{
                //    if (input.UI_Accept)
                //    {
                //        m_state = State.Profile;
                //        m_input = input;
                //        m_screen.Sound.PlaySubmitSound();
                //        break;
                //    }
                //    else if (input.UI_Cancel)
                //    {
                //        m_screen.SetMenu(m_screen.Root,TransitionSound.Back);
                //    }
                //}
            }
            else if (m_state == State.Profile)
            {
                Profile = null;

                if (m_input.UI_Accept)
                {
                    if (SelectedProfile == 0)
                    {
                        m_state = State.Select;
                        Profile = ProfileManager.CreateTemporaryProfile("Guest", true);
                        SelectCharacter(m_selectedCharacter);
                        m_menu.Sound.PlaySubmitSound();
                    }
                    else if (SelectedProfile == 1)
                    {
                        //m_menu.Get<ProfileNameMenu>().CreateNew(OnProfileCreate, m_selectMenu);
                    }
                    else
                    {
                        Profile profile = ProfileManager.Profiles[SelectedProfile - 2];

                        //if (!m_selectMenu.PlayerProfiles.Contains(profile))
                        //{
                        //    m_state = State.Select;
                        //    Profile = profile;
                        //    SelectCharacter(m_selectedCharacter);
                        //    m_menu.Sound.PlaySubmitSound();
                        //}
                        //else
                        //{
                        //    m_menu.Sound.PlayCancelSound();
                        //}
                    }
                }
                else if (m_input.UI_Down || m_input.UI_Up)
                {
                    int previous = SelectedProfile;
                    int maxIndex = ProfileManager.Profiles.Count + 1;

                    SelectedProfile = Mathf.Clamp(SelectedProfile + (m_input.UI_Down ? 1 : -1), 0, maxIndex);
                    
                    if (previous != SelectedProfile)
                    {
                        m_menu.Sound.PlaySelectSound();
                    }
                }
                else if (m_input.UI_Cancel)
                {
                    m_state = State.Join;
                    m_menu.Sound.PlayCancelSound();
                }
            }
            else if (m_state == State.Select)
            {
                if (m_input.UI_Accept)
                {
                    m_state = State.Ready;
                    m_menu.Sound.PlaySubmitSound();
                }
                else if (m_input.UI_Right || m_input.UI_Left)
                {
                    SelectCharacter(m_selectedCharacter + (m_input.UI_Right ? 1 : -1));
                    m_characterHighlight.color = new Color(1, 1, 1, 0.35f);
                    m_menu.Sound.PlaySelectSound();
                }
                else if (m_input.UI_Cancel)
                {
                    m_state = State.Profile;
                    Profile = null;
                    m_menu.Sound.PlayCancelSound();
                }
            }
            else if (m_state == State.Ready)
            {
                if (m_input.UI_Accept)
                {
                    Continue = true;
                }
                else if (m_input.UI_Cancel)
                {
                    m_state = State.Select;
                    m_menu.Sound.PlayCancelSound();
                }
            }
        }

        private void OnProfileCreate(Profile profile)
        {
            if (profile != null)
            {
                IReadOnlyList<Profile> profiles = ProfileManager.Profiles;
                for (int i = 0; i < profiles.Count; i++)
                {
                    if (profiles[i] == profile)
                    {
                        SelectedProfile = i + 2;
                        break;
                    }
                }

                m_state = State.Select;
                Profile = profile;
                SelectCharacter(m_selectedCharacter);
            }
        }

        private void SetProfiles()
        {
            IReadOnlyList<Profile> profiles = ProfileManager.Profiles;

            for (int i = 0; i < m_profilePanels.Count; i++)
            {
                int profileIndex = i - 2 + m_profileWindow;
                Profile profile = 0 <= profileIndex && profileIndex < profiles.Count ? profiles[profileIndex] : null;

                //ProfilePanel.Mode mode = ProfilePanel.Mode.Profile;
                //if (m_profileWindow + i == 0)
                //{
                //    mode = ProfilePanel.Mode.Guest;
                //}
                //else if (m_profileWindow + i == 1)
                //{
                //    mode = ProfilePanel.Mode.AddNew;
                //}

                //m_profilePanels[i].SetProfile(profile, mode, null, null);
                //m_profilePanels[i].UpdateGraphics((i + m_profileWindow) == SelectedProfile, m_selectMenu.PlayerProfiles.Contains(profile));
            }
        }

        public void UpdateGraphics(MainMenu menu)
        {
            m_joinTab.SetActive(m_state == State.Join);
            m_profileTab.SetActive(m_state == State.Profile);
            m_characterSelectTab.SetActive(m_state == State.Select || m_state == State.Ready);
            m_readyText.gameObject.SetActive(m_state == State.Ready);

            SetProfiles();

            Color playerCol = Color.Lerp(Consts.GetRacerColor(m_playerNum), Color.white, 0.35f);
            
            m_playerName.gameObject.SetActive(Profile != null);
            if (Profile != null)
            {
                //UIUtils.FitText(m_playerName, Profile.Name);
                m_playerName.color = playerCol;
            }

            Color bgCol = Color.Lerp(m_previewBgColor, playerCol, m_previewBgColorFac);
            m_previewCam.backgroundColor = bgCol;
            
            switch (m_state)
            {
                case State.Join:
                    m_input = null;
                    //m_joinControls.UpdateUI("Join", menu.UnreservedInputs.SelectMany(i => i.SpriteAccept).ToList());

                    m_controls3.SetActive(false);
                    m_controls2.SetActive(false);
                    m_controls1.SetActive(true);
                    //m_controls1.UpdateUI("Back", menu.UnreservedInputs.SelectMany(i => i.SpriteCancel).ToList());
                    break;
                case State.Profile:
                    //m_controls3.SetActive(true);
                    //m_controls3.UpdateUI("Next Profile", m_input.SpriteDownUp);
                    //m_controls2.SetActive(true);
                    //m_controls2.UpdateUI("Accept", m_input.SpriteAccept);
                    //m_controls1.SetActive(true);
                    //m_controls1.UpdateUI("Leave", m_input.SpriteCancel);
                    break;
                case State.Select:
                    //m_controls3.SetActive(true);
                    //m_controls3.UpdateUI("Next Character", m_input.SpriteLeftRight);
                    //m_controls2.SetActive(true);
                    //m_controls2.UpdateUI("Accept", m_input.SpriteAccept);
                    //m_controls1.SetActive(true);
                    //m_controls1.UpdateUI("Back", m_input.SpriteCancel);
                    break;
                case State.Ready:
                    //m_controls3.SetActive(false);
                    //m_controls2.SetActive(false);
                    //m_controls1.SetActive(true);
                    //m_controls1.UpdateUI("Cancel", m_input.SpriteCancel);
                    break;
            }

            Character config = Character;

            SetCameraActive(m_characterSelectTab.activeInHierarchy);

            if (m_previewCam.enabled)
            {
                //ColorGrading colorGrading = m_previewCam.GetComponent<PostProcessLayer>

                //ColorGradingModel.Settings settings = m_post.colorGrading.settings;
                //ColorGradingModel.BasicSettings basic = settings.basic;
                //basic.saturation = IsReady ? 0.275f : 1f;
                //settings.basic = basic;
                //m_post.colorGrading.settings = settings;

                GameObject previewObject = null;
                if (m_configToPreview.TryGetValue(config, out previewObject))
                {
                    foreach (GameObject go in m_previewObjects)
                    {
                        go.SetActive(go == previewObject);
                    }
                    float timeSinceSelect = Time.unscaledTime - m_selectTime;
                    float rotSpeed = Mathf.Lerp(0, m_rotateSpeed, 1f - (0.5f * Mathf.Cos(Mathf.PI * Mathf.Clamp01((timeSinceSelect - m_rotateWait) / 4.0f)) + 0.5f));

                    previewObject.transform.rotation *= Quaternion.Euler(0, rotSpeed * Time.unscaledDeltaTime, 0);
                }

                RectTransform previewRT = m_characterPreview.GetComponent<RectTransform>();

                int width = Mathf.RoundToInt(m_resolutionScale * previewRT.rect.width);
                int height = Mathf.RoundToInt(m_resolutionScale * previewRT.rect.height);
                if (m_previewTex == null || width != m_previewTex.width || height != m_previewTex.height)
                {
                    FreeTexture();
                    CreateTexture();
                }
            }

            m_characterName.text = config.Meta.Name;
            m_characterHighlight.color = new Color(1f, 1f, 1f, Mathf.Lerp(m_characterHighlight.color.a, 0f, Time.unscaledDeltaTime /  0.035f));

            m_characterStats.SetActive(m_state == State.Select);
            if (m_characterStats.activeInHierarchy)
            {
                SetRating(m_characterSpeed, config.Meta.SpeedRating);
                SetRating(m_characterAgility, config.Meta.AgilityRating);
                m_characterDesc.text = config.Meta.Description;
            }
        }

        private void SetRating(Text text, int rating)
        {
            string str = "";
            for (int i = 0; i < rating; i++)
            {
                str += '-';
            }
            text.text = str;
            text.color = m_ratingGradient.Evaluate(rating / 10.0f);
        }

        private void CreateTexture()
        {
            RectTransform previewRT = m_characterPreview.GetComponent<RectTransform>();

            int width = Mathf.RoundToInt(m_resolutionScale * previewRT.rect.width);
            int height = Mathf.RoundToInt(m_resolutionScale * previewRT.rect.height);

            if (width > 0 && height > 0)
            {
                m_previewTex = new RenderTexture(width, height, 24);
                m_previewCam.targetTexture = m_previewTex;
                m_characterPreview.texture = m_previewTex;
            }
        }

        private void FreeTexture()
        {
            if (m_previewTex != null)
            {
                m_previewTex.Release();
                m_previewTex = null;

                if (m_previewCam != null)
                {
                    m_previewCam.targetTexture = null;
                }
            }
        }

        private void SelectCharacter(int index)
        {
            int configs = CharacterManager.Characters.Length;

            m_selectedCharacter = (index + configs) % configs;
            m_selectTime = Time.unscaledTime;

            if (m_configToPreview.TryGetValue(Character, out GameObject previewObject))
            {
                previewObject.transform.rotation = Quaternion.identity;
            }
        }
    }
}
