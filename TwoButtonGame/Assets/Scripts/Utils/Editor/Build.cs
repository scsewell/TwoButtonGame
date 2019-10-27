using System;
using System.Linq;
using System.IO;

using UnityEngine;

using UnityEditor;
using UnityEditor.Build.Reporting;

using Framework;

namespace BoostBlasters
{
    public class Build
    {
        private static string BuildPath => $"{Application.dataPath}/../../Build/{Application.version}";

        private static string ProductName => Application.productName.Replace(" ", "");
        private static string ExecutableName => $"{ProductName}.exe";

        [MenuItem("BoostBlasters/Build _F5", priority = 100)]
        public static void DoBuild()
        {
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.locationPathName = $"{BuildPath}/{ExecutableName}";
            buildPlayerOptions.target = EditorUserBuildSettings.activeBuildTarget;
            buildPlayerOptions.options = BuildOptions.None;

            BuildProject(buildPlayerOptions);
            BuildBundles(GetBundleDirectory());
        }

        [MenuItem("BoostBlasters/Build + Run _F6", priority = 105)]
        public static void DoBuildRun()
        {
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.locationPathName = $"{BuildPath}/{ExecutableName}";
            buildPlayerOptions.target = EditorUserBuildSettings.activeBuildTarget;
            buildPlayerOptions.options = BuildOptions.None | BuildOptions.AutoRunPlayer;

            BuildProject(buildPlayerOptions);
            BuildBundles(GetBundleDirectory());
        }

        [MenuItem("BoostBlasters/Build Bundles _F7", priority = 110)]
        public static void DoBuildBundles()
        {
            BuildBundles(GetBundleDirectory());
        }

        private static void BuildProject(BuildPlayerOptions options)
        {
            Debug.Log("Starting build...");

            // only use the mandetory scenes
            options.scenes = new string[]
            {
                "Assets/Scenes/main.unity",
                "Assets/Scenes/MainMenu/menu.unity",
            };

            // build the game
            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;

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

        private static void BuildBundles(string path)
        {
            // create a clean folder for the asset bundles
            if (Directory.Exists(path))
            {
                Debug.Log("Clearing asset bundle directory...");

                new DirectoryInfo(path).Delete(true);
            }

            Directory.CreateDirectory(path);

            // build the asset bundles
            Debug.Log("Building asset bundles...");

            AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(
                path,
                BuildAssetBundleOptions.StrictMode |
                BuildAssetBundleOptions.DisableLoadAssetByFileName |
                BuildAssetBundleOptions.DisableLoadAssetByFileNameWithExtension,
                EditorUserBuildSettings.activeBuildTarget
            );

            if (manifest == null)
            {
                Debug.LogError("Failed to build asset bundles!");
                return;
            }

            // verify that there are no unexpected dependencies between asset bundles
            foreach (string bundle in manifest.GetAllAssetBundles())
            {
                foreach (string dependency in manifest.GetAllDependencies(bundle))
                {
                    Debug.LogError($"Asset bundle \"{bundle}\" has dependancy \"{dependency}\"! Asset bundles are expected to have no dependencies.");
                }
            }

            Debug.Log("Asset bundle build completed");
        }

        private static string GetBundleDirectory()
        {
            return $"{BuildPath}/{ProductName}_Data/Bundles/";
        }
    }
}
