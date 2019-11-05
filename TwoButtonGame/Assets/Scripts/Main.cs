using System;
using System.Threading.Tasks;

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
    /// Manages the update loop and core game state.
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
            // we want nice and smooth loading
            Application.backgroundLoadingPriority = ThreadPriority.Normal;

            // start loading operations
            var loadProfiles = ProfileManager.LoadProfilesAsync();
            var loadCharacters = CharacterManager.LoadCharactersAsync();
            var loadlevels = LevelManager.LoadLevelsAsync();
            var loadRaceManager = Resources.LoadAsync<RaceManager>("RaceManager");

            // wait for loading operations to complete and get the results if needed
            m_raceManagerPrefab = await loadRaceManager as RaceManager;

            await Task.WhenAll(loadProfiles, loadCharacters, loadlevels);

            // load the main menu
            LastRaceType = RaceType.None;

            LoadMainMenu();
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
            bool freeCursor = Input.GetKey(KeyCode.LeftControl);
            Cursor.lockState = true ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = freeCursor;

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

        /// <summary>
        /// Loads the main menu scene.
        /// </summary>
        /// <param name="doLoad">A function which is true when the actual scene transition
        /// should be allowed to occur.</param>
        public async void LoadMainMenu(Func<bool> doLoad = null)
        {
            AsyncOperation op = SceneManager.LoadSceneAsync(1);
            await LoadScene(op, doLoad);

            RaceManager = null;
        }

        /// <summary>
        /// Starts a race.
        /// </summary>
        /// <param name="raceParams">The race configuration.</param>
        /// <param name="doLoad">A function which is true when the actual scene transition
        /// should be allowed to occur.</param>
        public async void LoadRace(RaceParameters raceParams, Func<bool> doLoad = null)
        {
            LastRaceType = RaceType.Race;
            LastRaceParams = raceParams;

            string scene = await raceParams.level.Scene.GetAsync();
            AsyncOperation op = SceneManager.LoadSceneAsync(scene);
            await LoadScene(op, doLoad);

            RaceManager = Instantiate(m_raceManagerPrefab);
            RaceManager.LoadRace(raceParams);
        }

        /// <summary>
        /// Starts a replay.
        /// </summary>
        /// <param name="recording">The replay to view.</param>
        /// <param name="doLoad">A function which is true when the actual scene transition
        /// should be allowed to occur.</param>
        public async void LoadRace(RaceRecording recording, Func<bool> doLoad = null)
        {
            LastRaceType = RaceType.Replay;
            LastRaceParams = null;

            string scene = await recording.RaceParams.level.Scene.GetAsync();
            AsyncOperation op = SceneManager.LoadSceneAsync(scene);
            await LoadScene(op, doLoad);

            RaceManager = Instantiate(m_raceManagerPrefab);
            RaceManager.LoadReplay(recording);
        }

        private async Task LoadScene(AsyncOperation op, Func<bool> doLoad)
        {
            // wait until the caller wants the scene transition to occur
            if (doLoad != null)
            {
                op.allowSceneActivation = false;

                await new WaitWhile(() => !doLoad());
            }

            // clear unwanted persistent state
            AudioManager.Instance.StopMusic();
            AudioManager.Instance.StopSounds();

            // wait for scene transition to complete
            op.allowSceneActivation = true;
            await op;

            // cleanup unused objects
            GC.Collect();

            // restore default state
            AudioManager.Instance.MusicVolume = 1f; 
            AudioManager.Instance.Volume = 1f;

            AudioListener.pause = false;

            Time.timeScale = 1f;
        }
    }
}
