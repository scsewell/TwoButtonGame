using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Framework.IO;

using UnityEngine;

namespace BoostBlasters.Replays
{
    /// <summary>
    /// Manages the loading and saving of race recordings.
    /// </summary>
    public static class RecordingManager
    {
        /// <summary>
        /// The name of the folder to store replays under.
        /// </summary>
        private static readonly string FOLDER_NAME = "Replays";

        /// <summary>
        /// The format of the date used in the file name.
        /// </summary>
        private static readonly string FILE_DATE_FORMAT = "yyyy-MM-dd_HH-mm-ss";

        /// <summary>
        /// The file extention used for replays.
        /// </summary>
        private static readonly string FILE_EXTENTION = ".rep";

        /// <summary>
        /// The maximum number of replays kept, beyond which the oldest are deleted.
        /// </summary>
        private static readonly int MAX_REPLAYS = 100;


        private static readonly object m_replayLock = new object();
        private static readonly List<RecordingInfo> m_replays = new List<RecordingInfo>();
        private static bool m_isRefreshing;

        /// <summary>
        /// Gets a new list with all currently loaded replay information.
        /// </summary>
        public static List<RecordingInfo> Replays
        {
            get
            {
                lock (m_replayLock)
                {
                    return new List<RecordingInfo>(m_replays);
                }
            }
        }

        /// <summary>
        /// Checks if the replay information is currently being refreshed.
        /// </summary>
        public static bool IsRefreshing
        {
            get
            {
                lock (m_replayLock)
                {
                    return m_isRefreshing;
                }
            }
        }


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Init()
        {
            m_replays.Clear();
            m_isRefreshing = false;
        }

        /// <summary>
        /// Loads all exising replay file information.
        /// </summary>
        public static async void RefreshRecordingsAsync()
        {
            lock (m_replayLock)
            {
                m_replays.Clear();
                m_isRefreshing = true;
            }

            await Task.Run(() => GetReplays());

            lock (m_replayLock)
            {
                m_isRefreshing = false;
            }
        }

        /// <summary>
        /// Loads a replay file asynchronously.
        /// </summary>
        /// <param name="info">The info of the recording to load.</param>
        /// <returns>The race recording.</returns>
        public static async Task<Recording> LoadReplayAsync(RecordingInfo info)
        {
            return await Task.Run(() => LoadReplay(info));
        }

        /// <summary>
        /// Loads a replay file.
        /// </summary>
        /// <param name="info">The info of the recording to load.</param>
        /// <returns>The race recording.</returns>
        private static Recording LoadReplay(RecordingInfo info)
        {
            if (FileIO.OpenFileStream(info.File.FullName, out var stream))
            {
                using (var reader = new DataReader(stream))
                {
                    var recording = new Recording(reader);

                    Debug.Log($"Loaded replay \"{info.File.Name}\"");
                    return recording;
                }
            }
            else
            {
                Debug.LogError($"Failed to load replay \"{info.File.Name}\"!");
                return null;
            }
        }

        /// <summary>
        /// Saves a replay file asynchronously.
        /// </summary>
        /// <param name="replay">The replay to save.</param>
        public static async void SaveReplayAsync(Recording replay)
        {
            await Task.Run(() => SaveReplay(replay));
        }

        /// <summary>
        /// Saves a replay file.
        /// </summary>
        /// <param name="replay">The replay to save.</param>
        private static void SaveReplay(Recording replay)
        {
            var name = $"Replay_{DateTime.Now.ToString(FILE_DATE_FORMAT)}{FILE_EXTENTION}";
            var path = Path.Combine(GetReplayDir(), name);

            using (var writer = new DataWriter())
            {
                if (replay.Serialize(writer) && FileIO.WriteFile(path, writer.GetBytes()))
                {
                    Debug.Log($"Saved replay \"{name}\"");
                }
                else
                {
                    Debug.LogError($"Failed to save replay!");
                }
            }
        }

        private static void GetReplays()
        {
            var files = FileIO.GetFiles(GetReplayDir(), FILE_EXTENTION).ToList();
            files.Sort((x, y) => y.CreationTime.CompareTo(x.CreationTime));

            foreach (var file in files)
            {
                // remove replays if there are too many
                int replayCount;

                lock (m_replayLock)
                {
                    replayCount = m_replays.Count;
                }

                if (replayCount >= MAX_REPLAYS)
                {
                    File.Delete(file.FullName);
                    continue;
                }

                // try to load the recording information
                if (FileIO.OpenFileStream(file.FullName, out var stream))
                {
                    using (var reader = new DataReader(stream))
                    {
                        var recording = new Recording(reader, true);

                        if (recording.IsValid)
                        {
                            var info = new RecordingInfo(file, recording.Params, recording.Results);

                            lock (m_replayLock)
                            {
                                m_replays.Add(info);
                            }

                            Debug.Log($"Loaded replay info for \"{file.Name}\"");
                        }
                    }
                }
                else
                {
                    Debug.LogError($"Failed to load replay info for \"{file.Name}\"!");
                }
            }
        }

        private static string GetReplayDir()
        {
            return Path.Combine(FileIO.GetConfigDirectory(), FOLDER_NAME);
        }
    }
}