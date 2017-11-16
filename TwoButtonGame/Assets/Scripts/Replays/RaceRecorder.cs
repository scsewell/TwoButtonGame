using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Framework.IO;

[Serializable]
public class RaceRecording
{
    private int m_playerCount;
    
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

    public RaceRecording(int playerCount)
    {
        m_playerCount = playerCount;

        m_positions         = new List<Vector3>[m_playerCount];
        m_velocities        = new List<Vector3>[m_playerCount];
        m_rotations         = new List<float>[m_playerCount];
        m_angularVelocities = new List<float>[m_playerCount];
        m_h                 = new List<float>[m_playerCount];
        m_v                 = new List<float>[m_playerCount];
        m_toggleFramesBoost = new List<int>[m_playerCount];

        m_lastFrameInputs   = new MovementInputs[m_playerCount];
        m_nextInputIndices  = new int[m_playerCount][];

        for (int playerIndex = 0; playerIndex < m_playerCount; playerIndex++)
        {
            m_positions[playerIndex]            = new List<Vector3>();
            m_velocities[playerIndex]           = new List<Vector3>();
            m_rotations[playerIndex]            = new List<float>();
            m_angularVelocities[playerIndex]    = new List<float>();
            m_h[playerIndex]                    = new List<float>();
            m_v[playerIndex]                    = new List<float>();
            m_toggleFramesBoost[playerIndex]    = new List<int>();

            m_nextInputIndices[playerIndex] = new int[3];
        }

        ResetRecorder();
    }
     
    public RaceRecording(byte[] bytes) : this(BinaryReader.ReadValue<int>(bytes, 0))
    {
        BinaryReader reader = new BinaryReader(bytes);
        reader.SetReadPointer(sizeof(int));

        for (int i = 0; i < m_playerCount; i++)
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

    public byte[] ToBytes()
    {
        BinaryWriter writer = new BinaryWriter();
        writer.WriteValue(m_playerCount);
        
        for (int i = 0; i < m_playerCount; i++)
        {
            writer.WriteArray(m_positions[i].ToArray());
            writer.WriteArray(m_velocities[i].ToArray());
            writer.WriteArray(m_rotations[i].ToArray());
            writer.WriteArray(m_angularVelocities[i].ToArray());
            writer.WriteArray(m_h[i].ToArray());
            writer.WriteArray(m_v[i].ToArray());
            writer.WriteArray(m_toggleFramesBoost[i].ToArray());
        }
        
        return writer.GetBytes();
    }

    public void Record(List<Player> players, int fixedFramesSoFar)
    {
        for (int playerIndex = 0; playerIndex < players.Count; playerIndex++)
        {
            Player player = players[playerIndex];
            if (fixedFramesSoFar % FRAMES_PER_POSITION == 0)
            {
                m_positions[playerIndex].Add(player.transform.position);
                m_velocities[playerIndex].Add(player.Movement.Velocity);
                m_rotations[playerIndex].Add(player.transform.rotation.eulerAngles.y);
                m_angularVelocities[playerIndex].Add(player.Movement.AngularVelocity);
            }
            if (fixedFramesSoFar % FRAMES_PER_INPUT == 0)
            {
                m_h[playerIndex].Add(player.Inputs.h);
                m_v[playerIndex].Add(player.Inputs.v);
                if (m_lastFrameInputs[playerIndex].boost != player.Inputs.boost)
                {
                    m_lastFrameInputs[playerIndex].boost ^= true;
                    m_toggleFramesBoost[playerIndex].Add(fixedFramesSoFar);
                }
            }
        }
    }

    public void ResetRecorder()
    {
        Array.ForEach(m_nextInputIndices, array => Array.Clear(array, 0, array.Length));
        Array.Clear(m_lastFrameInputs, 0, m_lastFrameInputs.Length);
    }

    public void MoveGhosts(List<Player> players, List<CameraManager> cameras, int fixedFrameToDisplay, bool isAfterStart)
    {
        int fixIndex    = fixedFrameToDisplay / FRAMES_PER_POSITION;
        int inputIndex  = fixedFrameToDisplay / FRAMES_PER_INPUT;

        for (int playerIndex = 0; playerIndex < players.Count; playerIndex++)
        {
            Player player = players[playerIndex];
            MovementInputs inputs;

            if (fixIndex < m_positions[playerIndex].Count)
            {
                if (fixedFrameToDisplay % FRAMES_PER_POSITION == 0)
                {
                    Vector3 pos = m_positions[playerIndex][fixIndex];
                    float rot = m_rotations[playerIndex][fixIndex];
                    Vector3 vel = m_velocities[playerIndex][fixIndex];
                    float angVel = m_angularVelocities[playerIndex][fixIndex];

                    if (cameras.Count > playerIndex)
                    {
                        CameraManager camera = cameras[playerIndex];
                        camera.transform.position += pos - player.transform.position;
                    }

                    player.transform.position = pos;
                    player.transform.rotation = Quaternion.Euler(0, rot, 0);
                    player.Movement.Velocity = vel;
                    player.Movement.AngularVelocity = angVel;
                }

                m_lastFrameInputs[playerIndex].h = m_h[playerIndex][inputIndex];
                m_lastFrameInputs[playerIndex].v = m_v[playerIndex][inputIndex];
                if ((m_nextInputIndices[playerIndex][2] < m_toggleFramesBoost[playerIndex].Count) && (m_toggleFramesBoost[playerIndex][m_nextInputIndices[playerIndex][2]] == fixedFrameToDisplay))
                {
                    m_nextInputIndices[playerIndex][2]++;
                    m_lastFrameInputs[playerIndex].boost ^= true;
                }

                inputs = m_lastFrameInputs[playerIndex];
            }
            else
            {
                inputs = new MovementInputs();
            }

            player.ProcessReplaying(true, isAfterStart, inputs);
        }
    }
}