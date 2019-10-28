using System;
using System.Collections;

using UnityEngine;
using UnityEngine.SceneManagement;

using Framework;
using Framework.Interpolation;

using BoostBlasters.Players;
using BoostBlasters.Characters;
using BoostBlasters.Levels;
using BoostBlasters.Races;
using BoostBlasters.Replays;

namespace BoostBlasters
{
    /// <summary>
    /// Manages global information.
    /// </summary>
    public class Main : ComponentSingleton<Main>
    {
        private RaceManager m_raceManagerPrefab;

        public RaceManager RaceManager { get; private set; }
        public RaceParameters LastRaceParams { get; private set; }
        public RaceType LastRaceType { get; private set; }

        public enum RaceType
        {
            Race,
            Replay,
            None,
        }

        private async void Start()
        {
            // do loading operations
            PlayerProfileManager.Instance.LoadProfiles();

            await CharacterManager.LoadCharactersAsync();
            await LevelManager.LoadLevelsAsync();
            m_raceManagerPrefab = await Resources.LoadAsync<RaceManager>("RaceManager") as RaceManager;

            // load the main menu
            LastRaceType = RaceType.None;

            AsyncOperation loading = LoadMainMenu();
            loading.allowSceneActivation = true;
        }

        private void FixedUpdate()
        {
            InterpolationController.Instance.EarlyFixedUpdate();

            if (RaceManager != null)
            {
                RaceManager.FixedUpdateRace();
            }
        }

        private void Update()
        {
            // lock the cursor if required
            bool useCursor = Input.GetKey(KeyCode.LeftControl);
            Cursor.lockState = true ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = useCursor;

            // main update loop
            InputManager.Instance.Update();
            InterpolationController.Instance.VisualUpdate();

            if (RaceManager != null)
            {
                RaceManager.UpdateRace();
            }

            // save a screenshot if requested
            if (Input.GetKeyDown(KeyCode.F11))
            {
                ScreenCapture.CaptureScreenshot($"screenshot_{DateTime.Now.ToString("MM-dd-yy_H-mm-ss")}.png");
            }
        }

        private void LateUpdate()
        {
            if (RaceManager != null)
            {
                RaceManager.LateUpdateRace();
            }
        }

        public AsyncOperation LoadMainMenu()
        {
            AsyncOperation loading = SceneManager.LoadSceneAsync(1);
            StartCoroutine(LoadLevel(loading, () => StartMainMenu()));
            return loading;
        }

        private void StartMainMenu()
        {
            RaceManager = null;
        }

        public AsyncOperation LoadRace(RaceParameters raceParams)
        {
            LastRaceType = RaceType.Race;
            LastRaceParams = raceParams;

            AsyncOperation loading = SceneManager.LoadSceneAsync(raceParams.level.SceneName);
            StartCoroutine(LoadLevel(loading, () => StartRace(raceParams)));
            return loading;
        }

        public AsyncOperation LoadRace(RaceRecording recording)
        {
            LastRaceType = RaceType.Replay;
            LastRaceParams = null;

            AsyncOperation loading = SceneManager.LoadSceneAsync(recording.RaceParams.level.SceneName);
            StartCoroutine(LoadLevel(loading, () => StartRace(recording)));
            return loading;
        }

        private void StartRace(RaceParameters raceParams)
        {
            RaceManager = Instantiate(m_raceManagerPrefab);
            RaceManager.LoadRace(raceParams);
        }

        private void StartRace(RaceRecording recording)
        {
            RaceManager = Instantiate(m_raceManagerPrefab);
            RaceManager.LoadReplay(recording);
        }

        private IEnumerator LoadLevel(AsyncOperation loading, Action onComplete = null)
        {
            Application.backgroundLoadingPriority = ThreadPriority.BelowNormal;
            loading.allowSceneActivation = false;

            yield return new WaitWhile(() => !loading.allowSceneActivation);

            AudioManager.Instance.Volume = 0f;
            AudioManager.Instance.StopMusic();
            AudioManager.Instance.StopSounds();

            AudioListener.volume = 0f;
            AudioListener.pause = false;

            Time.timeScale = 1f;

            yield return new WaitWhile(() => !loading.isDone);

            Resources.UnloadUnusedAssets();

            AudioManager.Instance.Volume = 1f;
            AudioManager.Instance.MusicVolume = 1f;

            onComplete?.Invoke();
        }
    }
}
