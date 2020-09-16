using System.IO;

using Framework;

using UnityEditor;
using UnityEditor.Build.Reporting;

using UnityEngine;

namespace BoostBlasters
{
    /// <summary>
    /// A class containing methods for managing builds.
    /// </summary>
    public class Build
    {
        private static string BuildPath => $"{Application.dataPath}/../../Build/{Application.version}";

        private static string ProductName => Application.productName.Replace(" ", "");
        private static string ExecutableName => $"{ProductName}.exe";

        /// <summary>
        /// Builds the main executable and the asset bundles.
        /// </summary>
        [MenuItem("BoostBlasters/Build All", priority = 100)]
        public static void DoBuild()
        {
            var buildPlayerOptions = new BuildPlayerOptions
            {
                locationPathName = $"{BuildPath}/{ExecutableName}",
                target = EditorUserBuildSettings.activeBuildTarget,
                options = BuildOptions.None
            };

            BuildProject(buildPlayerOptions);
            BuildBundles(false);
        }

        /// <summary>
        /// Builds only the asset bundles.
        /// </summary>
        [MenuItem("BoostBlasters/Build Bundles", priority = 105)]
        public static void DoBuildBundles()
        {
            BuildBundles(false);
        }

        /// <summary>
        /// Builds the main executable and the asset bundles, then lanches the executable.
        /// </summary>
        [MenuItem("BoostBlasters/Build All + Run", priority = 110)]
        public static void DoBuildRun()
        {
            var buildPlayerOptions = new BuildPlayerOptions
            {
                locationPathName = $"{BuildPath}/{ExecutableName}",
                target = EditorUserBuildSettings.activeBuildTarget,
                options = BuildOptions.None
            };

            BuildProject(buildPlayerOptions);
            BuildBundles(false);

            Debug.Log("Launching build...");
            System.Diagnostics.Process.Start(buildPlayerOptions.locationPathName);
        }

        /// <summary>
        /// Builds the main game executable.
        /// </summary>
        /// <param name="options">The build options.</param>
        public static void BuildProject(BuildPlayerOptions options)
        {
            Debug.Log("Starting build...");

            // only use the mandetory scenes
            options.scenes = new string[]
            {
                "Assets/Scenes/main.unity",
                "Assets/Scenes/MainMenu/menu.unity",
            };

            // build the game
            var report = BuildPipeline.BuildPlayer(options);
            var summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"Build completed: size={StringUtils.BytesToString((long)summary.totalSize)}");
            }
            if (summary.result == BuildResult.Failed)
            {
                Debug.LogError("Failed to build game!");
                return;
            }
        }

        /// <summary>
        /// Builds the game asset bundles.
        /// </summary>
        /// <param name="isEditor">If true builds the asset bundles used for play mode in the editor.</param>
        /// <param name="cleanBuild">If true, cleans the bundle directories before building the new bundles.</param>
        public static void BuildBundles(bool isEditor, bool cleanBuild = true)
        {
            // get the build path
            string path;

            if (isEditor)
            {
                path = $"{Application.dataPath}/../Bundles/";
            }
            else
            {
                path = $"{BuildPath}/{ProductName}_Data/Bundles/";
            }

            // create a clean folder for the asset bundles
            if (Directory.Exists(path))
            {
                Debug.Log("Clearing asset bundle directory...");

                new DirectoryInfo(path).Delete(true);
            }

            Directory.CreateDirectory(path);

            // build the asset bundles
            Debug.Log("Building asset bundles...");

            var options =
                BuildAssetBundleOptions.StrictMode |
                BuildAssetBundleOptions.DisableLoadAssetByFileNameWithExtension;

            if (cleanBuild)
            {
                options |= BuildAssetBundleOptions.ForceRebuildAssetBundle;
            }

            var manifest = BuildPipeline.BuildAssetBundles(
                path,
                options,
                EditorUserBuildSettings.activeBuildTarget
            );

            if (manifest == null)
            {
                Debug.LogError("Failed to build asset bundles!");
                return;
            }

            // verify that there are no unexpected dependencies between asset bundles
            foreach (var bundle in manifest.GetAllAssetBundles())
            {
                foreach (var dependency in manifest.GetAllDependencies(bundle))
                {
                    Debug.LogError($"Asset bundle \"{bundle}\" has dependancy \"{dependency}\"! Asset bundles are expected to have no dependencies.");
                }
            }

            Debug.Log("Asset bundle build completed");
        }
    }
}
