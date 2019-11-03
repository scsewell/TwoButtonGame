﻿using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

using Framework.Audio;

using BoostBlasters.Races;
using BoostBlasters.Races.Racers;
using BoostBlasters.Levels;

namespace BoostBlasters.UI.RaceMenus
{
    public class InRaceMenu : MenuBase
    {
        [Header("Intro Panel")]

        [SerializeField] private Canvas m_titleCardMenu = null;
        [SerializeField] private Text m_trackName = null;
        [SerializeField] private Text m_songName = null;
        [SerializeField] private ControlPanel m_skipControls = null;
        [SerializeField] private CanvasGroup m_cardUpper = null;
        [SerializeField] private CanvasGroup m_cardLower = null;
        [SerializeField] private CanvasGroup m_cardBottom = null;

        [SerializeField]
        [Range(0f, 5f)]
        private float m_slideInWait = 1.0f;
        [SerializeField]
        [Range(0f, 5f)]
        private float m_slideOutTime = 1.0f;
        [SerializeField]
        [Range(0f, 5f)]
        private float m_slideDutation = 0.5f;

        private float m_introSkipTime = 0f;

        [Header("UI Elements")]

        [SerializeField] private Canvas m_playerUIParent = null;
        [SerializeField] private ControlPanel m_menuControls1 = null;
        [SerializeField] private ControlPanel m_menuControls2 = null;
        [SerializeField] private Image m_fade = null;

        [Header("Fade")]

        [SerializeField]
        [Range(0f, 10f)]
        private float m_finishWait = 1f;
        [SerializeField]
        [Range(0f, 5f)]
        private float m_finishFadeInTime = 0.5f;

        private GameMenuRoot m_root = null;
        public GameMenuRoot Root => m_root;

        private GameMenuFinished m_finish = null;
        public GameMenuFinished Finish => m_finish;

        private List<PlayerBaseInput> m_activeInputs = null;
        public List<PlayerBaseInput> ActiveInputs => m_activeInputs;

        private List<PlayerUI> m_playerUIs = null;
        private float m_finishTime = 0f;

        public InRaceMenu Init(RaceParameters raceParameters)
        {
            m_activeInputs = raceParameters.inputs;
            InitBase(InputManager.Instance.PlayerInputs.ToList());

            m_root = GetComponentInChildren<GameMenuRoot>();
            m_finish = GetComponentInChildren<GameMenuFinished>();

            m_playerUIs = new List<PlayerUI>();

            CanvasScaler scaler = m_playerUIParent.GetComponent<CanvasScaler>();
            Rect splitscreen = RacerCamera.GetSplitscreen(0, raceParameters.humanCount);
            scaler.referenceResolution = new Vector2(
                scaler.referenceResolution.x / splitscreen.width,
                scaler.referenceResolution.y / splitscreen.height
            );

            m_trackName.text = raceParameters.level.Name;
            SetMusicTitle(raceParameters.level);

            m_fade.enabled = true;
            m_fade.color = new Color(0f, 0f, 0f, 1f);

            return this;
        }

        private async void SetMusicTitle(Level level)
        {
            m_songName.text = string.Empty;

            Music music = await level.Music.GetAsync();

            m_songName.text = $"\"{music.Name}\" - {music.Artist}";
        }

        public void ResetUI()
        {
            m_playerUIs.ForEach(ui => ui.ResetPlayerUI());
        }

        public void AddPlayerUI(PlayerUI playerUI)
        {
            playerUI.transform.SetParent(m_playerUIParent.transform, false);
            m_playerUIs.Add(playerUI);
        }

        public void UpdateUI(bool showPlayerUI, bool isPaused, bool isFinished, bool isQuitting)
        {
            UpdateBase();

            m_playerUIParent.enabled = showPlayerUI;
            if (m_playerUIParent.enabled)
            {
                foreach (PlayerUI ui in m_playerUIs)
                {
                    ui.UpdateUI();
                }
            }

            RaceManager raceManager = Main.Instance.RaceManager;

            m_titleCardMenu.enabled = Time.time <= raceManager.TimeIntroEnd;
            if (m_titleCardMenu.enabled)
            {
                float inTime = raceManager.TimeRaceLoad + m_slideInWait;
                float outTime = Mathf.Min(raceManager.TimeRaceLoad + raceManager.TimeIntroDuration - m_slideOutTime, raceManager.TimeIntroSkip);

                SetIntroCardPos(m_cardUpper, inTime, 0.0f, outTime, 0.2f);
                SetIntroCardPos(m_cardLower, inTime, 0.1f, outTime, 0.1f);
                SetIntroCardPos(m_cardBottom, inTime, 0.3f, outTime, 0.0f);

                m_skipControls.UpdateUI("Skip", m_activeInputs.SelectMany(i => i.SpriteAccept).ToList());

                if (ActiveMenu == null)
                {
                    foreach (PlayerBaseInput input in Inputs)
                    {
                        if (input.UI_Accept && raceManager.SkipIntro())
                        {
                            PlayNextMenuSound();
                            break;
                        }
                    }
                }
            }

            if (Inputs.Any(i => i.UI_Menu) || Input.GetKeyDown(KeyCode.Escape))
            {
                if (!isFinished)
                {
                    if (isPaused)
                    {
                        SetMenu(null, TransitionSound.Back);
                    }
                    else
                    {
                        SetMenu(Root, TransitionSound.Open);
                    }
                }
                else if (ActiveMenu == null)
                {
                    SetMenu(Finish, TransitionSound.Open);
                }
            }

            if (!isFinished)
            {
                if (ActiveMenu == null && !isQuitting)
                {
                    raceManager.Resume();
                }
                else
                {
                    raceManager.Pause();
                }
            }

            /*
            float menuAlpha = 1;
            if (isFinished)
            {
                menuAlpha = Mathf.Clamp01((Time.unscaledTime - (m_finishTime + m_finishWait)) / m_finishFadeInTime);
            }
            m_menuGroup.alpha = menuAlpha * (isPaused ? 1 : (1 - fade));
            */
        }

        public void LateUpdateUI()
        {
            LateUpdateBase((previous) => true);

            m_fade.color = new Color(0, 0, 0, Main.Instance.RaceManager.GetFadeFactor(false));
        }

        private void SetIntroCardPos(CanvasGroup card, float inTime, float inOffset, float outTime, float outOffset)
        {
            float time = Mathf.Clamp01((Time.time - (inTime + inOffset)) / m_slideDutation) * (1 - Mathf.Clamp01((Time.time - (outTime + outOffset)) / m_slideDutation));
            float fac = (0.5f * Mathf.Cos(time * Mathf.PI)) + 0.5f;

            RectTransform rt = card.GetComponent<RectTransform>();
            float x = -(rt.rect.width + 300) * fac;
            rt.anchoredPosition = new Vector2(x, rt.anchoredPosition.y);

            card.alpha = 1f - fac;
        }

        public void OnFinish()
        {
            m_finishTime = Time.unscaledTime;
        }
    }
}
