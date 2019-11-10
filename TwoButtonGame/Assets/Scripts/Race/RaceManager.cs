using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using Framework.AssetBundles;

using BoostBlasters.Players;
using BoostBlasters.Characters;
using BoostBlasters.Races.Racers;
using BoostBlasters.Replays;
using BoostBlasters.UI.RaceMenus;

namespace BoostBlasters.Races
{
    public class RaceManager : MonoBehaviour
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
        [Range(0.01f, 5f)]
        private float m_fadeInTime = 1.0f;
        [SerializeField]
        [Range(0.01f, 5f)]
        private float m_introFadeTime = 0.5f;
        [SerializeField]
        [Range(0.01f, 5f)]
        private float m_replayFadeTime = 0.5f;
        [SerializeField]
        [Range(0.01f, 5f)]
        private float m_fadeOutTime = 1.0f;

        [Header("Countdown")]

        [SerializeField]
        [Range(0f, 10f)]
        private int m_countdownDuration = 5;
        [SerializeField]
        [Range(0.5f, 5f)]
        private float m_countdownScale = 1.65f;

        [SerializeField] private AudioClip m_countdownSound = null;
        [SerializeField] private AudioClip m_goSound = null;

        [Header("Replay")]

        [SerializeField]
        [Range(0f, 30f)]
        private float m_replayStartWait = 5.0f;
        [SerializeField]
        private AssetBundleMusicReference m_replayMusic = null;

        private RacePath m_racePath;
        public RacePath RacePath => m_racePath;

        private List<Racer> m_racers = new List<Racer>();
        public List<Racer> Racers => m_racers;

        private List<RacerCamera> m_cameras = new List<RacerCamera>();

        private RaceParameters m_raceParams;
        private InRaceMenu m_raceMenu;
        private IntroCamera m_intoCamera;
        private ReplayCamera m_replayCamera;

        private bool m_isQuiting;

        private Recording m_raceRecording;
        private int m_fixedFramesSoFar;

        private bool m_isInIntro;
        private bool m_introSkipped;
        private bool m_musicStarted;
        private bool m_savedResults;
        private bool m_savedRecording;

        private float m_raceLoadTime;
        private float m_introEndTime;
        private float m_introSkipTime;
        private float m_raceStartTime;
        private float m_replayStartTime;
        private float m_quitStartTime;
        private int m_countdownSecond;

        public float TimeRaceLoad => m_raceLoadTime;
        public float TimeIntroDuration => m_intoCamera.GetIntroSequenceLength();
        public float TimeIntroEnd => m_introEndTime;
        public float TimeIntroSkip => m_introSkipTime;
        public float TimeRaceStart => m_raceStartTime;

        public float CountdownTime => (m_raceStartTime - Time.time) / m_countdownScale;

        public int RacerCount => m_raceParams.racerCount;

        private enum State
        {
            Racing,
            Paused,
            Finished,
            Replay,
        }

        private State m_state;

        public void LoadRace(RaceParameters raceParams)
        {
            m_raceParams = raceParams;
            InitRace();
            StartRace();
        }

        public void LoadReplay(Recording recording)
        {
            m_raceParams = recording.Params;
            InitRace();

            m_state = State.Replay;

            m_raceRecording = recording;
            m_replayStartTime = Time.time;

            ResetRace(0f);

            AudioManager.Instance.PlayMusic(m_replayMusic);
            m_musicStarted = true;
            m_replayCamera.Activate();
        }

        public bool RestartRace()
        {
            if (!m_isQuiting)
            {
                AudioManager.Instance.StopSounds();
                AudioManager.Instance.StopMusic();
                m_intoCamera.StopIntroSequence();

                StartRace();

                return true;
            }
            return false;
        }

        private void StartRace()
        {
            m_state = State.Racing;

            m_raceRecording = new Recording(m_raceParams, m_racers.Select(r => r.RaceResult).ToArray());
            m_replayStartTime = float.PositiveInfinity;

            ResetRace(m_intoCamera.GetIntroSequenceLength());

            m_isInIntro = true;
            m_musicStarted = false;
            m_savedResults = false;
            m_savedRecording = false;
            m_intoCamera.PlayIntroSequence();
        }

        private void ResetRace(float introLength)
        {
            m_countdownSecond = 0;
            m_isInIntro = false;
            m_introSkipped = false;

            m_raceLoadTime = Time.time;
            m_introEndTime = m_raceLoadTime + introLength;
            m_introSkipTime = float.MaxValue;
            m_raceStartTime = m_introEndTime + GetCountdownLength();

            m_racePath.ResetPath();

            foreach (Racer racer in m_racers)
            {
                racer.ResetRacer();
            }

            foreach (RacerCamera cam in m_cameras)
            {
                cam.ResetCam();
            }

            m_raceMenu.ResetUI();

            m_fixedFramesSoFar = 0;
        }

        private void InitRace()
        {
            m_racePath = FindObjectOfType<RacePath>().Init(m_raceParams.laps);
            m_raceMenu = Instantiate(m_raceMenuPrefab).Init(m_raceParams);

            Instantiate(m_clearCameraPrefab);
            m_replayCamera = Instantiate(m_replayCameraPrefab);
            m_intoCamera = Instantiate(m_introCameraPrefab).Init(m_raceParams.level);

            List<Transform> spawns = m_racePath.Spawns.Take(RacerCount).ToList();

            for (int racerNum = 0; racerNum < RacerCount; racerNum++)
            {
                int index = Random.Range(0, spawns.Count);
                Transform spawn = spawns[index];
                spawns.RemoveAt(index);

                Racer racer = Instantiate(m_racerPrefab, spawn.position, spawn.rotation);
                m_racers.Add(racer);

                Profile profile = m_raceParams.profiles[racerNum];
                Character config = m_raceParams.characters[racerNum];

                GameObject graphics = Instantiate(config.CharacterGraphics, spawn.position, spawn.rotation, racer.transform);
                graphics.transform.localPosition = config.GraphicsOffset;

                if (racerNum < m_raceParams.humanCount)
                {
                    PlayerBaseInput input = m_raceParams.inputs[racerNum];
                    racer.InitHuman(racerNum, profile, config, input);

                    RacerCamera camera = Instantiate(m_racerCameraPrefab).Init(racer, m_raceParams.humanCount);
                    camera.MainCam.enabled = false;
                    m_cameras.Add(camera);

                    PlayerUI ui = Instantiate(m_playerUIPrefab);
                    m_raceMenu.AddPlayerUI(ui);
                    ui.Init(racer, input, camera, m_raceParams.humanCount);
                }
                else
                {
                    racer.InitAI(racerNum, profile, config);
                }
            }
        }

        public void FixedUpdateRace()
        {
            if (m_isInIntro && Time.time >= m_introEndTime)
            {
                m_isInIntro = false;
                m_raceStartTime = m_introEndTime + GetCountdownLength();
            }

            bool isAfterStart = Time.time >= m_raceStartTime;

            if (m_state == State.Replay)
            {
                m_raceRecording.ApplyRecordedFrame(m_fixedFramesSoFar, m_racers, m_cameras, isAfterStart);
            }
            else
            {
                if (!m_isInIntro)
                {
                    m_raceRecording.Record(m_fixedFramesSoFar, m_racers);
                }
                m_racers.ForEach(p => p.ProcessPlaying(!m_isInIntro, isAfterStart));
            }

            m_cameras.ForEach(c => c.UpdateCamera());
            m_racePath.FixedUpdatePath();

            foreach (Racer player in m_racers)
            {
                int rank = 1;
                foreach (Racer other in m_racers)
                {
                    if (other != player)
                    {
                        RaceResult pRes = player.RaceResult;
                        RaceResult oRes = other.RaceResult;
                        if (pRes.Finished)
                        {
                            if (oRes.Finished && oRes.FinishTime < pRes.FinishTime)
                            {
                                rank++;
                            }
                        }
                        else
                        {
                            int progressDiff = other.WaypointsCompleted - player.WaypointsCompleted;

                            if (progressDiff > 0)
                            {
                                rank++;
                            }
                            else if (progressDiff == 0)
                            {
                                float otherDist = Vector3.Distance(other.NextWaypoint.Position, other.transform.position);
                                float playerDist = Vector3.Distance(player.NextWaypoint.Position, player.transform.position);

                                if (otherDist < playerDist)
                                {
                                    rank++;
                                }
                            }
                        }
                    }
                }
                player.RaceResult.Rank = rank;
            }

            if (m_state == State.Racing && m_racers.All(p => p.RaceResult.Finished))
            {
                m_state = State.Finished;

                SaveResults();

                m_raceMenu.OnFinish();
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

                m_replayStartTime = Time.time + m_raceRecording.Duration + m_replayStartWait;
                ResetRace(0);
            }
        }

        public void UpdateRace()
        {
            m_intoCamera.UpdateCamera(m_isInIntro);

            m_racers.ForEach(p => p.UpdateRacer());
            m_racePath.UpdatePath();

            m_replayCamera.SetTarget(m_racers);

            if (!m_musicStarted && Time.time - m_raceLoadTime > m_raceParams.level.MusicDelay)
            {
                AudioManager.Instance.PlayMusic(m_raceParams.level.Music);
                m_musicStarted = true;
            }

            int countdownSecond = Mathf.CeilToInt(CountdownTime);
            if (countdownSecond != m_countdownSecond && 0 <= countdownSecond && countdownSecond <= 3)
            {
                AudioManager.Instance.PlaySound(countdownSecond == 0 ? m_goSound : m_countdownSound);
            }
            m_countdownSecond = countdownSecond;

            AudioManager.Instance.MusicPausable = Time.time - m_raceStartTime < 0f;
            AudioManager.Instance.MusicVolume = Mathf.MoveTowards(AudioManager.Instance.MusicVolume, 1f - GetFadeFactor(true), Time.unscaledDeltaTime / 0.5f);

            bool showPlayerUI = !m_isInIntro && m_state != State.Replay;
            bool allowQuit = m_state == State.Finished || m_state == State.Replay;

            m_raceMenu.UpdateUI(showPlayerUI, m_state == State.Paused, allowQuit, m_isQuiting);
        }

        public void LateUpdateRace()
        {
            bool showPlayerUI = !m_isInIntro && m_state != State.Replay;

            m_racers.ForEach(p => p.LateUpdateRacer());
            m_cameras.ForEach(c => c.SetCameraEnabled(showPlayerUI));

            m_raceMenu.LateUpdateUI();
        }

        public bool SkipIntro()
        {
            if (m_isInIntro && !m_introSkipped)
            {
                m_introSkipped = true;
                m_introSkipTime = Time.time;
                m_introEndTime = m_introSkipTime + m_introFadeTime;

                return true;
            }
            return false;
        }

        public void Pause()
        {
            if (m_state == State.Racing)
            {
                m_state = State.Paused;
                AudioListener.pause = true;
                Time.timeScale = 0;
            }
        }

        public void Resume()
        {
            if (m_state == State.Paused)
            {
                m_state = State.Racing;
                AudioListener.pause = false;
                Time.timeScale = 1f;
            }
        }

        public void Quit()
        {
            if (!m_isQuiting)
            {
                m_isQuiting = true;
                m_quitStartTime = Time.unscaledTime;

                SaveResults();
                SaveRecording();

                Main.Instance.LoadMainMenu(() =>
                {
                    return GetFadeFactor(false) >= 0.99f;
                });
            }
        }

        private void SaveResults()
        {
            if (!m_savedResults)
            {
                foreach (Racer player in m_racers)
                {
                    player.Profile.AddRaceResult(m_raceParams.level, player.RaceResult);
                    m_savedResults = true;
                }
            }
        }

        private void SaveRecording()
        {
            if (!m_savedRecording && m_fixedFramesSoFar > 0)
            {
                RecordingManager.Instance.SaveReplayAsync(m_raceRecording);
                m_savedRecording = true;
            }
        }

        public float GetFadeFactor(bool audio)
        {
            float fac = 1f;

            // fade screen when the scene is loaded or replay restarts
            if (!audio || m_state != State.Replay)
            {
                fac *= Mathf.Clamp01(Mathf.Abs(Time.time - m_raceLoadTime) / m_fadeInTime);
                fac *= Mathf.Clamp01(Mathf.Abs(Time.time - m_replayStartTime) / m_replayFadeTime);
            }
            // fade screen from intro animation end to race
            if (!audio)
            {
                fac *= Mathf.Clamp01(Mathf.Abs(Time.time - m_introEndTime) / m_introFadeTime);
            }
            // fade out when exiting scene
            if (m_isQuiting)
            {
                fac *= 1f - Mathf.Clamp01((Time.unscaledTime - m_quitStartTime) / m_fadeOutTime);
            }
            // fade view when paused
            if (m_state == State.Paused)
            {
                fac *= 1f - 0.65f;
            }

            return Mathf.SmoothStep(1f, 0f, fac);
        }

        public float GetStartRelativeTime()
        {
            return GetStartRelativeTime(Time.time);
        }

        public float GetStartRelativeTime(float time)
        {
            return time - m_raceStartTime;
        }

        private float GetCountdownLength()
        {
            return (m_countdownScale * m_countdownDuration) + m_introFadeTime;
        }
    }
}
