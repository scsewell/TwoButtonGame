using System.Collections.Generic;
using System.Linq;

using BoostBlasters.Races.Racers;
using BoostBlasters.Replays;
using BoostBlasters.UI.RaceMenus;

using Framework;
using Framework.StateMachines;

using UnityEngine;

namespace BoostBlasters.Races
{
    public class RaceManager : StateMachineComponent<RaceManager, RaceState>
    {
        [Header("Prefabs")]

        [SerializeField] private Camera m_clearCameraPrefab = null;
        [SerializeField] private ReplayCamera m_replayCameraPrefab = null;
        [SerializeField] private IntroCamera m_introCameraPrefab = null;
        [SerializeField] private InRaceMenu m_raceMenuPrefab = null;
        [SerializeField] private Racer m_racerPrefab = null;
        [SerializeField] private RacerCamera m_racerCameraPrefab = null;
        [SerializeField] private PlayerUI m_playerUIPrefab = null;

        [Header("Fade")]

        [SerializeField]
        [Range(0f, 1f)]
        private float m_pauseFadeStrength = 0.65f;
        [SerializeField]
        [Range(0.01f, 5f)]
        private float m_quitFadeDuration = 1.0f;

        private RacePath m_racePath;
        public RacePath RacePath => m_racePath;

        private List<Racer> m_racers = new List<Racer>();
        public List<Racer> Racers => m_racers;

        private List<RacerCamera> m_cameras = new List<RacerCamera>();

        private RaceParameters m_raceParams;
        private Recording m_recording;
        private int m_fixedFramesSoFar;

        private InRaceMenu m_menu;
        private bool m_musicStarted;
        private bool m_savedResults;
        private bool m_savedRecording;

        private bool m_isPaused;
        private bool m_isQuiting;
        private float m_quitEndTime;

        public int RacerCount => m_raceParams.Racers.Length;

        /// <inheritdoc/>
        protected override RaceState InitialState => GetState<IntroState>();


        public void LoadRace(RaceParameters raceParams)
        {
            m_raceParams = raceParams;
            InitRace();
            StartRace();
        }

        //public void LoadReplay(Recording recording)
        //{
        //    m_raceParams = recording.Params;
        //    InitRace();

        //    m_state = State.Replay;

        //    m_raceRecording = recording;
        //    m_replayStartTime = Time.time;

        //    ResetRace(0f);

        //    AudioManager.Instance.PlayMusic(m_replayMusic);
        //    m_musicStarted = true;
        //    m_replayCamera.Activate();
        //}

        public bool RestartRace()
        {
            if (!m_isQuiting)
            {
                AudioManager.Instance.StopSounds();
                AudioManager.Instance.StopMusic();

                StartRace();

                return true;
            }
            return false;
        }

        private void StartRace()
        {
            ResetRace();

            m_musicStarted = false;
            m_savedResults = false;
            m_savedRecording = false;
        }

        private void ResetRace()
        {
            m_racePath.ResetPath();

            foreach (var racer in m_racers)
            {
                racer.ResetRacer();
            }

            foreach (var cam in m_cameras)
            {
                cam.ResetCam();
            }

            m_menu.ResetUI();

            m_fixedFramesSoFar = 0;
        }

        private void InitRace()
        {
            m_racePath = FindObjectOfType<RacePath>().Init(m_raceParams.Laps);
            m_menu = Instantiate(m_raceMenuPrefab).Init(m_raceParams);

            Instantiate(m_clearCameraPrefab);
            //m_replayCamera = Instantiate(m_replayCameraPrefab);
            //m_intoCamera = Instantiate(m_introCameraPrefab);

            // get enough spawns for the racers
            var spawns = m_racePath.Spawns.Take(RacerCount).ToList();

            // create the racers
            for (var i = 0; i < RacerCount; i++)
            {
                var config = m_raceParams.Racers[i];

                // spawn the racer at a random spawn
                var spawn = spawns.RemoveRandom();

                var racer = Instantiate(m_racerPrefab, spawn.position, spawn.rotation);
                m_racers.Add(racer);

                // create the graphics for the racer
                var graphics = Instantiate(config.Character.Graphics.Rig, racer.transform);
                graphics.transform.localPosition = config.Character.Graphics.Offset;
                graphics.transform.localRotation = Quaternion.identity;

                // prepare the racer
                racer.Init(i, config);

                // add a splitscreen if needed
                //if (i < m_raceParams.PlayerCount)
                //{
                //    RacerCamera camera = Instantiate(m_racerCameraPrefab).Init(racer, m_raceParams.PlayerCount);
                //    camera.MainCam.enabled = false;
                //    m_cameras.Add(camera);

                //    PlayerUI ui = Instantiate(m_playerUIPrefab);
                //    m_raceMenu.AddPlayerUI(ui);
                //    ui.Init(racer, config.Input, camera, m_raceParams.PlayerCount);
                //}
            }
        }

        public void FixedUpdateRace()
        {
            /*
            var isAfterStart = Time.time >= m_raceStartTime;

            if (m_state == State.Replay)
            {
                m_recording.ApplyRecordedFrame(m_fixedFramesSoFar, m_racers, m_cameras, isAfterStart);
            }
            else
            {
                if (!m_isInIntro)
                {
                    m_recording.Record(m_fixedFramesSoFar, m_racers);
                }
                m_racers.ForEach(p => p.ProcessPlaying(!m_isInIntro, isAfterStart));
            }

            m_cameras.ForEach(c => c.UpdateCamera());
            m_racePath.FixedUpdatePath();

            foreach (var player in m_racers)
            {
                var rank = 1;
                foreach (var other in m_racers)
                {
                    if (other != player)
                    {
                        var pRes = player.RaceResult;
                        var oRes = other.RaceResult;
                        if (pRes.Finished)
                        {
                            if (oRes.Finished && oRes.FinishTime < pRes.FinishTime)
                            {
                                rank++;
                            }
                        }
                        else
                        {
                            var progressDiff = other.WaypointsCompleted - player.WaypointsCompleted;

                            if (progressDiff > 0)
                            {
                                rank++;
                            }
                            else if (progressDiff == 0)
                            {
                                var otherDist = Vector3.Distance(other.NextWaypoint.Position, other.transform.position);
                                var playerDist = Vector3.Distance(player.NextWaypoint.Position, player.transform.position);

                                if (otherDist < playerDist)
                                {
                                    rank++;
                                }
                            }
                        }
                    }
                }
                //player.RaceResult.Rank = rank;
            }

            if (m_state == State.Racing && m_racers.All(p => p.RaceResult.Finished))
            {
                m_state = State.Finished;

                SaveResults();

                m_menu.OnFinish();
                m_replayStartTime = Time.time + m_replayStartWait;
            }

            if (!m_isInIntro)
            {
                m_fixedFramesSoFar++;
            }

            if (Time.time >= m_replayStartTime)
            {
                if (m_state != State.Replay)
                {
                    SaveRecording();

                    m_state = State.Replay;
                    m_replayCamera.Activate();

                    AudioManager.Instance.StopMusic();
                    AudioManager.Instance.PlayMusic(m_replayMusic);
                }

                m_replayStartTime = Time.time + m_recording.Duration + m_replayStartWait;
                ResetRace(0);
            }
            */
        }

        public void UpdateRace()
        {
            /*
            m_racers.ForEach(p => p.UpdateRacer());
            m_racePath.UpdatePath();

            m_replayCamera.SetTarget(m_racers);

            if (!m_musicStarted && Time.time - m_raceLoadTime > m_raceParams.Level.MusicDelay)
            {
                AudioManager.Instance.PlayMusic(m_raceParams.Level.Music);
                m_musicStarted = true;
            }

            AudioManager.Instance.MusicPausable = Time.time - m_raceStartTime < 0f;
            AudioManager.Instance.MusicVolume = Mathf.MoveTowards(AudioManager.Instance.MusicVolume, 1f - GetFadeFactor(true), Time.unscaledDeltaTime / 0.5f);

            var showPlayerUI = !m_isInIntro && m_state != State.Replay;
            var allowQuit = m_state == State.Finished || m_state == State.Replay;

            m_menu.UpdateUI(showPlayerUI, m_state == State.Paused, allowQuit, m_isQuiting);
            */
        }

        public void LateUpdateRace()
        {
            /*
            var showPlayerUI = !m_isInIntro && m_state != State.Replay;

            m_racers.ForEach(p => p.LateUpdateRacer());
            m_cameras.ForEach(c => c.SetCameraEnabled(showPlayerUI));

            m_menu.LateUpdateUI();
            */
        }

        public void Pause()
        {
            if (!m_isPaused)
            {
                m_isPaused = true;
                AudioListener.pause = true;
                Time.timeScale = 0;
            }
        }

        public void Resume()
        {
            if (m_isPaused)
            {
                m_isPaused = false;
                AudioListener.pause = false;
                Time.timeScale = 1f;
            }
        }

        public void Quit()
        {
            if (!m_isQuiting)
            {
                m_isQuiting = true;
                m_quitEndTime = Time.unscaledTime + m_quitFadeDuration;

                SaveResults();
                SaveRecording();

                Main.Instance.LoadMainMenu(() =>
                {
                    return Time.unscaledTime >= m_quitEndTime;
                });
            }
        }

        private void SaveResults()
        {
            if (!m_savedResults)
            {
                foreach (var racer in m_racers)
                {
                    //racer.Config.Profile.AddRaceResult(m_raceParams.Level, racer.RaceResult);
                    m_savedResults = true;
                }
            }
        }

        private void SaveRecording()
        {
            if (!m_savedRecording && m_fixedFramesSoFar > 0)
            {
                RecordingManager.SaveReplayAsync(m_recording);
                m_savedRecording = true;
            }
        }

        private float GetFadeFactor(bool audio)
        {
            var fade = audio ? CurrentState.GetAudioFade() : CurrentState.GetScreenFade();

            if (m_isPaused)
            {
                fade = FadeUtils.CombineFadeFactors(fade, m_pauseFadeStrength);
            }
            if (m_isQuiting)
            {
                var f = FadeUtils.FadeOut(Time.unscaledTime, m_quitEndTime, m_quitFadeDuration);
                fade = FadeUtils.CombineFadeFactors(fade, f);
            }

            return Mathf.SmoothStep(1f, 0f, fade);
        }
    }

    public abstract class RaceState : StateComponent<RaceManager, RaceState>
    {
        /// <summary>
        /// Gets the strength of the screen fade-to-black in this state.
        /// </summary>
        /// <remarks>
        /// This does not affect the UI.
        /// </remarks>
        /// <returns>The fade strength in the range [0,1], where 1 is fully obscured.</returns>
        public virtual float GetScreenFade()
        {
            return 0f;
        }

        /// <summary>
        /// Gets the strength of the fade applied to all audio in this state.
        /// </summary>
        /// <returns>The fade strength in the range [0,1], where 1 is fully muted.</returns>
        public virtual float GetAudioFade()
        {
            return 0f;
        }
    }

    public static class FadeUtils
    {
        /// <summary>
        /// Computes a fading factor for a fade in starting at a give time.
        /// </summary>
        /// <param name="startTime">The time in seconds at which the fade in begins.</param>
        /// <param name="duration">The duration of the fade in seconds.</param>
        /// <returns>The fade strength in the range [0,1], where 1 is fully faded out.</returns>
        public static float FadeIn(float startTime, float duration)
        {
            return FadeIn(Time.time, startTime, duration);
        }

        /// <summary>
        /// Computes a fading factor for a fade in starting at a give time.
        /// </summary>
        /// <param name="time">The current time in seconds.</param>
        /// <param name="startTime">The time in seconds at which the fade in begins.</param>
        /// <param name="duration">The duration of the fade in seconds.</param>
        /// <returns>The fade strength in the range [0,1], where 1 is fully faded out.</returns>
        public static float FadeIn(float time, float startTime, float duration)
        {
            return 1f - Mathf.Clamp01((time - startTime) / duration);
        }

        /// <summary>
        /// Computes a fading factor for a fade out starting at a give time.
        /// </summary>
        /// <param name="endTime">The time in seconds at which the fade out ends.</param>
        /// <param name="duration">The duration of the fade in seconds.</param>
        /// <returns>The fade strength in the range [0,1], where 1 is fully faded out.</returns>
        public static float FadeOut(float endTime, float duration)
        {
            return FadeOut(Time.time, endTime, duration);
        }

        /// <summary>
        /// Computes a fading factor for a fade out starting at a give time.
        /// </summary>
        /// <param name="time">The current time in seconds.</param>
        /// <param name="endTime">The time in seconds at which the fade out ends.</param>
        /// <param name="duration">The duration of the fade in seconds.</param>
        /// <returns>The fade strength in the range [0,1], where 1 is fully faded out.</returns>
        public static float FadeOut(float time, float endTime, float duration)
        {
            return Mathf.Clamp01((time - (endTime - duration)) / duration);
        }

        /// <summary>
        /// Computes the combination of multiple fade factors.
        /// </summary>
        /// <param name="a">A fade strength in the range [0,1], where 1 is fully faded out.</param>
        /// <param name="b">A fade strength in the range [0,1], where 1 is fully faded out.</param>
        /// <returns>The combined fade strength.</returns>
        public static float CombineFadeFactors(float a, float b)
        {
            return Mathf.Lerp(a, 1f, b);
        }
    }
}
