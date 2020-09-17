using System;
using System.IO;

using Framework;

using UnityEditor;
using UnityEditor.Build.Reporting;

using UnityEngine;
using UnityEditor.Build.Pipeline;
using System.Linq;

namespace BoostBlasters
{
    /// <summary>
    /// A class containing methods for managing builds.
    /// </summary>
    public class Build
    {
        private static string ProductName => Application.productName.Replace(" ", "");
        private static string ExecutableName => $"{ProductName}.exe";

        private static string BuildDir => $"{Application.dataPath}/../../Build/{Application.version}";
        private static string BundleDir => $"{BuildDir}/{ProductName}_Data/Bundles/";

        private static string EditorBundleDir => $"{Application.dataPath}/../Bundles/";

        /// <summary>
        /// Builds only the asset bundles.
        /// </summary>
        [MenuItem("BoostBlasters/Build Bundles", priority = 105)]
        public static void BuildBundles()
        {
            BuildBundles(false);
        }

        /// <summary>
        /// Builds the main executable and the asset bundles.
        /// </summary>
        [MenuItem("BoostBlasters/Build All", priority = 100)]
        public static void BuildAll()
        {
            var buildPlayerOptions = new BuildPlayerOptions
            {
                locationPathName = $"{BuildDir}/{ExecutableName}",
                target = EditorUserBuildSettings.activeBuildTarget,
                options = BuildOptions.None
            };

            BuildProject(buildPlayerOptions);
            BuildBundles(false);
        }

        /// <summary>
        /// Builds the main executable and the asset bundles, then lanches the executable.
        /// </summary>
        [MenuItem("BoostBlasters/Build All + Run", priority = 110)]
        public static void BuildAllAndRun()
        {
            var buildPlayerOptions = new BuildPlayerOptions
            {
                locationPathName = $"{BuildDir}/{ExecutableName}",
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

        private class CustomBuildParameters : BundleBuildParameters
        {
            public CustomBuildParameters(BuildTarget target, BuildTargetGroup group, string outputFolder) :
                base(target, group, outputFolder)
            {
            }

            public override BuildCompression GetCompressionForIdentifier(string identifier)
            {
                var bundleRoot = identifier
                    .Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries)
                    .First();

                switch (bundleRoot)
                {
                    case Bundles.CHARACTERS:
                    case Bundles.LEVELS:
                    case Bundles.LEVEL_SCENES:
                    case Bundles.MUSIC:
                    case Bundles.MUSIC_DATA:
                        return BuildCompression.Uncompressed;
                }
                return BuildCompression.Uncompressed;
            }
        }

        /// <summary>
        /// Builds the game asset bundles.
        /// </summary>
        /// <param name="isEditor">If true builds the asset bundles used for play mode in the editor.</param>
        /// <param name="cleanBuild">If true, cleans the bundle directories before building the new bundles.</param>
        public static void BuildBundles(bool isEditor, bool cleanBuild = true)
        {
            var path = isEditor ? EditorBundleDir : BundleDir;

            // get the content to build
            var bundles = UnityEditor.Build.Content.ContentBuildInterface.GenerateAssetBundleBuilds();
            var buildContent = new BundleBuildContent(bundles);

            // configure the build parameters
            var buildParams = new CustomBuildParameters(
                EditorUserBuildSettings.activeBuildTarget,
                EditorUserBuildSettings.selectedBuildTargetGroup,
                path)
            {
                BundleCompression = BuildCompression.Uncompressed,
                UseCache = !cleanBuild,
            };

            // create or clean folder for the asset bundles
            if (cleanBuild && Directory.Exists(path))
            {
                Debug.Log("Clearing asset bundle directory...");

                new DirectoryInfo(path).Delete(true);
            }

            Directory.CreateDirectory(path);

            // build the asset bundles
            Debug.Log("Building asset bundles...");

            var exitCode = ContentPipeline.BuildAssetBundles(buildParams, buildContent, out var results);

            if (exitCode < 0)
            {
                Debug.LogError($"Failed to build asset bundles with code: {exitCode}!");
                return;
            }

            // verify that there are no unexpected dependencies between asset bundles
            foreach (var bundle in results.BundleInfos)
            {
                foreach (var dependency in bundle.Value.Dependencies)
                {
                    Debug.LogError($"Asset bundle \"{bundle.Key}\" has dependancy \"{dependency}\"! Asset bundles are expected to have no dependencies.");
                }
            }

            Debug.Log("Asset bundle build completed");
        }
    }
}
