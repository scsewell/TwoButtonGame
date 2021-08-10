using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

using BoostBlasters.Races;
using BoostBlasters.Races.Racers;
using BoostBlasters.Levels;

namespace BoostBlasters.UI.RaceMenus
{
    public class PlayerUI : MonoBehaviour
    {
        [Header("Player Indicator")]

        [SerializeField]
        private RectTransform m_playerIndicatorPrefab = null;
        [SerializeField]
        private RectTransform m_playerIndicatorParent = null;
        [SerializeField]
        private LayerMask m_indicatorBlockingLayers = Physics.DefaultRaycastLayers;
        [SerializeField]
        [Range(0f, 2f)]
        private float m_indicatorVerticalOffset = 0.85f;
        [SerializeField]
        [Range(0f, 50f)]
        private float m_indicatorNearFadeStart = 2.0f;
        [SerializeField]
        [Range(0f, 50f)]
        private float m_indicatorNearFadeEnd = 4.0f;
        [SerializeField]
        [Range(0f, 1000f)]
        private float m_indicatorFarFadeStart = 200.0f;
        [SerializeField]
        [Range(0f, 1000f)]
        private float m_indicatorFarFadeEnd = 250.0f;
        [SerializeField]
        [Range(0f, 2f)]
        private float m_indicatorMinSize = 0.5f;
        [SerializeField]
        [Range(0f, 2f)]
        private float m_indicatorMaxSize = 1.0f;
        [SerializeField]
        [Range(0f, 1f)]
        private float m_indicatorMaxAlpha = 1.0f;

        private Dictionary<Racer, RectTransform> m_playerToIndicators = null;

        [Header("Countdown")]

        [SerializeField]
        private Text m_countdownText = null;

        [Header("Time")]

        [SerializeField]
        private Text m_timerText = null;
        [SerializeField]
        private Text m_lapTimeText = null;

        [Header("Player")]

        [SerializeField]
        private Text m_playerText = null;

        [Header("Waypoint Arrow")]

        [SerializeField]
        private Transform m_arrowPrefab = null;
        [SerializeField]
        [Range(0, 1)]
        private float m_arrowPosition = 0.85f;
        [SerializeField]
        [Range(0, 1)]
        private float m_arrowScale = 0.1f;
        [SerializeField]
        [Range(0, 1)]
        private float m_arrowSmoothing = 0.2f;
        [SerializeField]
        [Range(0, 30)]
        private float m_arrowNextBlendDistance = 15.0f;

        private Transform m_arrow = null;

        [Header("Rank Text")]

        [SerializeField]
        private RectTransform m_rankRect = null;
        [SerializeField]
        private Text m_rankText = null;
        [SerializeField]
        private Text m_rankSubText = null;
        [SerializeField]
        private Text m_finalRankText = null;
        [SerializeField]
        private Text m_finalRankSubText = null;
        [SerializeField]
        private Color[] m_rankColors = null;
        [SerializeField]
        [Range(0.01f, 4)]
        private float m_finalRankFadeTime = 1.0f;

        private int m_lastRank;

        [Header("Lap Text")]

        [SerializeField]
        private Text m_lapText = null;
        [SerializeField]
        private Text m_newLapText = null;
        [SerializeField]
        [Range(0, 10)]
        private float m_newLapDuration = 2.5f;
        [SerializeField]
        [Range(0, 0.5f)]
        private float m_newLapSizeMagnitude = 0.5f;
        [SerializeField]
        [Range(1, 8)]
        private float m_newLapSizeFrequency = 4.0f;
        [SerializeField]
        [Range(0.01f, 1)]
        private float m_newLapAlphaSmoothing = 0.25f;

        private int m_lastLap = 0;
        private float m_lapChangeTime = 0f;

        [Header("Energy Bar")]

        [SerializeField]
        private GameObject m_energyBar = null;
        [SerializeField]
        private Image m_energyBarBorder = null;
        [SerializeField]
        private Image m_energyBarBack = null;
        [SerializeField]
        private Image m_energyBarFill = null;
        [SerializeField]
        private Image m_energyButton = null;
        [SerializeField]
        [Range(0f, 1f)]
        private float m_energyBackAlpha = 0.2f;
        [SerializeField]
        [Range(0f, 1f)]
        private float m_energyFailAlpha = 0.65f;
        [SerializeField]
        [Range(0f, 1f)]
        private float m_energyFailDuration = 0.65f;
        [SerializeField]
        [Range(0f, 1f)]
        private float m_energyFailFadeDuration = 0.25f;
        [SerializeField]
        [Range(0.5f, 20f)]
        private float m_energyFailFrequency = 5.0f;
        [SerializeField]
        private Image m_energyBarHighlight = null;
        [SerializeField]
        [Range(0.01f, 1f)]
        private float m_energyHighlightDuration = 0.35f;
        [SerializeField]
        [Range(0f, 1f)]
        private float m_energyFullIntensity = 0.65f;
        [SerializeField]
        [Range(0.5f, 20f)]
        private float m_energyFullFrequency = 5.0f;
        [SerializeField]
        [Range(0.01f, 1f)]
        private float m_energyCantBoostAlpha = 0.35f;

        private float m_energyGainTime = 0f;
        private float m_energyFailTime = 0f;

        private Racer m_racer = null;
        private RacerCamera m_camera = null;
        private RaceManager m_raceManager = null;

        private void Awake()
        {
            m_playerToIndicators = new Dictionary<Racer, RectTransform>();

            m_arrow = Instantiate(m_arrowPrefab);
        }

        public PlayerUI Init(Racer racer, RacerCamera cam, int humanCount)
        {
            m_racer = racer;
            m_camera = cam;

            m_raceManager = Main.Instance.RaceManager;

            m_arrow.gameObject.layer = cam.PlayerUILayer;

            RectTransform rt = GetComponent<RectTransform>();
            Rect splitscreen = RacerCamera.GetSplitscreen(racer.RacerNum, humanCount);

            rt.localScale = Vector3.one;
            rt.anchorMin = new Vector2(splitscreen.x, splitscreen.y);
            rt.anchorMax = new Vector2(splitscreen.x + splitscreen.width, splitscreen.y + splitscreen.height);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = Vector2.zero;

            m_playerText.text = racer.Config.Profile.Name;
            m_playerText.color = Color.Lerp(racer.Color, Color.white, 0.35f);

            //m_energyButton.sprite = input.SpriteBoost.First();

            m_racer.EnergyGained += Player_EnergyGained;
            m_racer.EnergyUseFailed += Player_EnergyUseFailed;

            ResetPlayerUI();

            return this;
        }

        public void ResetPlayerUI()
        {
            m_lastRank = 1;

            m_lastLap = 1;
            m_lapChangeTime = float.NegativeInfinity;

            m_energyGainTime = float.NegativeInfinity;
            m_energyFailTime = float.NegativeInfinity;

            SetAlpha(m_newLapText, 0);
            SetArrow(0);
        }

        private void OnDestroy()
        {
            if (m_arrow != null)
            {
                Destroy(m_arrow);
                m_arrow = null;
            }
            if (m_racer != null)
            {
                m_racer.EnergyGained -= Player_EnergyGained;
                m_racer.EnergyUseFailed -= Player_EnergyUseFailed;
            }
        }

        public void UpdateUI()
        {
            // start countdown
            /*
            float countdown = m_raceManager.CountdownTime;
            m_countdownText.gameObject.SetActive(-1 < countdown && countdown <= 3);

            if (m_countdownText.isActiveAndEnabled)
            {
                m_countdownText.text = (countdown > 0) ? Mathf.CeilToInt(countdown).ToString() : "GO!";
            }
            */
            
            int rank = m_racer.RaceResult.Rank;
            bool isSolo = m_raceManager.RacerCount == 1;

            int lap = m_racer.CurrentLap;
            RacePath path = m_raceManager.RacePath;
            bool finished = m_racer.RaceResult.Finished;

            m_arrow.gameObject.SetActive(!finished);
            m_rankText.gameObject.SetActive(!finished && !isSolo);
            m_rankSubText.gameObject.SetActive(!finished && !isSolo);
            m_newLapText.gameObject.SetActive(!finished);
            m_energyBar.SetActive(!finished);

            m_finalRankText.gameObject.SetActive(finished && !isSolo);
            m_finalRankSubText.gameObject.SetActive(finished && !isSolo);

            if (m_arrow.gameObject.activeInHierarchy)
            {
                SetArrow(m_arrowSmoothing);
            }

            foreach (Racer player in m_raceManager.Racers.OrderBy(p => Vector3.Distance(m_camera.transform.position, p.transform.position)))
            {
                if (player != m_racer)
                {
                    RectTransform indicator;
                    if (!m_playerToIndicators.TryGetValue(player, out indicator))
                    {
                        indicator = Instantiate(m_playerIndicatorPrefab, m_playerIndicatorParent);
                        indicator.SetAsFirstSibling();
                        indicator.pivot = new Vector2(0.5f, 0);

                        foreach (Graphic g in indicator.GetComponentsInChildren<Graphic>())
                        {
                            g.color = player.Color;
                        }

                        m_playerToIndicators.Add(player, indicator);
                    }

                    Vector3 camPos = m_camera.transform.position;
                    Vector3 indicatorPos = player.transform.position + m_indicatorVerticalOffset * Vector3.up;
                    Vector3 viewportPoint = m_camera.MainCam.WorldToViewportPoint(indicatorPos);

                    bool active = 0 < viewportPoint.z;

                    Vector3 inticatorDisp = indicatorPos - camPos;
                    float distance = inticatorDisp.magnitude;
                    Ray ray = new Ray(camPos, inticatorDisp);
                    RaycastHit hit;

                    if (active && !Physics.Raycast(ray, distance, m_indicatorBlockingLayers) && !m_arrow.GetComponent<Collider>().Raycast(ray, out hit, distance))
                    {
                        indicator.gameObject.SetActive(true);
                        indicator.SetAsFirstSibling();
                        indicator.anchoredPosition = Vector2.zero;
                        indicator.anchorMin = viewportPoint;
                        indicator.anchorMax = viewportPoint;

                        float farFade = Mathf.Clamp01((distance - m_indicatorFarFadeStart) / (m_indicatorFarFadeEnd - m_indicatorFarFadeStart));
                        float nearFade = Mathf.Clamp01((distance - m_indicatorNearFadeStart) / (m_indicatorNearFadeEnd - m_indicatorNearFadeStart));
                        float fade = Mathf.Min(nearFade, 1 - farFade);
                        SetAlpha(indicator.GetComponentInChildren<Graphic>(), Mathf.Lerp(0, m_indicatorMaxAlpha, fade));

                        float scaleFac = 1 - Mathf.Clamp01((distance - m_indicatorNearFadeEnd) / (m_indicatorFarFadeStart - m_indicatorNearFadeEnd));
                        float scale = Mathf.Lerp(m_indicatorMinSize, m_indicatorMaxSize, scaleFac);
                        indicator.localScale = scale * Vector3.one;
                    }
                    else
                    {
                        indicator.gameObject.SetActive(false);
                    }
                }
            }

            // set rank text
            Color rankColor = m_rankColors[Mathf.Min(rank - 1, m_rankColors.Length - 1)];
            SetColorNoAlpha(m_rankText, rankColor);
            SetColorNoAlpha(m_rankSubText, rankColor);
            SetColorNoAlpha(m_finalRankText, rankColor);
            SetColorNoAlpha(m_finalRankSubText, rankColor);

            SetAlpha(m_finalRankText, finished ? Mathf.MoveTowards(m_finalRankText.color.a, 1, Time.deltaTime / m_finalRankFadeTime) : 0);
            SetAlpha(m_finalRankSubText, m_finalRankText.color.a);

            m_rankText.text = rank.ToString();
            m_finalRankText.text = rank.ToString();

            string subText = "th";
            switch (rank)
            {
                case 1: subText = "st"; break;
                case 2: subText = "nd"; break;
                case 3: subText = "rd"; break;
            }

            m_rankSubText.text = subText;
            m_finalRankSubText.text = subText;

            // if (rank != m_lastRank && m_raceManager.GetStartRelativeTime() > 0)
            // {
            //     m_rankRect.localScale = 1.2f * Vector3.one;
            // }
            // else
            // {
            //     m_rankRect.localScale = Vector3.Lerp(m_rankRect.localScale, Vector3.one, Time.deltaTime * 3);
            // }

            // timer
            // float raceTime = finished ? m_racer.RaceResult.FinishTime : m_raceManager.GetStartRelativeTime(Time.time);
            //m_timerText.text = UIUtils.FormatRaceTime(raceTime);

            string lapTimes = "";
            if (path.Laps > 1)
            {
                foreach (float lapTime in m_racer.RaceResult.LapTimes)
                {
                    lapTimes += UIUtils.FormatRaceTime(lapTime) + '\n';
                }
                if (!finished && m_racer.RaceResult.LapTimes.Count > 0)
                {
                    //lapTimes += UIUtils.FormatRaceTime(raceTime - m_racer.RaceResult.LapTimes.Sum());
                }
            }
            m_lapTimeText.text = lapTimes;

            // set lap text
            m_lapText.text = "Lap " + lap + "/" + path.Laps;

            RectTransform newLapRT = m_newLapText.GetComponent<RectTransform>();
            if (lap != m_lastLap)
            {
                newLapRT.localScale = 1.2f * Vector3.one;
                SetAlpha(m_newLapText, 0);

                int remainingLaps = path.Laps - (lap - 1);
                switch (remainingLaps)
                {
                    case 1: m_newLapText.text = "Final Lap"; break;
                    default: m_newLapText.text = remainingLaps + " Laps Left"; break;
                }

                m_lapChangeTime = Time.time;
            }
            else
            {
                bool activated = Time.time - m_lapChangeTime < m_newLapDuration;
                SetAlpha(m_newLapText, Mathf.Lerp(m_newLapText.color.a, activated ? 1 : 0, Time.deltaTime / m_newLapAlphaSmoothing));
                m_newLapText.enabled = (m_newLapText.color.a > 0);

                if (m_newLapText.enabled)
                {
                    float targetScale = 1 + (m_newLapSizeMagnitude * (0.5f * Mathf.Cos(Time.time * m_newLapSizeFrequency) + 0.5f));
                    newLapRT.localScale = Vector3.Lerp(newLapRT.localScale, targetScale * Vector3.one, Time.deltaTime * 4);
                }
            }

            m_lastRank = rank;
            m_lastLap = lap;

            // energy bar
            bool usingEnergy = m_racer.Movement.IsBoosting;
            bool canUseEnergy = m_racer.Movement.CanBoost;
            float energyFill = m_racer.Energy / m_racer.MaxEnergy;

            m_energyBarFill.fillAmount = energyFill;
            m_energyBarHighlight.fillAmount = energyFill;

            float energyFailFac = 1 - Mathf.Clamp01((Time.time - (m_energyFailTime + m_energyFailFadeDuration)) / m_energyFailDuration);
            energyFailFac = Mathf.Lerp(0, energyFailFac, Mathf.Pow(0.5f * Mathf.Sin(Time.time * m_energyFailFrequency * 2 * Mathf.PI) + 0.5f, 0.35f));
            SetColorNoAlpha(m_energyBarBack, Color.Lerp(Color.black, Color.red, energyFailFac));
            SetAlpha(m_energyBarBack, Mathf.Lerp(m_energyBackAlpha, m_energyFailAlpha, energyFailFac));

            Color barColorBase = usingEnergy ? Color.white : (canUseEnergy ? 0.85f * Color.white : Color.black);
            float barAlpha = (usingEnergy || canUseEnergy) ? 1 : m_energyCantBoostAlpha;
            SetColorNoAlpha(m_energyBarFill, Color.Lerp(m_racer.Color, barColorBase, usingEnergy ? 0.15f : 0.5f));
            SetAlpha(m_energyBarFill, barAlpha);

            float energyHighlight = 1 - Mathf.Clamp01((Time.time - m_energyGainTime) / m_energyHighlightDuration);
            float energyButtonAlpha = 0;
            if (energyFill >= 1)
            {
                float energyFullFac = (0.5f * Mathf.Sin(Time.time * m_energyFullFrequency * 2 * Mathf.PI)) + 0.5f;
                energyHighlight = Mathf.Lerp(energyHighlight, m_energyFullIntensity, energyFullFac);
                energyButtonAlpha = Mathf.Lerp(1f, 0.35f, energyFullFac);
            }
            SetAlpha(m_energyBarHighlight, energyHighlight);
            SetAlpha(m_energyButton, energyButtonAlpha);

            SetColorNoAlpha(m_energyBarBorder, Color.Lerp(m_racer.Color, Color.white * 0.65f, usingEnergy ? 0 : 0.75f));
        }

        private void Player_EnergyGained(float total, float delta)
        {
            m_energyGainTime = Time.time;
        }

        private void Player_EnergyUseFailed()
        {
            m_energyFailTime = Time.time;
        }

        private void SetArrow(float smoothing)
        {
            //Waypoint next = m_racer.NextWaypoint;

            //if (next != null)
            //{
            //    Vector3 arrowPos = m_camera.MainCam.ViewportToWorldPoint(new Vector3(0.5f, m_arrowPosition, 1));
            //    Vector3 disp = next.Position - m_racer.transform.position;
            //    Quaternion arrowRot = Quaternion.LookRotation(disp);

            //    Waypoint secNext = m_racer.SecondNextWaypoint;
            //    if (secNext != null)
            //    {
            //        Quaternion nextArrowRot = Quaternion.LookRotation(secNext.Position - m_racer.transform.position);
            //        arrowRot = Quaternion.Slerp(arrowRot, nextArrowRot, 1 - Mathf.Clamp01(disp.magnitude / m_arrowNextBlendDistance));
            //    }

            //    m_arrow.position = arrowPos;
            //    m_arrow.rotation = m_arrowSmoothing > 0 ? Quaternion.Slerp(m_arrow.rotation, arrowRot, Time.deltaTime / smoothing) : arrowRot;
            //    m_arrow.localScale = m_arrowScale * Vector3.one;
            //}
        }

        private void SetColorNoAlpha(Graphic g, Color c)
        {
            g.color = new Color(c.r, c.g, c.b, g.color.a);
        }

        private void SetAlpha(Graphic g, float alpha)
        {
            Color col = g.color;
            col.a = alpha;
            g.color = col;
        }
    }
}
