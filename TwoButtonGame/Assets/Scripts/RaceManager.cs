﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RaceManager : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private Camera m_clearCameraPrefab;
    [SerializeField] private ReplayCamera m_replayCameraPrefab;
    [SerializeField] private CameraRig m_cameraRigPrefab;
    [SerializeField] private InRaceMenu m_raceMenuPrefab;
    [SerializeField] private Player m_playerPrefab;
    [SerializeField] private CameraManager m_playerCameraPrefab;
    [SerializeField] private PlayerUI m_playerUIPrefab;

    [Header("Fade")]
    [SerializeField]
    [Range(0.01f, 5)]
    private float m_fadeInTime = 1.0f;
    [SerializeField]
    [Range(0.01f, 5)]
    private float m_introFadeTime = 0.5f;
    [SerializeField]
    [Range(0.01f, 5)]
    private float m_replayFadeTime = 0.5f;
    [SerializeField]
    [Range(0.01f, 5)]
    private float m_fadeOutTime = 1.0f;

    [Header("Countdown")]
    [SerializeField]
    [Range(0, 10)]
    private int m_countdownDuration = 5;
    [SerializeField]
    [Range(0.5f, 5)]
    private float m_countdownScale = 1.65f;

    [SerializeField] private AudioClip m_countdownSound;
    [SerializeField] private AudioClip m_goSound;

    [Header("Replay")]
    [SerializeField]
    [Range(0, 30)]
    private float m_replayStartWait = 5.0f;
    [SerializeField]
    private MusicParams m_replayMusic;

    [Header("Options")]
    [SerializeField]
    private bool m_skipIntro = false;
    [SerializeField]
    private bool m_skipCoundown = false;

    private RacePath m_racePath;
    public RacePath RacePath { get { return m_racePath; } }

    private List<Player> m_players = new List<Player>();
    public List<Player> Players { get { return m_players; } }

    private List<CameraManager> m_cameras = new List<CameraManager>();

    private RaceParameters m_raceParams;
    private InRaceMenu m_raceMenu;
    private CameraRig m_cameraRig;
    private ReplayCamera m_replayCamera;
    private AsyncOperation m_loading;
    private float m_raceLoadTime;
    private float m_introEndTime;
    private float m_raceStartTime;
    private float m_replayStartTime;
    private float m_quitStartTime;
    private int m_countdownSecond;
    private bool m_musicStarted = false;
    private float m_fadeFac = 1;
    private bool m_isReplay;
    private RaceRecording m_raceRecording;
    private int m_fixedFramesSoFar;

    public float CountdownTime
    {
        get { return (m_raceStartTime - Time.time) / m_countdownScale; }
    }

    public int PlayerCount { get { return m_raceParams.PlayerCount; } }

    private enum State
    {
        Racing,
        Paused,
        Finished,
        Replay,
    }

    private State m_state;
    
    public void StartRace(RaceParameters raceParams)
    {
        if (!Application.isEditor)
        {
            m_skipCoundown = false;
            m_skipIntro = false;
        }

        m_raceParams = raceParams;

        m_isReplay = false;
        m_raceRecording = new RaceRecording(m_raceParams);
        m_replayStartTime = float.PositiveInfinity;
        
        InitRace();

        m_state = State.Racing;
    }

    public void StartRace(RaceRecording recording)
    {
        if (!Application.isEditor)
        {
            m_skipCoundown = false;
        }
        m_skipIntro = true;

        m_isReplay = true;
        m_raceRecording = recording;
        m_replayStartTime = Time.time;

        m_raceParams = recording.RaceParams;
        
        InitRace();

        m_state = State.Replay;
        m_replayCamera.Activate();

        AudioManager.Instance.PlayMusic(m_replayMusic);
        m_musicStarted = true;
    }

    public void InitRace()
    {
        m_racePath = FindObjectOfType<RacePath>().Init(m_raceParams.Laps);
        m_raceMenu = Instantiate(m_raceMenuPrefab).Init(m_raceParams.HumanCount);

        Instantiate(m_clearCameraPrefab);
        m_replayCamera = Instantiate(m_replayCameraPrefab);
        m_cameraRig = Instantiate(m_cameraRigPrefab).Init(m_raceParams.LevelConfig);

        List<Transform> spawns = m_racePath.Spawns.Take(PlayerCount).ToList();

        for (int playerNum = 0; playerNum < PlayerCount; playerNum++)
        {
            int index = Random.Range(0, spawns.Count);
            Transform spawn = spawns[index];
            spawns.RemoveAt(index);

            Player player = Instantiate(m_playerPrefab, spawn.position, spawn.rotation);
            m_players.Add(player);

            PlayerConfig config = m_raceParams.PlayerConfigs[playerNum];
            GameObject graphics = Instantiate(config.CharacterGraphics, spawn.position, spawn.rotation, player.transform);
            graphics.transform.localPosition = config.GraphicsOffset;

            if (playerNum < m_raceParams.HumanCount)
            {
                player.InitHuman(playerNum, m_raceParams.PlayerConfigs[playerNum], m_raceParams.GetPlayerInput(playerNum));

                CameraManager camera = Instantiate(m_playerCameraPrefab).Init(player, m_raceParams.HumanCount);
                camera.MainCam.enabled = false;
                m_cameras.Add(camera);

                PlayerUI ui = Instantiate(m_playerUIPrefab);
                m_raceMenu.AddPlayerUI(ui);
                ui.Init(player, camera, m_raceParams.HumanCount);
            }
            else
            {
                player.InitAI(playerNum, m_raceParams.PlayerConfigs[playerNum]);
            }
        }

        ResetRace();
    }

    private void ResetRace()
    {
        float introLength = 0;
        if (!m_skipIntro)
        {
            m_cameraRig.PlayIntroSequence();
            introLength = m_cameraRig.GetIntroSequenceLength();
        }
        m_skipIntro = true;

        float coundownLength = 0;
        if (!m_skipCoundown)
        {
            coundownLength = (m_countdownScale * m_countdownDuration) + m_introFadeTime;
        }

        m_raceLoadTime = Time.time;
        m_introEndTime = m_raceLoadTime + introLength;
        m_raceStartTime = m_introEndTime + coundownLength;

        m_racePath.ResetPath();

        foreach (Player player in m_players)
        {
            player.ResetPlayer();
        }

        foreach (CameraManager cameraManager in m_cameras)
        {
            cameraManager.ResetCam();
        }

        m_raceRecording.ResetRecorder();
        m_fixedFramesSoFar = 0;
    }

    public void FixedUpdateRace()
    {
        bool isAfterIntro = Time.time >= m_introEndTime;
        bool isAfterStart = Time.time >= m_raceStartTime;
        
        if (m_state == State.Replay)
        {
            m_raceRecording.MoveGhosts(m_fixedFramesSoFar, m_players, m_cameras, isAfterStart);
        }
        else
        {
            if (isAfterIntro)
            {
                m_raceRecording.Record(m_fixedFramesSoFar, m_players);
            }
            m_players.ForEach(p => p.ProcessPlaying(isAfterIntro, isAfterStart));
        }

        m_cameras.ForEach(c => c.UpdateCamera());

        m_racePath.FixedUpdatePath();

        foreach (Player player in m_players)
        {
            int rank = 1;
            foreach (Player other in m_players)
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

        if (m_state == State.Racing && m_players.All(p => p.RaceResult.Finished))
        {
            m_state = State.Finished;
            m_replayStartTime = Time.time + m_replayStartWait;
            m_raceMenu.OnFinish();
        }
        
        if (isAfterIntro)
        {
            m_fixedFramesSoFar++;
        }

        if (Time.time >= m_replayStartTime)
        {
            if (m_state != State.Replay)
            {
                m_state = State.Replay;
                m_replayCamera.Activate();
                
                AudioManager.Instance.StopMusic();
                AudioManager.Instance.PlayMusic(m_replayMusic);
            }
            m_replayStartTime = Time.time + m_raceRecording.Duration + m_replayStartWait;
            ResetRace();
        }
    }

    public void UpdateRace()
    {
        m_fadeFac = GetFadeFactor(false);

        if (m_loading != null && m_fadeFac == 1)
        {
            m_loading.allowSceneActivation = true;
        }

        if (!m_musicStarted && Time.time - m_raceLoadTime > m_raceParams.LevelConfig.MusicDelay)
        {
            MusicParams music = m_raceParams.LevelConfig.Music;
            if (music != null)
            {
                AudioManager.Instance.PlayMusic(music);
            }
            m_musicStarted = true;
        }

        if (!m_skipCoundown && m_state != State.Replay)
        {
            int countdownSecond = Mathf.CeilToInt(CountdownTime);
            if (countdownSecond != m_countdownSecond && 0 <= countdownSecond && countdownSecond <= 3)
            {
                AudioManager.Instance.PlaySound(countdownSecond == 0 ? m_goSound : m_countdownSound);
            }
            m_countdownSecond = countdownSecond;
        }

        SettingManager.Instance.SetShadowDistance();

        AudioManager.Instance.MusicPausable = (Time.time - m_raceStartTime < 0);
        AudioManager.Instance.Volume = Mathf.MoveTowards(AudioManager.Instance.Volume, 1 - GetFadeFactor(true), Time.unscaledDeltaTime / 0.5f);

        m_players.ForEach(p => p.UpdatePlayer());
        m_racePath.UpdatePath();

        foreach (Player player in m_players)
        {
            if (player.RaceResult.Rank == 1)
            {
                m_replayCamera.SetTarget(player.transform);
            }
        }
    }

    public void LateUpdateRace()
    {
        bool isInIntro = (m_cameraRig != null && m_cameraRig.IsPlaying);
        bool showPlayerUI = !isInIntro && m_state != State.Replay;
        bool allowQuit = m_state == State.Finished || m_state == State.Replay;

        m_players.ForEach(p => p.LateUpdatePlayer());
        m_cameras.ForEach(c => c.SetCameraEnabled(showPlayerUI));

        m_raceMenu.UpdateUI(this, showPlayerUI, m_state == State.Paused, allowQuit, m_loading != null, m_fadeFac);
    }

    public void Pause()
    {
        switch (m_state)
        {
            case State.Racing:
                m_state = State.Paused;
                AudioListener.pause = true;
                Time.timeScale = 0;
                m_raceMenu.OnPause();
                break;
        }
    }

    public void Resume()
    {
        switch (m_state)
        {
            case State.Paused:
                m_state = State.Racing;
                AudioListener.pause = false;
                Time.timeScale = 1;
                m_raceMenu.OnResume();
                break;
        }
    }

    public void Quit()
    {
        if (m_loading == null)
        {
            if (!m_isReplay)
            {
                RecordingManager.Instance.SaveRecording(m_raceRecording, m_players);
            }
            
            m_loading = Main.Instance.LoadMainMenu();
            m_quitStartTime = Time.unscaledTime;
        }
    }

    private float GetFadeFactor(bool audio)
    {
        float fadeFac = 0;

        if (!audio || m_state != State.Replay)
        {
            fadeFac = Mathf.Lerp(fadeFac, 1, 1 - Mathf.Clamp01(Mathf.Abs(Time.time - m_raceLoadTime) / m_fadeInTime));
            fadeFac = Mathf.Lerp(fadeFac, 1, 1 - Mathf.Clamp01(Mathf.Abs(Time.time - m_replayStartTime) / m_replayFadeTime));
        }
        
        if (!audio)
        {
            fadeFac = Mathf.Lerp(fadeFac, 1, 1 - Mathf.Clamp01(Mathf.Abs(Time.time - m_introEndTime) / m_introFadeTime));
        }

        if (m_loading != null)
        {
            fadeFac = Mathf.Lerp(fadeFac, 1, Mathf.Clamp01((Time.unscaledTime - m_quitStartTime) / m_fadeOutTime));
        }

        if (m_state == State.Paused)
        {
            fadeFac = Mathf.Lerp(fadeFac, 1, 0.65f);
        }

        return 1 - (0.5f * Mathf.Cos((Mathf.PI) * fadeFac) + 0.5f);
    }

    public float GetStartRelativeTime()
    {
        return Time.time - m_raceStartTime;
    }

    public float GetStartRelativeTime(float time)
    {
        return time - m_raceStartTime;
    }
}