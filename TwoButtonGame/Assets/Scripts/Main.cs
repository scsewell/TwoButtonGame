using System;
using System.Threading.Tasks;

using BoostBlasters.Characters;
using BoostBlasters.Levels;
using BoostBlasters.Profiles;
using BoostBlasters.Races;

using Framework;
using Framework.Interpolation;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting;

namespace BoostBlasters
{
    /// <summary>
    /// Manages initialization, scene transitions, and the update loop.
    /// </summary>
    public class Main : ComponentSingleton<Main>
    {
        /// <summary>
        /// How many nanoseconds per frame the GC is allowed to use.
        /// </summary>
        private static readonly ulong GC_SLICE_DURATION = 2 * 1000 * 1000;


        private RaceManager m_raceManagerPrefab = null;

        public RaceManager RaceManager { get; private set; }

        /// <summary>
        /// The configuration of the last race started.
        /// </summary>
        public RaceParameters LastRaceParams { get; private set; }


        private async void Start()
        {
            // TEMPORARY FIX!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! Unity is spamming event system warnings every frame :(
            var logger = Debug.unityLogger;
            logger.filterLogType = LogType.Assert;

            // we don't use the cursor
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = false;

            // configure how much time the gc can use
            GarbageCollector.incrementalTimeSliceNanoseconds = GC_SLICE_DURATION;

            // we want nice and smooth loading
            Application.backgroundLoadingPriority = ThreadPriority.Normal;

            // start loading operations
            var loadProfiles = Profile.LoadProfilesAsync();
            var loadCharacters = CharacterManager.LoadCharactersAsync();
            var loadlevels = LevelManager.LoadLevelsAsync();
            var loadRaceManager = Resources.LoadAsync<RaceManager>("RaceManager");

            // wait for loading operations to complete and get the results if needed
            m_raceManagerPrefab = await loadRaceManager as RaceManager;

            await Task.WhenAll(loadProfiles, loadCharacters, loadlevels);

            // load the main menu
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
        /// <param name="raceParams">The configuration of the race.</param>
        /// <param name="doLoad">A function which is true when the actual scene transition
        /// should be allowed to occur.</param>
        public async void LoadRace(RaceParameters raceParams, Func<bool> doLoad = null)
        {
            LastRaceParams = raceParams;

            var scene = await raceParams.Level.Scene.GetAsync();
            var op = SceneManager.LoadSceneAsync(scene);
            await LoadScene(op, doLoad);

            RaceManager = Instantiate(m_raceManagerPrefab);
            RaceManager.LoadRace(raceParams);
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
