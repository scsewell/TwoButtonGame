using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.PostProcessing;

public class PlayerSelectPanel : MonoBehaviour
{
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
    
    private State m_state;
    private bool m_continue;
    private PlayerConfig[] m_configs;
    private Camera m_previewCam;
    private RenderTexture m_previewTex;
    private int m_previewLayer;
    private Dictionary<PlayerConfig, GameObject> m_configToPreview = new Dictionary<PlayerConfig, GameObject>();
    private List<GameObject> m_previewObjects = new List<GameObject>();
    private float m_selectTime;

    private int m_selectedConfig;
    public int SelectedConfig { get { return m_selectedConfig; } }

    private PlayerInput m_input;
    public PlayerInput Input { get { return m_input; } }

    public bool IsJoined { get { return m_state != State.Join; } }
    public bool IsReady { get { return m_state == State.Ready; } }
    public bool CanContinue { get { return m_state == State.Ready || m_state == State.Join; } }
    public bool Continue { get { return m_continue; } }
    
    private void OnDestroy()
    {
        FreeTexture();
    }

    public void Init(int index, PlayerConfig[] configs)
    {
        m_previewCam = new GameObject("PreviewCamera").AddComponent<Camera>();
        m_previewCam.transform.position = m_previewCamPos;
        m_previewCam.transform.rotation = Quaternion.Euler(m_previewCamRot);
        m_previewCam.fieldOfView = m_previewCamFov;
        m_previewCam.clearFlags = CameraClearFlags.SolidColor;
        m_previewCam.renderingPath = RenderingPath.Forward;
        m_previewCam.allowMSAA = false;
        m_previewCam.depth = -10;

        m_previewLayer = (index + 8);
        m_previewCam.cullingMask = (1 << m_previewLayer);

        m_previewCam.gameObject.AddComponent<PostProcessingBehaviour>().profile = m_cameraPost;

        m_configs = configs;
        m_input = InputManager.Instance.PlayerInputs[index];

        foreach (PlayerConfig config in m_configs)
        {
            GameObject previewObject = Instantiate(config.CharacterGraphics);
            previewObject.GetComponentsInChildren<Transform>(true).ToList().ForEach(r => r.gameObject.layer = m_previewLayer);
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
        }
        
        m_characterHighlight.color = new Color(1, 1, 1, 0);

        UpdateGraphics();
    }

    public void FromConfig(PlayerConfig selectedConfig)
    {
        m_state = State.Select;
        SelectCharacter(System.Array.IndexOf(m_configs, selectedConfig));
    }

    public void SetCameraActive(bool isActive)
    {
        m_previewCam.enabled = isActive;
    }
    
    public bool UpdatePanel(int playerNum, Menu menu)
    {
        m_continue = false;
        if (m_input.Button1Pressed)
        {
            switch (m_state)
            {
                case State.Join: m_state = State.Select; menu.PlaySubmitSound(); SelectCharacter(0); break;
                case State.Select: m_state = State.Ready; menu.PlaySubmitSound(); break;
                case State.Ready: if (playerNum == 0) { m_continue = true; } break;
            }
        }

        if (m_input.Button2Pressed)
        {
            switch (m_state)
            {
                case State.Select:
                    SelectCharacter(m_selectedConfig + 1);
                    m_characterHighlight.color = new Color(1, 1, 1, 0.5f);
                    menu.PlaySelectSound();
                    break;
            }
        }

        bool back = false;
        if (m_input.BothDown)
        {
            switch (m_state)
            {
                case State.Join: back = true; break;
                case State.Select: m_state = State.Join; menu.PlayCancelSound(); break;
                case State.Ready: m_state = State.Select; menu.PlayCancelSound(); break;
            }
        }

        Color playerCol = Color.Lerp(Consts.PLAYER_COLORS[playerNum], Color.white, 0.35f);

        Color bgCol = Color.Lerp(m_previewBgColor, playerCol, m_previewBgColorFac);
        m_previewCam.backgroundColor = bgCol;
        
        m_playerName.text = "Player " + (playerNum + 1);
        m_playerName.color = playerCol;

        UpdateGraphics();

        return back;
    }

    private void UpdateGraphics()
    {
        m_joinTab.SetActive(m_state == State.Join);
        m_characterSelectTab.SetActive(m_state != State.Join);
        m_playerName.gameObject.SetActive(m_state != State.Join);
        m_readyText.gameObject.SetActive(m_state == State.Ready);

        if (m_input != null)
        {
            m_joinControls.UpdateUI("Join", m_input.Button1.Name);

            switch (m_state)
            {
                case State.Join:
                    m_controls1.SetActive(true);
                    m_controls1.UpdateUI("Back", m_input.ButtonNames);
                    m_controls2.SetActive(false);
                    m_controls3.SetActive(false);
                    break;
                case State.Select:
                    m_controls1.SetActive(true);
                    m_controls1.UpdateUI("Leave", m_input.ButtonNames);
                    m_controls2.SetActive(true);
                    m_controls2.UpdateUI("Next", m_input.Button2.Name);
                    m_controls3.SetActive(true);
                    m_controls3.UpdateUI("Select", m_input.Button1.Name);
                    break;
                case State.Ready:
                    m_controls1.SetActive(true);
                    m_controls1.UpdateUI("Cancel", m_input.ButtonNames);
                    m_controls2.SetActive(false);
                    m_controls3.SetActive(false);
                    break;
            }
        }

        PlayerConfig config = m_configs[m_selectedConfig];

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
        m_characterHighlight.color = new Color(1, 1, 1, Mathf.Lerp(m_characterHighlight.color.a, 0, Time.unscaledDeltaTime * 24f));
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
        m_selectedConfig = index % m_configs.Length;
        m_selectTime = Time.unscaledTime;
        
        GameObject previewObject = null;
        if (m_configToPreview.TryGetValue(m_configs[m_selectedConfig], out previewObject))
        {
            previewObject.transform.rotation = Quaternion.identity;
        }
    }
}
