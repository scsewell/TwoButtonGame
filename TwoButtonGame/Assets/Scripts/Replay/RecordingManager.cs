using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;

using UnityEngine;

using Framework;
using Framework.IO;

using BoostBlasters.Races;

namespace BoostBlasters.Replays
{
    /// <summary>
    /// Manages the loading and saving of race recordings.
    /// </summary>
    public class RecordingManager : Singleton<RecordingManager>
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


        private readonly object m_replayLock = new object();
        private readonly List<RecordingInfo> m_replays = new List<RecordingInfo>();
        private bool m_isRefreshing = false;

        /// <summary>
        /// Gets all new list with all currently loaded replay information.
        /// </summary>
        public List<RecordingInfo> Replays
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
        public bool IsRefreshing
        {
            get
            {
                lock (m_replayLock)
                {
                    return m_isRefreshing;
                }
            }
        }


        /// <summary>
        /// Loads all exising replay file information.
        /// </summary>
        public async void RefreshRecordings()
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
        /// Loads a replay file.
        /// </summary>
        /// <param name="info">The info of the recording to load.</param>
        /// <returns>The race recording.</returns>
        public Recording LoadReplay(RecordingInfo info)
        {
            if (FileIO.ReadFileBytes(info.File.FullName, out byte[] bytes))
            {
                Recording recording = new Recording(bytes);

                Debug.Log($"Loaded replay \"{info.File.Name}\"");
                return recording;
            }
            else
            {
                Debug.LogError($"Failed to load replay \"{info.File.Name}\"!");
                return null;
            }
        }

        /// <summary>
        /// Saves a replay file.
        /// </summary>
        /// <param name="replay">The replay to save.</param>
        public void SaveReplay(Recording replay)
        {
            string name = $"Replay_{DateTime.Now.ToString(FILE_DATE_FORMAT)}{FILE_EXTENTION}";
            string path = Path.Combine(GetReplayDir(), name);

            if (FileIO.WriteFile(path, replay.ToBytes()))
            {
                Debug.Log($"Saved replay \"{name}\"");
            }
            else
            {
                Debug.LogError($"Failed to save replay!");
            }
        }

        private void GetReplays()
        {
            List<FileInfo> files = FileIO.GetFiles(GetReplayDir(), FILE_EXTENTION).ToList();
            files.Sort((x, y) => y.CreationTime.CompareTo(x.CreationTime));

            foreach (FileInfo file in files)
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
                if (FileIO.ReadFileBytes(file.FullName, out byte[] bytes))
                {
                    using (DataReader reader = new DataReader(bytes))
                    {
                        Recording.ParseHeader(reader, out RaceParameters raceParams, out RaceResult[] raceResults);
                        RecordingInfo info = new RecordingInfo(file, raceParams, raceResults);

                        lock (m_replayLock)
                        {
                            m_replays.Add(info);
                        }
                    }

                    Debug.Log($"Loaded replay info for \"{file.Name}\"");
                }
                else
                {
                    Debug.LogError($"Failed to load replay info for \"{file.Name}\"!");
                }
            }
        }

        private string GetReplayDir()
        {
            return Path.Combine(FileIO.GetConfigDirectory(), FOLDER_NAME);
        }
    }
}