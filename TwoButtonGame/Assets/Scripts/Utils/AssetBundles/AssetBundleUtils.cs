using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;

using UnityEngine;

namespace BoostBlasters
{
    /// <summary>
    /// Utilities for handling asset bundles.
    /// </summary>
    public static class AssetBundleUtils
    {
        /// <summary>
        /// Loads an asset bundle.
        /// </summary>
        /// <param name="bundlePath">The path of the bundles relative to the bundles folder.</param
        /// <param name="bundleName">The name of the asset bundle to load.</param>
        /// <returns>The asset bundle.</returns>
        public static async Task<AssetBundle> LoadBundleAsync(string bundlePath, string bundleName)
        {
            // get the bundle directory
            string bundlesPath = GetBundlesPath(bundlePath);

            DirectoryInfo directory = new DirectoryInfo(bundlesPath);
            if (!directory.Exists)
            {
                Debug.LogError($"Asset bundle directory \"{bundlesPath}\" does not exist!");
                return null;
            }

            // get the path to the bundle
            FileInfo file = new FileInfo($"{bundlesPath}/{bundleName}");

            if (!file.Exists)
            {
                Debug.LogWarning($"No asset bundle found at path \"{file.FullName}\"!");
                return null;
            }

            // start loading the asset bundle
            AssetBundleCreateRequest loadBundle = AssetBundle.LoadFromFileAsync(file.FullName);

            // wait until all the bundle has loaded
            while (!loadBundle.isDone)
            {
                await Task.Yield();
            }

            return loadBundle.assetBundle;
        }

        /// <summary>
        /// Loads a type of asset by name from asset bundles at a given path. The asset bundles
        /// are unloading after any asset has been loaded from them.
        /// </summary>
        /// <typeparam name="T">The type of asset to load from the bundles.</typeparam>
        /// <param name="bundlePath">The path of the bundles relative to the bundles folder.</param>
        /// <param name="assetName">The name of the asset to load from the bundle.</param>
        /// <returns>The loaded assets.</returns>
        public static async Task<T[]> LoadAssetsAsync<T>(string bundlePath, string assetName) where T : Object
        {
            // get the bundle directory
            string bundlesPath = GetBundlesPath(bundlePath);

            DirectoryInfo directory = new DirectoryInfo(bundlesPath);
            if (!directory.Exists)
            {
                Debug.LogError($"Asset bundle directory \"{bundlesPath}\" does not exist!");
                return new T[0];
            }

            // find the available bundles
            List<string> bundlePaths = new List<string>();

            foreach (FileInfo file in directory.EnumerateFiles())
            {
                // asset bundle files should have no extention
                if (string.IsNullOrEmpty(file.Extension))
                {
                    bundlePaths.Add(file.FullName);
                }
            }

            if (bundlePaths.Count <= 0)
            {
                Debug.LogWarning($"Found no asset bundles at path \"{bundlesPath}\"!");
                return new T[0];
            }

            // start loading the asset bundles
            List<T> assets = new List<T>();

            foreach (string path in bundlePaths)
            {
                AssetBundleCreateRequest loadBundle = AssetBundle.LoadFromFileAsync(path);
                loadBundle.completed += (b) =>
                {
                    if (loadBundle.assetBundle != null)
                    {
                        AssetBundleRequest loadAsset = loadBundle.assetBundle.LoadAssetAsync<T>(assetName);
                        loadAsset.completed += (a) =>
                        {
                            if (loadAsset.asset != null)
                            {
                                assets.Add(loadAsset.asset as T);

                                // unload the bundle once we have the asset we need
                                loadBundle.assetBundle.Unload(false);
                            }
                        };
                    }
                };
            }

            // wait until all the files have loaded
            while (assets.Count != bundlePaths.Count)
            {
                await Task.Yield();
            }

            return assets.ToArray();
        }

        private static string GetBundlesPath(string bundlePath)
        {
            return $"{Application.dataPath}{(Application.isEditor ? "/.." : "")}/Bundles/{bundlePath}/";
        }
    }
}
