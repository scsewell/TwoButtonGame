using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.PostProcessing;

namespace BoostBlasters.MainMenus
{
    public class PlayerSelectPanel : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private GameObject m_joinTab;
        [SerializeField] private GameObject m_characterSelectTab;
        [SerializeField] private Text m_playerName;
        [SerializeField] private Text m_readyText;
        [SerializeField] private ControlPanel m_joinControls;
        [SerializeField] private ControlPanel m_controls1;
        [SerializeField] private ControlPanel m_controls2;
        [SerializeField] private ControlPanel m_controls3;
        [SerializeField] private RawImage m_characterPreview;
        [SerializeField] private Text m_characterName;
        [SerializeField] private Image m_characterNamePlate;
        [SerializeField] private Image m_characterBackground;
        [SerializeField] private Image m_characterHighlight;

        [Header("Options")]
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

        public enum State
        {
            Join,
            Select,
            Ready,
        }
    
        private Camera m_previewCam;
        private RenderTexture m_previewTex;
        private Dictionary<PlayerConfig, GameObject> m_configToPreview;
        private List<GameObject> m_previewObjects;
        private State m_state;
        private int m_playerNum;
        private float m_selectTime;
        private bool m_continue;

        private int m_selectedConfig;
        public PlayerConfig SelectedConfig { get { return Main.Instance.PlayerConfigs[m_selectedConfig]; } }

        private PlayerBaseInput m_input;
        public PlayerBaseInput Input { get { return m_input; } }

        public bool IsJoined { get { return m_state != State.Join; } }
        public bool IsReady { get { return m_state == State.Ready; } }
        public bool CanContinue { get { return m_state == State.Ready || m_state == State.Join; } }
        public bool Continue { get { return m_continue; } }

        private void OnDestroy()
        {
            FreeTexture();
        }

        public void Init(int index)
        {
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

            m_previewCam.gameObject.AddComponent<PostProcessingBehaviour>().profile = m_cameraPost;
        
            m_configToPreview = new Dictionary<PlayerConfig, GameObject>();
            m_previewObjects = new List<GameObject>();

            foreach (PlayerConfig config in Main.Instance.PlayerConfigs)
            {
                GameObject previewObject = Instantiate(config.CharacterGraphics);
                previewObject.GetComponentsInChildren<Transform>(true).ToList().ForEach(r => r.gameObject.layer = previewLayer);
                m_configToPreview.Add(config, previewObject);
                m_previewObjects.Add(previewObject);
            }

            ResetState(true);
        }

        public void ResetState(bool fullReset)
        {
            m_state = (!fullReset && m_state != State.Join) ? State.Select : State.Join;

            if (fullReset)
            {
                SelectCharacter(0);
                m_input = null;
            }
        
            m_characterHighlight.color = new Color(1, 1, 1, 0);
        }

        public void FromConfig(PlayerBaseInput input, PlayerConfig selectedConfig)
        {
            m_state = State.Select;
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
    
        public void UpdatePanel(int playerNum, MainMenu menu)
        {
            m_playerNum = playerNum;
            m_continue = false;

            if (m_state == State.Join)
            {
                foreach (PlayerBaseInput input in menu.UnreservedInputs)
                {
                    if (input.UI_Accept)
                    {
                        m_state = State.Select;
                        m_input = input;
                        SelectCharacter(0);
                        menu.PlaySubmitSound();
                        break;
                    }
                    else if (input.UI_Cancel)
                    {
                        menu.SetMenu(Menu.Root, true);
                    }
                }
            }
            else if (m_state == State.Select)
            {
                if (m_input.UI_Accept)
                {
                    m_state = State.Ready;
                    menu.PlaySubmitSound();
                }
                else if (m_input.UI_Right || m_input.UI_Left)
                {
                    SelectCharacter(m_selectedConfig + (m_input.UI_Right ? 1 : -1));
                    m_characterHighlight.color = new Color(1, 1, 1, 0.5f);
                    menu.PlaySelectSound();
                }
                else if (m_input.UI_Cancel)
                {
                    m_state = State.Join;
                    menu.PlayCancelSound();
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
                    menu.PlayCancelSound();
                }
            }
        }

        public void UpdateGraphics(MainMenu menu)
        {
            m_joinTab.SetActive(m_state == State.Join);
            m_characterSelectTab.SetActive(m_state != State.Join);
            m_playerName.gameObject.SetActive(m_state != State.Join);
            m_readyText.gameObject.SetActive(m_state == State.Ready);
        
            Color playerCol = Color.Lerp(Consts.PLAYER_COLORS[m_playerNum], Color.white, 0.35f);

            m_playerName.text = "Player " + (m_playerNum + 1);
            m_playerName.color = playerCol;

            Color bgCol = Color.Lerp(m_previewBgColor, playerCol, m_previewBgColorFac);
            m_previewCam.backgroundColor = bgCol;

            if (m_state == State.Join)
            {
                m_input = null;
                m_joinControls.UpdateUI("Join", menu.UnreservedInputs.SelectMany(i => i.SpriteAccept).ToList());

                m_controls1.SetActive(true);
                m_controls1.UpdateUI("Back", menu.UnreservedInputs.SelectMany(i => i.SpriteCancel).ToList());
                m_controls2.SetActive(false);
                m_controls3.SetActive(false);
            }
            else
            {
                switch (m_state)
                {
                    case State.Select:
                        m_controls1.SetActive(true);
                        m_controls1.UpdateUI("Leave", m_input.SpriteCancel);
                        m_controls2.SetActive(true);
                        m_controls2.UpdateUI("Next Character", m_input.SpriteLeftRight);
                        m_controls3.SetActive(true);
                        m_controls3.UpdateUI("Accept", m_input.SpriteAccept);
                        break;
                    case State.Ready:
                        m_controls1.SetActive(true);
                        m_controls1.UpdateUI("Cancel", m_input.SpriteCancel);
                        m_controls2.SetActive(false);
                        m_controls3.SetActive(false);
                        break;
                }
            }

            PlayerConfig config = SelectedConfig;

            SetCameraActive(m_state != State.Join);

            if (m_previewCam.enabled)
            {
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
        
            m_characterPreview.texture = m_previewTex;
            m_characterName.text = config.Name;
            m_characterHighlight.color = new Color(1, 1, 1, Mathf.Lerp(m_characterHighlight.color.a, 0, Time.unscaledDeltaTime /  0.05f));
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

            m_selectedConfig = (index + configs) % configs;
            m_selectTime = Time.unscaledTime;
        
            GameObject previewObject = null;
            if (m_configToPreview.TryGetValue(SelectedConfig, out previewObject))
            {
                previewObject.transform.rotation = Quaternion.identity;
            }
        }
    }
}
