using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.PostProcessing;

namespace BoostBlasters.Menus
{
    public class PlayerSelectPanel : MonoBehaviour
    {
        [SerializeField]
        private GameObject m_profileSelectPrefab;

        [Header("UI Elements")]
        [SerializeField] private GameObject m_joinTab;
        [SerializeField] private GameObject m_profileTab;
        [SerializeField] private GameObject m_characterSelectTab;
        [SerializeField] private Text m_playerName;
        [SerializeField] private Text m_readyText;
        [SerializeField] private RectTransform m_profileContent;
        [SerializeField] private ControlPanel m_joinControls;
        [SerializeField] private ControlPanel m_controls1;
        [SerializeField] private ControlPanel m_controls2;
        [SerializeField] private ControlPanel m_controls3;
        [SerializeField] private RawImage m_characterPreview;
        [SerializeField] private Text m_characterName;
        [SerializeField] private GameObject m_characterStats;
        [SerializeField] private Text m_characterSpeed;
        [SerializeField] private Text m_characterAgility;
        [SerializeField] private Text m_characterDesc;
        [SerializeField] private Image m_characterBackground;
        [SerializeField] private Image m_characterHighlight;

        [Header("Options")]
        [SerializeField]
        private int m_profilePanelCount = 12;
        [SerializeField]
        public PostProcessingProfile m_cameraPost;
        [SerializeField]
        private Vector3 m_previewCamPos = new Vector3(0, 1.5f, 2.5f);
        [SerializeField]
        private Vector3 m_previewCamRot = new Vector3(12, 180, 0);
        [SerializeField] [Range(0, 180)]
        private float m_previewCamFov = 50;
        [SerializeField] [Range(0.25f, 1)]
        private float m_resolutionScale = 1.0f;
        [SerializeField]
        private Color m_previewBgColor = new Color(0.05f, 0.05f, 0.05f);
        [SerializeField] [Range(0, 1)]
        private float m_previewBgColorFac = 0.1f;
        [SerializeField] [Range(0, 5)]
        private float m_rotateWait = 2.0f;
        [SerializeField] [Range(0, 180)]
        private float m_rotateSpeed = 30.0f;
        [SerializeField]
        private Gradient m_ratingGradient;

        public enum State
        {
            Join,
            Profile,
            Select,
            Ready,
        }

        private MainMenu m_menu;
        private PlayerSelectMenu m_selectMenu;
        private List<PlayerProfilePanel> m_profilePanels;
        private Camera m_previewCam;
        private RenderTexture m_previewTex;
        private PostProcessingProfile m_post;
        private Dictionary<PlayerConfig, GameObject> m_configToPreview;
        private List<GameObject> m_previewObjects;
        private State m_state;
        private int m_playerNum;
        private float m_selectTime;

        private PlayerBaseInput m_input;
        public PlayerBaseInput Input { get { return m_input; } }

        private int m_profileWindow;
        private int m_selectedProfile;
        private PlayerProfile m_profile;

        private int SelectedProfile
        {
            get { return m_selectedProfile; }
            set
            {
                if (m_selectedProfile != value)
                {
                    m_selectedProfile = value;
                    
                    int centeringWindow = m_selectedProfile - ((m_profilePanelCount - 1) / 2);
                    int maxIndex = PlayerProfileManager.Instance.Profiles.Count + 2;
                    m_profileWindow = Mathf.Clamp(centeringWindow, 0, maxIndex - m_profilePanelCount);
                }
            }
        }

        public PlayerProfile Profile
        {
            get { return m_profile; }
            private set
            {
                if (m_profile != null && m_profile.IsGuest)
                {
                    PlayerProfileManager.Instance.ReleaseGuestProfile(m_profile);
                }
                m_profile = value;
            }
        }

        private int m_selectedCharacter;
        public PlayerConfig CharacterConfig { get { return Main.Instance.PlayerConfigs[m_selectedCharacter]; } }

        public bool IsJoined { get { return m_state != State.Join; } }
        public bool IsReady { get { return m_state == State.Ready; } }
        public bool CanContinue { get { return m_state == State.Ready || m_state == State.Join; } }
        
        private bool m_continue;
        public bool Continue { get { return m_continue; } }

        private void OnDestroy()
        {
            FreeTexture();
        }

        public void Init(PlayerSelectMenu selectMenu, int index)
        {
            m_menu = (MainMenu)selectMenu.Menu;
            m_selectMenu = selectMenu;
            m_playerNum = index;

            m_profilePanels = new List<PlayerProfilePanel>();

            for (int i = 0; i < m_profilePanelCount; i++)
            {
                m_profilePanels.Add(Instantiate(m_profileSelectPrefab, m_profileContent).AddComponent<PlayerProfilePanel>());
            }

            m_previewCam = new GameObject("PreviewCamera").AddComponent<Camera>();
            m_previewCam.transform.position = m_previewCamPos;
            m_previewCam.transform.rotation = Quaternion.Euler(m_previewCamRot);
            m_previewCam.fieldOfView = m_previewCamFov;
            m_previewCam.clearFlags = CameraClearFlags.SolidColor;
            m_previewCam.renderingPath = RenderingPath.Forward;
            m_previewCam.allowMSAA = false;
            m_previewCam.depth = -10;

            int previewLayer = (index + 8);
            m_previewCam.cullingMask = (1 << previewLayer);

            m_post = Instantiate(m_cameraPost);
            m_previewCam.gameObject.AddComponent<PostProcessingBehaviour>().profile = m_post;
        
            m_configToPreview = new Dictionary<PlayerConfig, GameObject>();
            m_previewObjects = new List<GameObject>();

            foreach (PlayerConfig config in Main.Instance.PlayerConfigs)
            {
                GameObject pivot = new GameObject("CharacterPivot");

                GameObject previewObject = Instantiate(config.CharacterGraphics, pivot.transform);
                previewObject.transform.localPosition = config.GraphicsOffset + Vector3.up;
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

            m_characterHighlight.color = new Color(1, 1, 1, 0);
        }

        public void FromConfig(PlayerProfile profile, PlayerBaseInput input, PlayerConfig selectedConfig)
        {
            m_state = State.Ready;
            Profile = profile;
            m_input = input;
            SelectCharacter(Array.IndexOf(Main.Instance.PlayerConfigs, selectedConfig));
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
            m_continue = false;

            if (m_state == State.Join)
            {
                Profile = null;
                SelectedProfile = 0;

                foreach (PlayerBaseInput input in m_menu.UnreservedInputs)
                {
                    if (input.UI_Accept)
                    {
                        m_state = State.Profile;
                        m_input = input;
                        m_menu.PlaySubmitSound();
                        break;
                    }
                    else if (input.UI_Cancel)
                    {
                        m_menu.SetMenu(m_menu.Root, MenuBase.TransitionSound.Back);
                    }
                }
            }
            else if (m_state == State.Profile)
            {
                Profile = null;

                if (m_input.UI_Accept)
                {
                    if (SelectedProfile == 0)
                    {
                        m_state = State.Select;
                        Profile = PlayerProfileManager.Instance.GetGuestProfile("Guest", true);
                        SelectCharacter(m_selectedCharacter);
                        m_menu.PlaySubmitSound();
                    }
                    else if (SelectedProfile == 1)
                    {
                        m_menu.ProfileName.EditProfile(PlayerProfileManager.Instance.AddNewProfile(), true, m_menu.PlayerSelect, OnProfileCreate);
                    }
                    else
                    {
                        PlayerProfile profile = PlayerProfileManager.Instance.Profiles[SelectedProfile - 2];

                        if (!m_selectMenu.PlayerProfiles.Contains(profile))
                        {
                            m_state = State.Select;
                            Profile = profile;
                            SelectCharacter(m_selectedCharacter);
                            m_menu.PlaySubmitSound();
                        }
                        else
                        {
                            m_menu.PlayCancelSound();
                        }
                    }
                }
                else if (m_input.UI_Down || m_input.UI_Up)
                {
                    int previous = SelectedProfile;
                    int maxIndex = PlayerProfileManager.Instance.Profiles.Count + 1;

                    SelectedProfile = Mathf.Clamp(SelectedProfile + (m_input.UI_Down ? 1 : -1), 0, maxIndex);
                    
                    if (previous != SelectedProfile)
                    {
                        m_menu.PlaySelectSound();
                    }
                }
                else if (m_input.UI_Cancel)
                {
                    m_state = State.Join;
                    m_menu.PlayCancelSound();
                }
            }
            else if (m_state == State.Select)
            {
                if (m_input.UI_Accept)
                {
                    m_state = State.Ready;
                    m_menu.PlaySubmitSound();
                }
                else if (m_input.UI_Right || m_input.UI_Left)
                {
                    SelectCharacter(m_selectedCharacter + (m_input.UI_Right ? 1 : -1));
                    m_characterHighlight.color = new Color(1, 1, 1, 0.35f);
                    m_menu.PlaySelectSound();
                }
                else if (m_input.UI_Cancel)
                {
                    m_state = State.Profile;
                    Profile = null;
                    m_menu.PlayCancelSound();
                }
            }
            else if (m_state == State.Ready)
            {
                if (m_input.UI_Accept)
                {
                    m_continue = true;
                }
                else if (m_input.UI_Cancel)
                {
                    m_state = State.Select;
                    m_menu.PlayCancelSound();
                }
            }
        }

        private void OnProfileCreate(PlayerProfile profile)
        {
            if (profile != null)
            {
                IReadOnlyList<PlayerProfile> profiles = PlayerProfileManager.Instance.Profiles;
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
            IReadOnlyList<PlayerProfile> profiles = PlayerProfileManager.Instance.Profiles;

            for (int i = 0; i < m_profilePanels.Count; i++)
            {
                int profileIndex = i - 2 + m_profileWindow;
                PlayerProfile profile = 0 <= profileIndex && profileIndex < profiles.Count ? profiles[profileIndex] : null;
                
                PlayerProfilePanel.Mode mode = PlayerProfilePanel.Mode.Profile;
                if (m_profileWindow + i == 0)
                {
                    mode = PlayerProfilePanel.Mode.Guest;
                }
                else if (m_profileWindow + i == 1)
                {
                    mode = PlayerProfilePanel.Mode.AddNew;
                }

                m_profilePanels[i].SetProfile(profile, mode, null, null);
                m_profilePanels[i].UpdateGraphics((i + m_profileWindow) == SelectedProfile, m_selectMenu.PlayerProfiles.Contains(profile));
            }
        }

        public void UpdateGraphics(MainMenu menu)
        {
            m_joinTab.SetActive(m_state == State.Join);
            m_profileTab.SetActive(m_state == State.Profile);
            m_characterSelectTab.SetActive(m_state == State.Select || m_state == State.Ready);
            m_readyText.gameObject.SetActive(m_state == State.Ready);

            SetProfiles();

            Color playerCol = Color.Lerp(Consts.PLAYER_COLORS[m_playerNum], Color.white, 0.35f);
            
            m_playerName.gameObject.SetActive(Profile != null);
            if (Profile != null)
            {
                UIUtils.FitText(m_playerName, Profile.Name);
                m_playerName.color = playerCol;
            }

            Color bgCol = Color.Lerp(m_previewBgColor, playerCol, m_previewBgColorFac);
            m_previewCam.backgroundColor = bgCol;
            
            switch (m_state)
            {
                case State.Join:
                    m_input = null;
                    m_joinControls.UpdateUI("Join", menu.UnreservedInputs.SelectMany(i => i.SpriteAccept).ToList());

                    m_controls3.SetActive(false);
                    m_controls2.SetActive(false);
                    m_controls1.SetActive(true);
                    m_controls1.UpdateUI("Back", menu.UnreservedInputs.SelectMany(i => i.SpriteCancel).ToList());
                    break;
                case State.Profile:
                    m_controls3.SetActive(true);
                    m_controls3.UpdateUI("Next Profile", m_input.SpriteDownUp);
                    m_controls2.SetActive(true);
                    m_controls2.UpdateUI("Accept", m_input.SpriteAccept);
                    m_controls1.SetActive(true);
                    m_controls1.UpdateUI("Leave", m_input.SpriteCancel);
                    break;
                case State.Select:
                    m_controls3.SetActive(true);
                    m_controls3.UpdateUI("Next Character", m_input.SpriteLeftRight);
                    m_controls2.SetActive(true);
                    m_controls2.UpdateUI("Accept", m_input.SpriteAccept);
                    m_controls1.SetActive(true);
                    m_controls1.UpdateUI("Back", m_input.SpriteCancel);
                    break;
                case State.Ready:
                    m_controls3.SetActive(false);
                    m_controls2.SetActive(false);
                    m_controls1.SetActive(true);
                    m_controls1.UpdateUI("Cancel", m_input.SpriteCancel);
                    break;
            }

            PlayerConfig config = CharacterConfig;

            SetCameraActive(m_characterSelectTab.activeInHierarchy);

            if (m_previewCam.enabled)
            {
                ColorGradingModel.Settings settings = m_post.colorGrading.settings;
                ColorGradingModel.BasicSettings basic = settings.basic;
                basic.saturation = IsReady ? 0.275f : 1f;
                settings.basic = basic;
                m_post.colorGrading.settings = settings;

                GameObject previewObject = null;
                if (m_configToPreview.TryGetValue(config, out previewObject))
                {
                    foreach (GameObject go in m_previewObjects)
                    {
                        go.SetActive(go == previewObject);
                    }
                    float timeSinceSelect = Time.unscaledTime - m_selectTime;
                    float rotSpeed = Mathf.Lerp(0, m_rotateSpeed, 1 - (0.5f * Mathf.Cos(Mathf.PI * Mathf.Clamp01((timeSinceSelect - m_rotateWait) / 4.0f)) + 0.5f));

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

            m_characterName.text = config.Name;
            m_characterHighlight.color = new Color(1, 1, 1, Mathf.Lerp(m_characterHighlight.color.a, 0, Time.unscaledDeltaTime /  0.035f));

            m_characterStats.SetActive(m_state == State.Select);
            if (m_characterStats.activeInHierarchy)
            {
                SetRating(m_characterSpeed, config.SpeedRating);
                SetRating(m_characterAgility, config.AgilityRating);
                m_characterDesc.text = config.Description;
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
            int configs = Main.Instance.PlayerConfigs.Length;

            m_selectedCharacter = (index + configs) % configs;
            m_selectTime = Time.unscaledTime;
        
            GameObject previewObject = null;
            if (m_configToPreview.TryGetValue(CharacterConfig, out previewObject))
            {
                previewObject.transform.rotation = Quaternion.identity;
            }
        }
    }
}
