using UnityEditor;
using UnityEditor.SceneManagement;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace BoostBlasters
{
    /// <summary>
    /// Utility methods for entering play mode in the correct scene.
    /// </summary>
    public static class PlayGame
    {
        private static string PrefKey => $"{Application.productName}-{nameof(PlayGame)}-Scene";

        [InitializeOnLoadMethod]
        private static void SetUpPlayMode()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            // if we know what scene was open before we began play mode, go back to it
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                var scenePath = EditorPrefs.GetString(PrefKey, string.Empty);

                if (!string.IsNullOrWhiteSpace(scenePath))
                {
                    EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                }
            }
        }

        /// <summary>
        /// Starts the game in play mode without updating the asset bundles.
        /// </summary>
        [MenuItem("BoostBlasters/Play _F3", priority = 10)]
        public static void DoPlay()
        {
            Play();
        }

        /// <summary>
        /// Starts the game in play mode after building asset bundles.
        /// </summary>
        [MenuItem("BoostBlasters/Bundles + Play", priority = 12)]
        public static void DoPlayBundles()
        {
            Build.BuildBundles(true);
            Play();
        }

        private static void Play()
        {
            // remember the currently open scene
            EditorPrefs.SetString(PrefKey, SceneManager.GetActiveScene().path);

            // save the scene if it is dirty
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

            // open the load scene
            EditorSceneManager.OpenScene("Assets/Scenes/main.unity", OpenSceneMode.Single);
            EditorApplication.EnterPlaymode();
        }
    }
}
