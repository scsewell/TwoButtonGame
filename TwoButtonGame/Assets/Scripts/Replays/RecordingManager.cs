using System;
using System.Collections.Generic;
using System.IO;
using Framework;
using Framework.IO;

public class RecordingManager : Singleton<RecordingManager>
{
    private static readonly string FOLDER_NAME = "Replays/";
    private static readonly string FILE_EXTENTION = ".rep";

    public void SaveRecording(RaceRecording recording, List<Player> players)
    {
        string name = "Replay_" + DateTime.Now.ToString("MM-dd-yy_H-mm") + FILE_EXTENTION;

        FileIO.WriteFile(recording.ToBytes(players), GetReplayDir() + name);
    }
    
    public List<RecordingInfo> GetRecordings()
    {
        List<RecordingInfo> recInfos = new List<RecordingInfo>();
        
        foreach (FileInfo file in FileIO.GetFiles(GetReplayDir(), FILE_EXTENTION))
        {
            if (file.Extension == FILE_EXTENTION)
            {
                byte[] content = FileIO.ReadFileBytes(file.FullName);
                Framework.IO.BinaryReader reader = new Framework.IO.BinaryReader(content);

                RaceParameters raceParams = RaceRecording.ParseRaceParams(reader);
                RaceResult[] raceResults = RaceRecording.ParseRaceResults(reader, raceParams.PlayerCount);

                recInfos.Add(new RecordingInfo(file, raceParams, raceResults));
            }
        }
        return recInfos;
    }

    public RaceRecording LoadRecording(RecordingInfo info)
    {
        return new RaceRecording(FileIO.ReadFileBytes(info.File.FullName));
    }

    private string GetReplayDir()
    {
        return FileIO.GetInstallDirectory() + FOLDER_NAME;
    }
}
