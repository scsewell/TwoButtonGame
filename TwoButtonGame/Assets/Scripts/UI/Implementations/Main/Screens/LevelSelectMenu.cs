using System;
using System.Collections.Generic;
using System.Linq;

using BoostBlasters.Characters;
using BoostBlasters.Levels;
using BoostBlasters.Profiles;
using BoostBlasters.Races;

using Framework;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace BoostBlasters.UI.MainMenu
{
    public class LevelSelectMenu : MenuScreen
    {
        [Header("Prefabs")]

        [SerializeField] private GameObject m_levelPreviewCameraPrefab = null;

        [Header("UI Elements")]

        [SerializeField] private Spinner m_level = null;
        [SerializeField] private Spinner m_lapCount = null;
        [SerializeField] private Spinner m_aiCount = null;
        [SerializeField] private Button m_startRace = null;
        [SerializeField] private TextMeshProUGUI m_levelTitle = null;
        [SerializeField] private Image m_levelPreview = null;
        [SerializeField] private Image m_levelHighlight = null;
        [SerializeField] private RectTransform m_track3dPreview = null;
        [SerializeField] private RectTransform m_resultsContent = null;

        [Header("Options")]

        [SerializeField]
        [Range(1, Consts.MAX_LAP_COUNT)]
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


        private PlayerRacerConfig[] m_players;
        private List<PlayerResultPanel> m_playerResults;
        private Transform m_camPivot;
        private Camera m_camera;
        private float m_levelSelectTime;

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
            m_level.Options = LevelManager.Levels.Select(level => level.Name).ToArray();
            m_level.ValueChanged += OnLevelChange;

            m_lapCount.Options = Enumerable.Range(1, m_maxLapCount).Select(i => i.ToString()).ToArray();

            m_startRace.onClick.AddListener(StartRace);

            m_playerResults = new List<PlayerResultPanel>();
            var resultsTemplate = m_resultsContent.GetComponentInChildren<PlayerResultPanel>();
            m_playerResults.Add(resultsTemplate);

            for (var i = 0; i < m_topScoreCount - 1; i++)
            {
                m_playerResults.Add(Instantiate(resultsTemplate, m_resultsContent));
            }

            // create the camera used to preview the selected level
            m_camPivot = Instantiate(m_levelPreviewCameraPrefab).transform;
            m_camera = m_camPivot.GetComponentInChildren<Camera>();
            m_camera.cullingMask = 1;

            // load the level configurations
            m_levelInfo.Clear();

            foreach (var level in LevelManager.Levels)
            {
                var preview3d = new GameObject($"LevelPreview {level.Name}");
                if (level.Preview3d != null)
                {
                    Instantiate(level.Preview3d, preview3d.transform);
                }

                var results = new List<LevelInfo.Results>();
                foreach (var profile in Profile.AllProfiles)
                {
                    foreach (var result in profile.GetRaceResults(level))
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

        /// <summary>
        /// Opens this menu screen.
        /// </summary>
        /// <param name="players">The players in the race. Cannot be null.</param>
        /// <param name="sound">The menu transition sound to play.</param>
        public void Open(PlayerRacerConfig[] players, TransitionSound sound)
        {
            Menu.SwitchTo(this, sound);

            m_players = players;

            m_aiCount.Options = Enumerable.Range(0, (Consts.MAX_RACERS - m_players.Length) + 1)
                .Select(i => i.ToString())
                .ToArray();

            m_level.Index = 0;
            m_lapCount.Index = m_defaultLapCount - 1;
            m_aiCount.Index = 0;
        }

        /// <summary>
        /// Opens this menu screen.
        /// </summary>
        /// <param name="raceParams">The configuration to initialize from. Cannot be null.</param>
        /// <param name="sound">The menu transition sound to play.</param>
        public void Open(RaceParameters raceParams, TransitionSound sound)
        {
            Menu.SwitchTo(this, sound);

            m_players = raceParams.Racers
                .OfType<PlayerRacerConfig>()
                .ToArray();

            m_aiCount.Options = Enumerable.Range(0, Consts.MAX_RACERS - m_players.Length)
                .Select(i => i.ToString())
                .ToArray();

            m_level.Index = Array.IndexOf(LevelManager.Levels, raceParams.Level);
            m_lapCount.Index = raceParams.Laps - 1;
            m_aiCount.Index = raceParams.Racers.OfType<AIRacerConfig>().Count();
        }

        protected override void OnShow()
        {
            OnLevelChange(m_level.Index);

            m_camera.enabled = true;
            m_levelHighlight.color = new Color(1f, 1f, 1f, 0f);
            m_levelSelectTime = float.MinValue;
        }

        protected override void OnHide()
        {
            if (m_camera != null)
            {
                m_camera.enabled = false;
            }
        }

        protected override void OnUpdate()
        {
            m_camPivot.Rotate(Vector3.up, m_rotateSpeed * Time.deltaTime);
        }

        protected override void OnUpdateVisuals()
        {
            // fade out the highlight
            m_levelHighlight.color = new Color(1f, 1f, 1f, Mathf.Lerp(0.035f, 0f, (Time.unscaledTime - m_levelSelectTime) / 0.125f));

            // position the 3d preview camera
            m_camera.transform.localPosition = m_previewCamPos + (Vector3.up * m_bobHeight * Mathf.Sin(Time.unscaledTime * m_bobFrequency * (2f * Mathf.PI)));
            m_camera.transform.rotation = Quaternion.LookRotation((Vector3.up * m_lookHeight) - m_camera.transform.position);

            var corners = new Vector3[4];
            m_track3dPreview.GetWorldCorners(corners);

            for (var i = 0; i < corners.Length; i++)
            {
                corners[i] = Camera.main.WorldToViewportPoint(corners[i]);
            }
            var size = corners[2] - corners[0];
            m_camera.rect = new Rect(corners[0].x, corners[0].y, size.x, size.y);

            foreach (var i in m_levelInfo.Values)
            {
                // i.preview3d.SetActive(i.preview3d == info.preview3d);
            }
        }

        public override void Back()
        {
            Menu.Get<PlayerSelectMenu>().Open(CreateRaceParams(), TransitionSound.Back);
        }

        private void OnLevelChange(int index)
        {
            m_levelHighlight.color = new Color(1f, 1f, 1f, 0.35f);
            m_camPivot.rotation = Quaternion.identity;

            var level = LevelManager.Levels[index];

            m_levelSelectTime = Time.unscaledTime;
            m_levelTitle.text = level.Name;
            m_levelPreview.sprite = level.Preview;

            //LevelInfo info = m_levelInfo[level];

            //List<LevelInfo.Results> results = info.results.Where(r => r.result.LapTimes.Count == m_lapSelect.Value).ToList();
            //for (int i = 0; i < m_playerResults.Count; i++)
            //{
            //    if (i < results.Count)
            //    {
            //        m_playerResults[i].SetResults(results[i].profile, results[i].result, i + 1);
            //    }
            //    else
            //    {
            //        m_playerResults[i].SetResults(null, null, i + 1);
            //    }
            //}
        }

        private void StartRace()
        {
            (Menu as MainMenu).LaunchRace(CreateRaceParams());
        }

        private RaceParameters CreateRaceParams()
        {
            var level = LevelManager.Levels[m_level.Index];
            var laps = m_lapCount.Index + 1;

            var aiRacers = new List<RacerConfig>();
            for (var i = 0; i < m_aiCount.Index; i++)
            {
                var character = CharacterManager.Characters.PickRandom();
                var profile = new AIProfile("AI ");

                aiRacers.Add(new AIRacerConfig(character, profile));
            }

            var racers = m_players
                .Union(aiRacers)
                .ToArray();

            return new RaceParameters(level, laps, racers);
        }
    }
}
