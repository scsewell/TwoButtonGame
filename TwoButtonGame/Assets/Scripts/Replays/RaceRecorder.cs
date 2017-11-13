using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.IO.Compression;

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
    private List<int>[] m_toggleFramesLeft;
    private List<int>[] m_toggleFramesRight;
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
        m_toggleFramesLeft  = new List<int>[m_playerCount];
        m_toggleFramesRight = new List<int>[m_playerCount];
        m_toggleFramesBoost = new List<int>[m_playerCount];

        m_lastFrameInputs   = new MovementInputs[m_playerCount];
        m_nextInputIndices  = new int[m_playerCount][];

        for (int playerIndex = 0; playerIndex < m_playerCount; playerIndex++)
        {
            m_positions[playerIndex]            = new List<Vector3>();
            m_velocities[playerIndex]           = new List<Vector3>();
            m_rotations[playerIndex]            = new List<float>();
            m_angularVelocities[playerIndex]    = new List<float>();
            m_toggleFramesLeft[playerIndex]     = new List<int>();
            m_toggleFramesRight[playerIndex]    = new List<int>();
            m_toggleFramesBoost[playerIndex]    = new List<int>();

            m_nextInputIndices[playerIndex] = new int[3];
        }

        ResetRecorder();
    }

    public RaceRecording(byte[] bytes) : this(ReadValue<int>(bytes, 0, sizeof(int)))
    {
        int offset = sizeof(int); // already read the player count

        for (int i = 0; i < m_playerCount; i++)
        {
            m_positions[i]          = ReadVector3Array(bytes, ref offset).ToList();
            m_velocities[i]         = ReadVector3Array(bytes, ref offset).ToList();
            m_rotations[i]          = ReadArray<float>(bytes, ref offset, sizeof(float)).ToList();
            m_angularVelocities[i]  = ReadArray<float>(bytes, ref offset, sizeof(float)).ToList();
            m_toggleFramesLeft[i]   = ReadArray<int>(bytes, ref offset, sizeof(int)).ToList();
            m_toggleFramesRight[i]  = ReadArray<int>(bytes, ref offset, sizeof(int)).ToList();
            m_toggleFramesBoost[i]  = ReadArray<int>(bytes, ref offset, sizeof(int)).ToList();
        }
    }

    private byte[] ToBytes()
    {
        int totalOutputSize = 0;
        List<Write> writes = new List<Write>();
        
        totalOutputSize += PlanWriteValue(writes, m_playerCount, sizeof(int));
        
        for (int i = 0; i < m_playerCount; i++)
        {
            totalOutputSize += PlanWriteVector3Array(writes, m_positions[i].ToArray());
            totalOutputSize += PlanWriteVector3Array(writes, m_velocities[i].ToArray());
            totalOutputSize += PlanWriteArray(writes, m_rotations[i].ToArray(),         sizeof(float));
            totalOutputSize += PlanWriteArray(writes, m_angularVelocities[i].ToArray(), sizeof(float));
            totalOutputSize += PlanWriteArray(writes, m_toggleFramesLeft[i].ToArray(),  sizeof(int));
            totalOutputSize += PlanWriteArray(writes, m_toggleFramesRight[i].ToArray(), sizeof(int));
            totalOutputSize += PlanWriteArray(writes, m_toggleFramesBoost[i].ToArray(), sizeof(int));
        }

        byte[] output = new byte[totalOutputSize];
        int offset = 0;
        foreach (Write write in writes)
        {
            offset = write(output, offset);
        }
        
        return output;
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
                if (m_lastFrameInputs[playerIndex].left != player.Inputs.left)
                {
                    m_lastFrameInputs[playerIndex].left ^= true;
                    m_toggleFramesLeft[playerIndex].Add(fixedFramesSoFar);
                }
                if (m_lastFrameInputs[playerIndex].right != player.Inputs.right)
                {
                    m_lastFrameInputs[playerIndex].right ^= true;
                    m_toggleFramesRight[playerIndex].Add(fixedFramesSoFar);
                }
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

            if (fixIndex < m_positions[playerIndex].Count)
            {
                if ((m_nextInputIndices[playerIndex][0] < m_toggleFramesLeft[playerIndex].Count) && (m_toggleFramesLeft[playerIndex][m_nextInputIndices[playerIndex][0]] == fixedFrameToDisplay))
                {
                    m_nextInputIndices[playerIndex][0]++;
                    m_lastFrameInputs[playerIndex].left ^= true;
                }
                if ((m_nextInputIndices[playerIndex][1] < m_toggleFramesRight[playerIndex].Count) && (m_toggleFramesRight[playerIndex][m_nextInputIndices[playerIndex][1]] == fixedFrameToDisplay))
                {
                    m_nextInputIndices[playerIndex][1]++;
                    m_lastFrameInputs[playerIndex].right ^= true;
                }
                if ((m_nextInputIndices[playerIndex][2] < m_toggleFramesBoost[playerIndex].Count) && (m_toggleFramesBoost[playerIndex][m_nextInputIndices[playerIndex][2]] == fixedFrameToDisplay))
                {
                    m_nextInputIndices[playerIndex][2]++;
                    m_lastFrameInputs[playerIndex].boost ^= true;
                }

                player.ProcessReplaying(true, isAfterStart, m_lastFrameInputs[playerIndex]);

                if (fixedFrameToDisplay % FRAMES_PER_POSITION == 0)
                {
                    Vector3 pos     = m_positions[playerIndex][fixIndex];
                    float rot       = m_rotations[playerIndex][fixIndex];
                    Vector3 vel     = m_velocities[playerIndex][fixIndex];
                    float angVel    = m_angularVelocities[playerIndex][fixIndex];
                    
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
            }
            else
            {
                player.ProcessReplaying(true, isAfterStart, new MovementInputs());
            }
        }
    }

    public static byte[] CompressToBytes(RaceRecording recording)
    {
        using (MemoryStream output = new MemoryStream())
        {
            using (DeflateStream dstream = new DeflateStream(output, CompressionLevel.Optimal, true))
            {
                byte[] data = recording.ToBytes();
                dstream.Write(data, 0, data.Length);
            }
            return output.ToArray();
        }
    }

    public static RaceRecording DecompressFromBytes(byte[] recording)
    {
        using (MemoryStream input = new MemoryStream(recording))
        {
            using (MemoryStream output = new MemoryStream())
            {
                using (DeflateStream dstream = new DeflateStream(input, CompressionMode.Decompress))
                {
                    dstream.CopyTo(output);
                }
                return new RaceRecording(output.ToArray());
            }
        }
    }

    private delegate int Write(byte[] buf, int offset);
    
    private static int PlanWriteValue<T>(List<Write> plannedWrites, T val, int sizeOfT)
    {
        PlanWriteArray(plannedWrites, new T[] { val }, sizeOfT, false);
        return sizeOfT;
    }

    private static int PlanWriteArray<T>(List<Write> plannedWrites, T[] vals, int sizeOfT, bool includeHeader = true)
    {
        int headerSize = 0;
        if (includeHeader)
        {
            headerSize = PlanWriteValue(plannedWrites, vals.Length, sizeof(int));
        }

        plannedWrites.Add(new Write((buff, offset) =>
        {
            int valsSize = vals.Length * sizeOfT;
            Buffer.BlockCopy(vals, 0, buff, offset, valsSize);
            return offset + valsSize;
        }));
        return (vals.Length * sizeOfT) + headerSize;
    }

    private static int PlanWriteVector3Array(List<Write> plannedWrites, Vector3[] vals, bool includeHeader = true)
    {
        float[] floatArray = new float[vals.Length * 3];
        for (int i = 0; i < vals.Length; i++)
        {
            Vector3 v = vals[i];
            floatArray[(i * 3) + 0] = v.x;
            floatArray[(i * 3) + 1] = v.y;
            floatArray[(i * 3) + 2] = v.z;
        }
        return PlanWriteArray(plannedWrites, floatArray, sizeof(float), true);
    }
    
    private static T ReadValue<T>(byte[] buff, int offset, int sizeOfT)
    {
        return ReadArray<T>(buff, ref offset, sizeOfT, false)[0];
    }

    private static T ReadValue<T>(byte[] buff, ref int offset, int sizeOfT)
    {
        return ReadArray<T>(buff, ref offset, sizeOfT, false)[0];
    }

    private static T[] ReadArray<T>(byte[] buff, ref int offset, int sizeOfT, bool readHeader = true)
    {
        T[] vals = new T[readHeader ? ReadValue<int>(buff, ref offset, sizeof(int)) : 1];
        int valsSize = vals.Length * sizeOfT;
        Buffer.BlockCopy(buff, offset, vals, 0, valsSize);
        offset += valsSize;
        return vals;
    }

    private static Vector3[] ReadVector3Array(byte[] buff, ref int offset)
    {
        float[] floatArray = ReadArray<float>(buff, ref offset, sizeof(float));
        Vector3[] vals = new Vector3[floatArray.Length / 3];

        for (int i = 0; i < vals.Length; i++)
        {
            vals[i] = new Vector3(
                floatArray[(i * 3) + 0],
                floatArray[(i * 3) + 1],
                floatArray[(i * 3) + 2]
            );
        }
        return vals;
    }
}