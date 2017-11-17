using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Framework.IO;

public class RaceRecording
{
    private RaceParameters m_raceParams;
    public RaceParameters RaceParams
    {
        get { return m_raceParams; }
    }

    private int PlayerCount
    {
        get { return RaceParams.PlayerCount; }
    }

    public float Duration
    {
        get { return m_positions[0].Count * FRAMES_PER_POSITION * Time.fixedDeltaTime; }
    }
    
    private static readonly int FRAMES_PER_POSITION = 10;
    private List<Vector3>[] m_positions;
    private List<Vector3>[] m_velocities;
    private List<float>[] m_rotations;
    private List<float>[] m_angularVelocities;

    private static readonly int FRAMES_PER_INPUT = 1;
    private List<float>[] m_h;
    private List<float>[] m_v;
    private List<int>[] m_toggleFramesBoost;

    private MovementInputs[] m_lastFrameInputs;
    private int[][] m_nextInputIndices;


    public RaceRecording(RaceParameters raceParams)
    {
        m_raceParams = raceParams;
        CreateBuffers();
    }
     
    public RaceRecording(byte[] bytes)
    {
        BinaryReader reader = new BinaryReader(bytes);
        
        m_raceParams = ParseRaceParams(reader);
        ParseRaceResults(reader, PlayerCount);

        CreateBuffers();
        
        byte[] compressed = new byte[bytes.Length - reader.GetReadPointer()];
        Array.Copy(bytes, reader.GetReadPointer(), compressed, 0, compressed.Length);
        reader = new BinaryReader(Compression.Decompress(compressed));
        
        for (int i = 0; i < PlayerCount; i++)
        {
            m_positions[i]          = reader.ReadArray<Vector3>().ToList();
            m_velocities[i]         = reader.ReadArray<Vector3>().ToList();
            m_rotations[i]          = reader.ReadArray<float>().ToList();
            m_angularVelocities[i]  = reader.ReadArray<float>().ToList();
            m_h[i]                  = reader.ReadArray<float>().ToList();
            m_v[i]                  = reader.ReadArray<float>().ToList();
            m_toggleFramesBoost[i]  = reader.ReadArray<int>().ToList();
        }
    }

    public static RaceParameters ParseRaceParams(BinaryReader reader)
    {
        LevelConfig level   = Main.Instance.GetLevelConfig(reader.ReadInt());
        int laps            = reader.ReadInt();
        int humanCount      = reader.ReadInt();
        int aiCount         = reader.ReadInt();
        List<PlayerConfig> playerConfigs = reader.ReadArray<int>().Select(id => Main.Instance.GetPlayerConfig(id)).ToList();

        return new RaceParameters(humanCount, new List<int>(), aiCount, playerConfigs, level, laps);
    }

    public static RaceResult[] ParseRaceResults(BinaryReader reader, int playerCount)
    {
        RaceResult[] results = new RaceResult[playerCount];
        
        for (int i = 0; i < playerCount; i++)
        {
            results[i] = new RaceResult(reader.ReadInt(), reader.ReadBool(), reader.ReadArray<float>().ToList());
        }
        return results;
    }

    public byte[] ToBytes(List<Player> players)
    {
        BinaryWriter headerWriter = new BinaryWriter();
        
        headerWriter.WriteValue(m_raceParams.LevelConfig.Id);
        headerWriter.WriteValue(m_raceParams.Laps);
        headerWriter.WriteValue(m_raceParams.HumanCount);
        headerWriter.WriteValue(m_raceParams.AICount);
        headerWriter.WriteArray(m_raceParams.PlayerConfigs.Select(c => c.Id).ToArray());

        for (int i = 0; i < PlayerCount; i++)
        {
            Player player = players[i];
            headerWriter.WriteValue(player.RaceResult.Rank);
            headerWriter.WriteValue(player.RaceResult.Finished);
            headerWriter.WriteArray(player.RaceResult.LapTimes.ToArray());
        }

        BinaryWriter bodyWriter = new BinaryWriter();

        for (int i = 0; i < PlayerCount; i++)
        {
            bodyWriter.WriteArray(m_positions[i].ToArray());
            bodyWriter.WriteArray(m_velocities[i].ToArray());
            bodyWriter.WriteArray(m_rotations[i].ToArray());
            bodyWriter.WriteArray(m_angularVelocities[i].ToArray());
            bodyWriter.WriteArray(m_h[i].ToArray());
            bodyWriter.WriteArray(m_v[i].ToArray());
            bodyWriter.WriteArray(m_toggleFramesBoost[i].ToArray());
        }

        byte[] header = headerWriter.GetBytes();
        byte[] body = bodyWriter.GetBytes();
        byte[] compressed = Compression.Compress(body);

        byte[] full = new byte[header.Length + compressed.Length];
        Array.Copy(header, full, header.Length);
        Array.Copy(compressed, 0, full, header.Length, compressed.Length);
        
        return full;
    }

    private void CreateBuffers()
    {
        m_positions             = new List<Vector3>[PlayerCount];
        m_velocities            = new List<Vector3>[PlayerCount];
        m_rotations             = new List<float>[PlayerCount];
        m_angularVelocities     = new List<float>[PlayerCount];
        m_h                     = new List<float>[PlayerCount];
        m_v                     = new List<float>[PlayerCount];
        m_toggleFramesBoost     = new List<int>[PlayerCount];

        m_lastFrameInputs       = new MovementInputs[PlayerCount];
        m_nextInputIndices      = new int[PlayerCount][];

        for (int i = 0; i < PlayerCount; i++)
        {
            m_positions[i]          = new List<Vector3>();
            m_velocities[i]         = new List<Vector3>();
            m_rotations[i]          = new List<float>();
            m_angularVelocities[i]  = new List<float>();
            m_h[i]                  = new List<float>();
            m_v[i]                  = new List<float>();
            m_toggleFramesBoost[i]  = new List<int>();

            m_nextInputIndices[i]   = new int[1];
        }

        ResetRecorder();
    }

    public void ResetRecorder()
    {
        Array.ForEach(m_nextInputIndices, array => Array.Clear(array, 0, array.Length));
        Array.Clear(m_lastFrameInputs, 0, m_lastFrameInputs.Length);
    }

    public void Record(int fixedFramesSoFar, List<Player> players)
    {
        for (int i = 0; i < PlayerCount; i++)
        {
            Player player = players[i];

            if (!player.RaceResult.Finished)
            {
                if (fixedFramesSoFar % FRAMES_PER_POSITION == 0)
                {
                    m_positions[i].Add(player.transform.position);
                    m_velocities[i].Add(player.Movement.Velocity);
                    m_rotations[i].Add(player.transform.rotation.eulerAngles.y);
                    m_angularVelocities[i].Add(player.Movement.AngularVelocity);
                }

                if (fixedFramesSoFar % FRAMES_PER_INPUT == 0)
                {
                    m_h[i].Add(player.Inputs.h);
                    m_v[i].Add(player.Inputs.v);
                    if (m_lastFrameInputs[i].boost != player.Inputs.boost)
                    {
                        m_lastFrameInputs[i].boost ^= true;
                        m_toggleFramesBoost[i].Add(fixedFramesSoFar);
                    }
                }
            }
        }
    }

    public void MoveGhosts(int fixedFrameToDisplay, List<Player> players, List<CameraManager> cameras, bool isAfterStart)
    {
        int fixIndex    = fixedFrameToDisplay / FRAMES_PER_POSITION;
        int inputIndex  = fixedFrameToDisplay / FRAMES_PER_INPUT;

        for (int i = 0; i < PlayerCount; i++)
        {
            Player player = players[i];
            MovementInputs inputs;

            if (fixIndex < m_positions[i].Count)
            {
                if (fixedFrameToDisplay % FRAMES_PER_POSITION == 0)
                {
                    Vector3 pos = m_positions[i][fixIndex];
                    float rot = m_rotations[i][fixIndex];
                    Vector3 vel = m_velocities[i][fixIndex];
                    float angVel = m_angularVelocities[i][fixIndex];

                    if (cameras.Count > i)
                    {
                        CameraManager camera = cameras[i];
                        camera.transform.position += pos - player.transform.position;
                        camera.Interpolator.ForgetPreviousValues();
                    }

                    player.transform.position = pos;
                    player.transform.rotation = Quaternion.Euler(0, rot, 0);
                    player.Movement.Velocity = vel;
                    player.Movement.AngularVelocity = angVel;
                    player.Interpolator.ForgetPreviousValues();
                }
            }
            
            if (inputIndex < m_h[i].Count)
            {
                m_lastFrameInputs[i].h = m_h[i][inputIndex];
                m_lastFrameInputs[i].v = m_v[i][inputIndex];
                if ((m_nextInputIndices[i][0] < m_toggleFramesBoost[i].Count) && (m_toggleFramesBoost[i][m_nextInputIndices[i][0]] == fixedFrameToDisplay))
                {
                    m_nextInputIndices[i][0]++;
                    m_lastFrameInputs[i].boost ^= true;
                }

                inputs = m_lastFrameInputs[i];
            }
            else
            {
                inputs = new MovementInputs();
            }

            player.ProcessReplaying(true, isAfterStart, inputs);
        }
    }
}