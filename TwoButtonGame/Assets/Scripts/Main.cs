﻿using System;
using System.Collections;
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

    private IEnumerator LoadLevel(AsyncOperation loading, Action onComplete)
    {
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
}
