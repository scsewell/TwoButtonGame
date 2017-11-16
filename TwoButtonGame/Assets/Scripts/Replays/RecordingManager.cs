using System;
using System.IO;
using Framework;
using Framework.IO;

public class RecordingManager : Singleton<RecordingManager>
{
    private static readonly string FOLDER_NAME = "Replays/";
    private static readonly string FILE_EXTENTION = ".rep";

    public void SaveRecording(RaceRecording recording)
    {
        byte[] content = Compression.Compress(recording.ToBytes());
        string name = "Replay_" + DateTime.Now.ToString("MM-dd-yy_H-mm") + FILE_EXTENTION;

        FileIO.WriteFile(content, GetReplayDir() + name);
    }
    
    public void GetRecordings(RaceRecording recording)
    {
        foreach (FileInfo file in FileIO.GetFiles(GetReplayDir(), FILE_EXTENTION))
        {
            if (file.Extension == FILE_EXTENTION)
            {
                byte[] content = FileIO.ReadFileBytes(file.FullName);
                RaceRecording rec = new RaceRecording(Compression.Decompress(content));
            }
        }
    }

    private string GetReplayDir()
    {
        return FileIO.GetInstallDirectory() + FOLDER_NAME;
    }
}
