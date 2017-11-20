using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Framework;
using Framework.Interpolation;

public class Main : ComponentSingleton<Main>
{
    private RaceManager m_raceManagerPrefab;

    private RaceManager m_raceManager;
    public RaceManager RaceManager { get { return m_raceManager; } }

    private RaceParameters m_raceParams = null;
    public RaceParameters LastRaceParams { get { return m_raceParams; } }

    private PlayerConfig[] m_playerConfigs;
    public PlayerConfig[] PlayerConfigs { get { return m_playerConfigs; } }

    private LevelConfig[] m_levelConfigs;
    public LevelConfig[] LevelConfigs { get { return m_levelConfigs; } }

    private bool m_hasLoadedScene = false;
    public bool HasLoadedScene { get { return m_hasLoadedScene; } }

    private RaceType m_lastRaceType;
    public RaceType LastRaceType { get { return m_lastRaceType; } }

    public enum RaceType
    {
        Race,
        Replay,
        None,
    }

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);

        m_raceManagerPrefab = Resources.Load<RaceManager>("RaceManager");
        m_playerConfigs = Resources.LoadAll<PlayerConfig>("PlayerConfigs/").OrderBy(c => c.SortOrder).ToArray();
        m_levelConfigs = Resources.LoadAll<LevelConfig>("LevelConfigs/").OrderBy(c => c.SortOrder).ToArray();

        PlayerProfileManager.Instance.LoadProfiles();

        SettingManager.Instance.Load();
        SettingManager.Instance.Apply();

        m_lastRaceType = RaceType.None;

        gameObject.AddComponent<AudioListener>();
    }

    private void FixedUpdate()
    {
        InterpolationController.Instance.EarlyFixedUpdate();

        if (m_raceManager != null)
        {
            m_raceManager.FixedUpdateRace();
        }
    }

    private void Update()
    {
        InputManager.Instance.Update();
        InterpolationController.Instance.VisualUpdate();

        if (m_raceManager != null)
        {
            m_raceManager.UpdateRace();
        }

        if (Input.GetKeyDown(KeyCode.F11))
        {
            ScreenCapture.CaptureScreenshot("screenshot_" + DateTime.Now.ToString("MM-dd-yy_H-mm-ss") + ".png");
        }
    }

    private void LateUpdate()
    {
        if (m_raceManager != null)
        {
            m_raceManager.LateUpdateRace();
        }
    }

    public AsyncOperation LoadMainMenu()
    {
        AsyncOperation loading = SceneManager.LoadSceneAsync(0);

        StartCoroutine(LoadLevel(loading, () => StartMainMenu()));
        return loading;
    }

    private void StartMainMenu()
    {
        m_raceManager = null;
    }

    public AsyncOperation LoadRace(RaceParameters raceParams)
    {
        m_lastRaceType = RaceType.Race;
        m_raceParams = raceParams;
        AsyncOperation loading = SceneManager.LoadSceneAsync(raceParams.LevelConfig.SceneName);
        StartCoroutine(LoadLevel(loading, () => StartRace(raceParams)));
        return loading;
    }

    private void StartRace(RaceParameters raceParams)
    {
        m_raceManager = Instantiate(m_raceManagerPrefab);
        m_raceManager.StartRace(raceParams);
    }

    public AsyncOperation LoadRace(RaceRecording recording)
    {
        m_lastRaceType = RaceType.Replay;
        m_raceParams = null;
        AsyncOperation loading = SceneManager.LoadSceneAsync(recording.RaceParams.LevelConfig.SceneName);
        StartCoroutine(LoadLevel(loading, () => StartRace(recording)));
        return loading;
    }

    private void StartRace(RaceRecording recording)
    {
        m_raceManager = Instantiate(m_raceManagerPrefab);
        m_raceManager.StartRace(recording);
    }

    private IEnumerator LoadLevel(AsyncOperation loading, Action onComplete)
    {
        m_hasLoadedScene = true;
        Application.backgroundLoadingPriority = ThreadPriority.BelowNormal;
        loading.allowSceneActivation = false;
        yield return new WaitWhile(() => !loading.allowSceneActivation);
        AudioListener.volume = 0;
        AudioListener.pause = false;
        Time.timeScale = 1;
        yield return new WaitWhile(() => !loading.isDone);
        onComplete();
    }

    private void OnApplicationFocus(bool focus)
    {
        if (focus)
        {
            SettingManager.Instance.Apply();
        }
    }

    public PlayerConfig GetPlayerConfig(int configID)
    {
        foreach (PlayerConfig config in m_playerConfigs)
        {
            if (config.Id == configID)
            {
                return config;
            }
        }
        return null;
    }

    public LevelConfig GetLevelConfig(int configID)
    {
        foreach (LevelConfig config in m_levelConfigs)
        {
            if (config.Id == configID)
            {
                return config;
            }
        }
        return null;
    }
}
