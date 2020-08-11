using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

using Framework.UI;

using BoostBlasters.Profiles;
using BoostBlasters.Levels;
using BoostBlasters.Races;

namespace BoostBlasters.UI.MainMenus
{
    public class LevelSelectMenu : MenuScreen
    {
        [Header("Prefabs")]

        [SerializeField] private Navigable m_optionPrefab = null;
        [SerializeField] private GameObject m_levelPreviewCameraPrefab = null;

        [Header("UI Elements")]

        [SerializeField] private RectTransform m_optionContent = null;
        [SerializeField] private Button m_startRaceButton = null;
        [SerializeField] private Button m_backButton = null;
        [SerializeField] private RectTransform m_resultsContent = null;
        [SerializeField] private Image m_levelPreview = null;
        [SerializeField] private Image m_levelHighlight = null;
        [SerializeField] private RectTransform m_track3dPreview = null;

        [Header("Options")]

        [SerializeField]
        [Range(1, 10)]
        private int m_maxLapCount = 5;
        [SerializeField]
        [Range(1, 10)]
        private int m_defaultLapCount = 3;
        [SerializeField]
        [Range(0, Consts.MAX_RACERS)]
        private int m_defaultAICount = 0;
        [SerializeField]
        [Range(0, 10)]
        private int m_topScoreCount = 5;

        [Header("Track Preview")]

        [SerializeField]
        private Vector3 m_previewCamPos = new Vector3(0, 1.5f, 2.5f);
        [SerializeField]
        [Range(-2f, 2f)]
        private float m_lookHeight = -0.25f;
        [SerializeField]
        [Range(0f, 90f)]
        private float m_rotateSpeed = 15.0f;
        [SerializeField]
        [Range(0f, 1f)]
        private float m_bobHeight = 1.0f;
        [SerializeField]
        [Range(0f, 1f)]
        private float m_bobFrequency = 0.113f;

        private Option<Level> m_trackSelect;
        public Option<Level> TrackSelect => m_trackSelect;

        private Option<int> m_lapSelect;
        public Option<int> LapSelect => m_lapSelect;

        private Option<int> m_aiCountSelect;
        public Option<int> AICountSelect => m_aiCountSelect;

        private List<Navigable> m_options;
        private List<PlayerResultPanel> m_playerResults;
        private Transform m_camPivot;
        private Camera m_previewCam;

        public class LevelInfo
        {
            public GameObject preview3d;
            public List<Results> results;

            public class Results
            {
                public Profile profile;
                public RaceResult result;
            }
        }

        private readonly Dictionary<Level, LevelInfo> m_levelInfo = new Dictionary<Level, LevelInfo>();


        protected override void OnInitialize()
        {
            m_startRaceButton.onClick.AddListener(() => ((MainMenu)Menu).LaunchRace());
            m_backButton.onClick.AddListener(() => Back());
            
            m_options = new List<Navigable>();
            m_trackSelect   = new Option<Level>(m_options, m_optionPrefab, m_optionContent, "Track", LevelManager.Levels, OnLevelChange);
            m_lapSelect     = new Option<int>(m_options, m_optionPrefab, m_optionContent, "Laps", Enumerable.Range(1, m_maxLapCount).ToArray(), null);
            m_aiCountSelect = new Option<int>(m_options, m_optionPrefab, m_optionContent, "AI Racers", Enumerable.Range(0, Consts.MAX_RACERS + 1).ToArray(), null);

            RaceParameters lastRace = Main.Instance.LastRaceParams;
            if (lastRace != null)
            {
                m_trackSelect.SetValue(lastRace.Level);
                m_lapSelect.SetValue(lastRace.Laps);
                m_aiCountSelect.SetValue(lastRace.AICount);
            }
            
            UIHelper.SetNavigationVertical(new NavConfig() 
            { 
                parent = m_optionContent, 
                down = m_startRaceButton,
            });

            PrimarySelection.DefaultSelectionOverride = m_trackSelect.Selectable.gameObject;

            m_playerResults = new List<PlayerResultPanel>();
            PlayerResultPanel resultsTemplate = m_resultsContent.GetComponentInChildren<PlayerResultPanel>();
            m_playerResults.Add(resultsTemplate);

            for (int i = 0; i < m_topScoreCount - 1; i++)
            {
                m_playerResults.Add(Instantiate(resultsTemplate, m_resultsContent));
            }

            // create the camera used to preview the selected level
            m_camPivot = Instantiate(m_levelPreviewCameraPrefab).transform;
            m_previewCam = m_camPivot.GetComponentInChildren<Camera>();
            m_previewCam.cullingMask = 1;

            // load the level configurations
            m_levelInfo.Clear();

            foreach (Level level in LevelManager.Levels)
            {
                GameObject preview3d = new GameObject($"LevelPreview {level.Name}");
                if (level.Preview3d != null)
                {
                    Instantiate(level.Preview3d, preview3d.transform);
                }

                List<LevelInfo.Results> results = new List<LevelInfo.Results>();
                foreach (Profile profile in ProfileManager.Profiles)
                {
                    foreach (RaceResult result in profile.GetRaceResults(level))
                    {
                        results.Add(new LevelInfo.Results()
                        {
                            profile = profile,
                            result = result,
                        });
                    }
                }
                results = results.OrderBy(r => r.result.FinishTime).ToList();

                m_levelInfo.Add(level, new LevelInfo()
                {
                    preview3d = preview3d,
                    results = results,
                });
            }
        }

        protected override void OnShow()
        {
            if (m_previewCam != null)
            {
                m_previewCam.enabled = true;
            }
        }

        protected override void OnHide()
        {
            if (m_previewCam != null)
            {
                m_previewCam.enabled = false;
            }
        }

        //protected override void OnResetMenu(bool fullReset)
        //{
        //    m_aiCountSelect.SetMaxIndex(Consts.MAX_RACERS - ((MainMenu)Menu).ReservedInputs.Count);

        //    if (fullReset)
        //    {
        //        m_trackSelect.SetValue(LevelManager.Levels.FirstOrDefault());
        //        m_lapSelect.SetValue(m_defaultLapCount);
        //        m_aiCountSelect.SetValue(m_defaultAICount);
        //    }

        //    m_levelHighlight.color = new Color(1, 1, 1, 0);
        //}

        protected override void OnUpdate()
        {
            m_camPivot.Rotate(Vector3.up, m_rotateSpeed * Time.deltaTime);
        }

        protected override void OnUpdateVisuals()
        {
            m_options.ForEach(o => o.UpdateGraphics());

            Level level = m_trackSelect.Value;
            m_levelPreview.sprite = level.Preview;

            m_levelHighlight.color = new Color(1f, 1f, 1f, Mathf.Lerp(m_levelHighlight.color.a, 0f, Time.unscaledDeltaTime / 0.035f));

            LevelInfo info = m_levelInfo[level];

            List<LevelInfo.Results> results = info.results.Where(r => r.result.LapTimes.Count == m_lapSelect.Value).ToList();
            for (int i = 0; i < m_playerResults.Count; i++)
            {
                if (i < results.Count)
                {
                    m_playerResults[i].SetResults(results[i].profile, results[i].result, i + 1);
                }
                else
                {
                    m_playerResults[i].SetResults(null, null, i + 1);
                }
            }

            if (m_previewCam.enabled)
            {
                m_previewCam.transform.localPosition = m_previewCamPos + (Vector3.up * m_bobHeight * Mathf.Sin(Time.unscaledTime * m_bobFrequency * (2f * Mathf.PI)));
                m_previewCam.transform.rotation = Quaternion.LookRotation((Vector3.up * m_lookHeight) - m_previewCam.transform.position);

                Vector3[] corners = new Vector3[4];
                m_track3dPreview.GetWorldCorners(corners);

                for (int i = 0; i < corners.Length; i++)
                {
                    corners[i] = Camera.main.WorldToViewportPoint(corners[i]);
                }
                Vector3 size = corners[2] - corners[0];
                m_previewCam.rect = new Rect(corners[0].x, corners[0].y, size.x, size.y);

                foreach (var i in m_levelInfo.Values)
                {
                    i.preview3d.SetActive(i.preview3d == info.preview3d);
                }
            }
        }

        private void OnLevelChange()
        {
            m_levelHighlight.color = new Color(1f, 1f, 1f, 0.35f);
            m_camPivot.rotation = Quaternion.identity;
        }

        public class Option<T>
        {
            private Navigable m_navigable;
            public Selectable Selectable => m_navigable.GetComponentInChildren<Selectable>();

            private T[] m_values;
            public T Value => m_values[m_navigable.Index];

            public Option(List<Navigable> options, Navigable prefab, Transform parent, string name, T[] values, Action onChange)
            {
                m_values = values;
                m_navigable = Instantiate(prefab, parent).Init(name, m_values.Length - 1, (i) => (0 <= i && i < m_values.Length) ? m_values[i].ToString() : string.Empty, onChange);
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
