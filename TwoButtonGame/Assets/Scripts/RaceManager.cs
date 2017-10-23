﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RaceManager : MonoBehaviour
{
    [SerializeField] private Camera m_clearCameraPrefab;
    [SerializeField] private CameraRig m_cameraRigPrefab;
    [SerializeField] private InRaceMenu m_raceMenuPrefab;
    [SerializeField] private Player m_playerPrefab;
    [SerializeField] private CameraManager m_playerCameraPrefab;
    [SerializeField] private PlayerUI m_playerUIPrefab;

    [SerializeField] [Range(0.1f, 5)]
    private float m_fadeInTime = 1.0f;
    [SerializeField] [Range(0.1f, 5)]
    private float m_fadeOutTime = 1.0f;
    
    [SerializeField] [Range(0, 10)]
    private int m_countdownDuration = 5;
    
    [SerializeField] private AudioClip m_countdownSound;
    [SerializeField] private AudioClip m_goSound;

    private List<Player> m_players = new List<Player>();
    private List<CameraManager> m_cameras = new List<CameraManager>();

    private RacePath m_racePath;
    public RacePath RacePath { get { return m_racePath; } }

    private InRaceMenu m_raceMenu;
    private CameraRig m_cameraRig;
    private RaceParameters m_raceParams;
    private AsyncOperation m_loading;
    private float m_raceLoadTime;
    private float m_introEndTime;
    private float m_raceStartTime;
    private float m_quitStartTime;
    private int m_countdownSecond;
    private bool m_musicStarted = false;
    private float m_fadeFac = 1;

    private Dictionary<Player, int> m_playerRanks = new Dictionary<Player, int>();

    public float CountdownTime
    {
        get { return m_raceStartTime - Time.time; }
    }

    public int PlayerCount { get { return m_players.Count; } }

    private enum State
    {
        Racing,
        Paused,
        Finished,
    }

    private State m_state = State.Racing;
    
    public void Pause()
    {
        switch (m_state)
        {
            case State.Racing:
                m_state = State.Paused;
                AudioListener.pause = true;
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
                m_raceMenu.OnResume();
                break;
        }
    }

    public void Quit()
    {
        if (m_loading == null)
        {
            m_loading = Main.Instance.LoadMainMenu();
        }
    }

    public void StartRace(RaceParameters raceParams)
    {
        m_raceParams = raceParams;
        int playerCount = raceParams.PlayerIndicies.Count;

        Instantiate(m_clearCameraPrefab);

        m_raceMenu = Instantiate(m_raceMenuPrefab).Init(playerCount);
        m_racePath = FindObjectOfType<RacePath>();

        List<Transform> spawns = new List<Transform>(m_racePath.Spawns);

        for (int playerNum = 0; playerNum < playerCount; playerNum++)
        {
            int index = Random.Range(0, spawns.Count);
            Transform spawn = spawns[index];
            spawns.RemoveAt(index);

            Player player = Instantiate(m_playerPrefab, spawn.position, spawn.rotation);

            PlayerConfig config = raceParams.PlayerConfigs[playerNum];
            GameObject graphics = Instantiate(config.CharacterGraphics, spawn.position, spawn.rotation, player.transform);
            graphics.transform.localPosition = config.GraphicsOffset;

            player.Init(playerNum, raceParams.GetPlayerInput(playerNum), raceParams.PlayerConfigs[playerNum]);
            m_players.Add(player);

            CameraManager camera = Instantiate(m_playerCameraPrefab).Init(player, playerCount);
            camera.Camera.enabled = false;
            m_cameras.Add(camera);

            PlayerUI ui = Instantiate(m_playerUIPrefab);
            m_raceMenu.AddPlayerUI(ui);
            ui.Init(player, camera, playerCount);
        }

        m_cameraRig = Instantiate(m_cameraRigPrefab).Init(raceParams.LevelConfig);
        m_cameraRig.PlayIntroSequence();

        m_raceLoadTime = Time.time;
        m_introEndTime = m_raceLoadTime + m_cameraRig.GetIntroSequenceLength();
        m_raceStartTime = m_introEndTime + m_countdownDuration + m_fadeInTime;
    }

    public void FixedUpdateRace()
    {
        if (m_state == State.Racing || m_state == State.Finished)
        {
            m_players.ForEach(p => p.MainUpdate(m_state == State.Racing && CountdownTime <= 0));
            m_cameras.ForEach(c => c.MainUpdate());

            foreach (Player player in m_players)
            {
                m_playerRanks[player] = m_players.Count(p => p != player && (
                    (p.IsFinished && player.IsFinished && p.FinishTime < player.FinishTime) ||
                    !player.IsFinished && (
                    (p.WaypointsCompleted > player.WaypointsCompleted) ||
                    (p.WaypointsCompleted == player.WaypointsCompleted && Vector3.Distance(p.CurrentWaypoint.Position, p.transform.position) < Vector3.Distance(player.CurrentWaypoint.Position, player.transform.position))
                    ))) + 1;
            }
        }

        if (m_state != State.Finished && m_players.All(p => p.IsFinished))
        {
            m_state = State.Finished;
            m_raceMenu.OnFinish();
        }
    }

    public void UpdateRace()
    {
        m_fadeFac = GetFadeFactor(false);

        if (m_loading != null)
        {
            if (m_fadeFac == 1)
            {
                m_loading.allowSceneActivation = true;
            }
        }
        else
        {
            m_quitStartTime = Time.unscaledTime;
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

        AudioManager.Instance.MusicPausable = (Time.time - m_raceStartTime < 0);
        AudioManager.Instance.Volume = Mathf.MoveTowards(AudioManager.Instance.Volume, 1 - GetFadeFactor(true), Time.unscaledDeltaTime / 0.5f);
        
        Time.timeScale = (m_state == State.Paused) ? 0 : 1;

        int countdownSecond = Mathf.CeilToInt(CountdownTime);
        if (countdownSecond != m_countdownSecond && 0 <= countdownSecond && countdownSecond <= 3)
        {
            AudioManager.Instance.PlaySound(countdownSecond == 0 ? m_goSound : m_countdownSound);
        }
        m_countdownSecond = countdownSecond;
    }

    public void LateUpdateRace()
    {
        m_cameras.ForEach(c => c.Camera.enabled = !m_cameraRig.IsPlaying);
        m_raceMenu.UpdateUI(this, !m_cameraRig.IsPlaying, m_state == State.Paused, m_state == State.Finished, m_loading != null, m_fadeFac);
    }

    private float GetFadeFactor(bool audio)
    {
        float fadeFac = 0;

        fadeFac = Mathf.Lerp(fadeFac, 1, 1 - Mathf.Clamp01(Mathf.Abs(Time.time - m_raceLoadTime) / m_fadeInTime));

        if (!audio)
        {
            fadeFac = Mathf.Lerp(fadeFac, 1, 1 - Mathf.Clamp01(Mathf.Abs(Time.time - m_introEndTime) / m_fadeInTime));
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

    public float GetTimeSinceStart(float time)
    {
        return Mathf.Max(time - m_raceStartTime, 0);
    }

    public int GetPlayerRank(Player player)
    {
        return m_playerRanks[player];
    }
}