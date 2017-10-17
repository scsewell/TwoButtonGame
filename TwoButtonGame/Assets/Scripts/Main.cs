using System;
using System.Collections;
using System.Collections.Generic;
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

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);

        m_raceManagerPrefab = Resources.Load<RaceManager>("RaceManager");
        gameObject.AddComponent<AudioListener>();

        SettingManager.Instance.Load();
        SettingManager.Instance.Apply();
    }

    private void Update()
    {
        InterpolationController.Instance.VisualUpdate();
        InputManager.Instance.Update();

        if (m_raceManager != null)
        {
            m_raceManager.UpdateRace();
        }
    }

    private void LateUpdate()
    {
        InputManager.Instance.LateUpdate();
    }

    private void FixedUpdate()
    {
        InterpolationController.Instance.EarlyFixedUpdate();

        if (m_raceManager != null)
        {
            m_raceManager.FixedUpdateRace();
        }
    }

    public AsyncOperation LoadMainMenu()
    {
        AsyncOperation loading = SceneManager.LoadSceneAsync(0);
        loading.allowSceneActivation = false;

        StartCoroutine(LoadLevel(loading, () => StartMainMenu()));
        return loading;
    }

    private void StartMainMenu()
    {
        m_raceManager = null;
    }

    public AsyncOperation LoadRace(RaceParameters raceParams)
    {
        m_raceParams = raceParams;

        AsyncOperation loading = SceneManager.LoadSceneAsync(raceParams.LevelConfig.SceneName);
        loading.allowSceneActivation = false;

        StartCoroutine(LoadLevel(loading, () => StartRace(raceParams)));
        return loading;
    }

    private void StartRace(RaceParameters raceParams)
    {
        m_raceManager = Instantiate(m_raceManagerPrefab);
        m_raceManager.StartRace(raceParams);
    }

    private IEnumerator LoadLevel(AsyncOperation loading, Action onComplete)
    {
        yield return new WaitWhile(() => !loading.allowSceneActivation);
        AudioListener.volume = 0;
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
}
