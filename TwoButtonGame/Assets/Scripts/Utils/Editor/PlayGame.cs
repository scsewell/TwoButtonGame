using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace BoostBlasters
{
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
                string scenePath = EditorPrefs.GetString(PrefKey, string.Empty);

                if (!string.IsNullOrWhiteSpace(scenePath))
                {
                    EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                }
            }
        }

        [MenuItem("BoostBlasters/Play _F3", priority = 10)]
        public static void DoPlay()
        {
            Play();
        }

        [MenuItem("BoostBlasters/Bundles + Play _F4", priority = 12)]
        public static void DoPlayBundles()
        {
            Build.BuildBundles(true);
            Play();
        }

        private static void Play()
        {
            // remember the currently open scene
            EditorPrefs.SetString(PrefKey, SceneManager.GetActiveScene().path);

            // open the load scene
            EditorSceneManager.OpenScene("Assets/Scenes/main.unity", OpenSceneMode.Single);
            EditorApplication.EnterPlaymode();
        }
    }
}
