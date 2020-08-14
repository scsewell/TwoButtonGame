using System;
using System.Threading.Tasks;

using BoostBlasters.Characters;
using BoostBlasters.Levels;
using BoostBlasters.Profiles;
using BoostBlasters.Races;
using BoostBlasters.Replays;

using Framework;
using Framework.Interpolation;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting;

namespace BoostBlasters
{
    /// <summary>
    /// Manages the update loop and core game state.
    /// </summary>
    public class Main : ComponentSingleton<Main>
    {
        /// <summary>
        /// How many nanoseconds per frame the GC is allowed to use.
        /// </summary>
        private static readonly ulong GC_SLICE_DURATION = 2 * 1000 * 1000;


        private RaceManager m_raceManagerPrefab = null;

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
            // we don't use the cursor
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = false;

            // configure how much time the gc can use
            GarbageCollector.incrementalTimeSliceNanoseconds = GC_SLICE_DURATION;

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
            InterpolationController.EarlyFixedUpdate();

            if (RaceManager != null)
            {
                RaceManager.FixedUpdateRace();
            }
        }

        private void Update()
        {
            InterpolationController.VisualUpdate();

            if (RaceManager != null)
            {
                RaceManager.UpdateRace();
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
            var op = SceneManager.LoadSceneAsync(1);
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

            var scene = await raceParams.Level.Scene.GetAsync();
            var op = SceneManager.LoadSceneAsync(scene);
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
        public async void LoadRace(Recording recording, Func<bool> doLoad = null)
        {
            LastRaceType = RaceType.Replay;
            LastRaceParams = null;

            var scene = await recording.Params.Level.Scene.GetAsync();
            var op = SceneManager.LoadSceneAsync(scene);
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
            GarbageCollector.CollectIncremental(GC_SLICE_DURATION);

            // restore default state
            AudioManager.Instance.MusicVolume = 1f;
            AudioManager.Instance.Volume = 1f;

            AudioListener.pause = false;

            Time.timeScale = 1f;
        }
    }
}
