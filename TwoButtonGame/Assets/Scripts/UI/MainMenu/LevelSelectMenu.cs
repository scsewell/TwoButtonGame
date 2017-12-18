using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.PostProcessing;
using Framework.UI;

namespace BoostBlasters.MainMenus
{
    public class LevelSelectMenu : MenuScreen
    {
        [SerializeField] private Navigable m_optionPrefab;

        [Header("UI Elements")]
        [SerializeField] private RectTransform m_optionContent;
        [SerializeField] private Button m_startRaceButton;
        [SerializeField] private Button m_backButton;
        [SerializeField] private Text m_levelDifficulty;
        [SerializeField] private Image m_levelPreview;
        [SerializeField] private Image m_levelHighlight;
        [SerializeField] private RectTransform m_track3dPreview;

        [Header("Options")]
        [SerializeField] [Range(1, 10)]
        private int m_maxLapCount = 5;
        [SerializeField] [Range(1, 10)]
        private int m_defaultLapCount = 3;
        [SerializeField] [Range(0, Consts.MAX_PLAYERS)]
        private int m_defaultAICount = 0;

        [Header("Track Preview")]
        [SerializeField]
        public PostProcessingProfile m_cameraPost;
        [SerializeField]
        private Vector3 m_previewCamPos = new Vector3(0, 1.5f, 2.5f);
        [SerializeField]
        [Range(-2f, 2f)]
        private float m_lookHeight = -0.25f;
        [SerializeField]
        [Range(0, 90f)]
        private float m_rotateSpeed = 15.0f;
        [SerializeField]
        [Range(0, 1)]
        private float m_bobHeight = 1.0f;
        [SerializeField]
        [Range(0, 1)]
        private float m_bobFrequency = 0.113f;
        [SerializeField]
        [Range(0, 180)]
        private float m_previewCamFov = 50;
        [SerializeField]
        private Color m_previewBgColor = new Color(0.05f, 0.05f, 0.05f);

        private Option<LevelConfig> m_trackSelect;
        public Option<LevelConfig> TrackSelect { get { return m_trackSelect; } }
        
        private Option<int> m_lapSelect;
        public Option<int> LapSelect { get { return m_lapSelect; } }

        private Option<int> m_aiCountSelect;
        public Option<int> AICountSelect { get { return m_aiCountSelect; } }

        private List<Navigable> m_options;
        private Transform m_camPivot;
        private Camera m_previewCam;
        private RenderTexture m_previewTex;
        private PostProcessingProfile m_post;
        private Dictionary<LevelConfig, GameObject> m_configToPreview;
        
        public override void InitMenu(RaceParameters lastRace)
        {
            m_startRaceButton.onClick.AddListener(() => MainMenu.LaunchRace());
            m_backButton.onClick.AddListener(() => OnBack());
            
            m_options = new List<Navigable>();
            m_trackSelect   = new Option<LevelConfig>(m_options, m_optionPrefab, m_optionContent, "Track", Main.Instance.LevelConfigs, OnLevelChange);
            m_lapSelect     = new Option<int>(m_options, m_optionPrefab, m_optionContent, "Laps", Enumerable.Range(1, m_maxLapCount).ToArray(), null);
            m_aiCountSelect = new Option<int>(m_options, m_optionPrefab, m_optionContent, "AI Racers", Enumerable.Range(0, Consts.MAX_PLAYERS + 1).ToArray(), null);

            if (lastRace != null)
            {
                m_trackSelect.SetValue(lastRace.LevelConfig);
                m_lapSelect.SetValue(lastRace.Laps);
                m_aiCountSelect.SetValue(lastRace.AICount);
            }
            
            UIHelper.SetNavigationVertical(m_optionContent, null, m_startRaceButton, null, null);

            DefaultSelectionOverride = m_trackSelect.Selectable;

            m_camPivot = new GameObject("CameraPivot").transform;
            m_previewCam = new GameObject("PreviewCamera").AddComponent<Camera>();
            m_previewCam.transform.SetParent(m_camPivot, false);
            m_previewCam.clearFlags = CameraClearFlags.Nothing;
            m_previewCam.renderingPath = RenderingPath.Forward;
            m_previewCam.allowMSAA = true;
            m_previewCam.depth = 10;
            m_previewCam.cullingMask = 1;

            m_post = Instantiate(m_cameraPost);
            m_previewCam.gameObject.AddComponent<PostProcessingBehaviour>().profile = m_post;

            m_configToPreview = new Dictionary<LevelConfig, GameObject>();

            foreach (LevelConfig config in Main.Instance.LevelConfigs)
            {
                if (config.Preview3d != null)
                {
                    GameObject pivot = new GameObject("Preview " + config.Name);
                    Instantiate(config.Preview3d, pivot.transform);

                    m_configToPreview.Add(config, pivot);
                }
            }
        }

        protected override void OnEnableMenu()
        {
            if (m_previewCam != null)
            {
                m_previewCam.enabled = true;
            }
        }

        protected override void OnDisableMenu()
        {
            if (m_previewCam != null)
            {
                m_previewCam.enabled = false;
            }
        }

        protected override void OnResetMenu(bool fullReset)
        {
            m_aiCountSelect.SetMaxIndex(Consts.MAX_PLAYERS - MainMenu.ReservedInputs.Count);

            if (fullReset)
            {
                m_trackSelect.SetValue(Main.Instance.LevelConfigs.First());
                m_lapSelect.SetValue(m_defaultLapCount);
                m_aiCountSelect.SetValue(m_defaultAICount);
            }
            m_levelHighlight.color = new Color(1, 1, 1, 0);
        }

        protected override void OnUpdate()
        {
            m_camPivot.Rotate(Vector3.up, m_rotateSpeed * Time.deltaTime);
        }

        protected override void OnUpdateGraphics()
        {
            m_options.ForEach(o => o.UpdateGraphics());

            LevelConfig config = m_trackSelect.Value;
            m_levelDifficulty.text = config.LevelDifficulty.ToString();
            m_levelPreview.sprite = config.Preview;

            m_levelHighlight.color = new Color(1, 1, 1, Mathf.Lerp(m_levelHighlight.color.a, 0, Time.unscaledDeltaTime / 0.035f));

            if (m_previewCam.enabled)
            {
                m_previewCam.transform.localPosition = m_previewCamPos + (Vector3.up * m_bobHeight * Mathf.Sin(Time.unscaledTime * m_bobFrequency * (2 * Mathf.PI)));
                m_previewCam.transform.rotation = Quaternion.LookRotation((Vector3.up * m_lookHeight) - m_previewCam.transform.position);
                m_previewCam.fieldOfView = m_previewCamFov;
                m_previewCam.backgroundColor = m_previewBgColor;

                Vector3[] corners = new Vector3[4];
                m_track3dPreview.GetWorldCorners(corners);

                for (int i = 0; i < corners.Length; i++)
                {
                    corners[i] = Camera.main.WorldToViewportPoint(corners[i]);
                }
                Vector3 size = corners[2] - corners[0];
                m_previewCam.rect = new Rect(corners[0].x, corners[0].y, size.x, size.y);

                GameObject previewObject;
                m_configToPreview.TryGetValue(TrackSelect.Value, out previewObject);
                foreach (GameObject go in m_configToPreview.Values)
                {
                    go.SetActive(go == previewObject);
                }
            }
        }

        private void OnLevelChange()
        {
            m_levelHighlight.color = new Color(1, 1, 1, 0.35f);
            m_camPivot.rotation = Quaternion.identity;
        }

        public class Option<T>
        {
            private Navigable m_navigable;
            public Selectable Selectable { get { return m_navigable.GetComponentInChildren<Selectable>(); } }

            private T[] m_values;
            public T Value { get { return m_values[m_navigable.Index]; } }

            public Option(List<Navigable> options, Navigable prefab, Transform parent, string name, T[] values, Action onChange)
            {
                m_values = values;
                m_navigable = Instantiate(prefab, parent).Init(name, m_values.Length - 1, (i) => m_values[i].ToString(), onChange);
                options.Add(m_navigable);
            }

            public void SetValue(T value)
            {
                m_navigable.Index = Array.IndexOf(m_values, value);
            }

            public void SetMaxIndex(int maxIndex)
            {
                m_navigable.SetMaxIndex(maxIndex);
            }
        }
    }
}
