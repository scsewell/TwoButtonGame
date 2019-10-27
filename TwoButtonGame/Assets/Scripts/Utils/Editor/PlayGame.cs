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

        /// <summary>
        /// Starts playing the game in the editor.
        /// </summary>
        [MenuItem("BoostBlasters/Play _F5")]
        public static void Play()
        {
            // remember the currently open scene
            EditorPrefs.SetString(PrefKey, SceneManager.GetActiveScene().path);

            // open the load scene
            EditorSceneManager.OpenScene("Assets/Scenes/main.unity", OpenSceneMode.Single);
            EditorApplication.EnterPlaymode();
        }
    }
}
