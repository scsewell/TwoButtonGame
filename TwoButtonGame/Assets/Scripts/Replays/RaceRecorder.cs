using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Runtime.Serialization;

[Serializable]
public class RaceRecording
{
    //this constant has big tradeoff on bytes/sec to jerkiness
    private static readonly int FRAMES_PER_POSITION = 50;
    private List<Vector3>[] m_positions;
    private List<Vector3>[] m_velocities;
    private List<Quaternion>[] m_rotations;
    private List<float>[] m_angularVelocities;

    private static readonly int FRAMES_PER_INPUT = 1;
    //m_inputs probably removable to save bytes/sec
    private List<MovementInputs>[] m_inputs;
    private List<int>[] m_toggleFramesLeft;
    private List<int>[] m_toggleFramesRight;
    private List<int>[] m_toggleFramesBoost;

    private MovementInputs[] m_lastFrameInputs;
    private int[][] m_nextInputIndices;

    public RaceRecording(List<Player> players)
    {
        m_positions = new List<Vector3>[players.Count];
        m_velocities = new List<Vector3>[players.Count];
        m_rotations = new List<Quaternion>[players.Count];
        m_angularVelocities = new List<float>[players.Count];
        m_inputs = new List<MovementInputs>[players.Count];
        m_toggleFramesLeft = new List<int>[players.Count];
        m_toggleFramesRight = new List<int>[players.Count];
        m_toggleFramesBoost = new List<int>[players.Count];

        m_lastFrameInputs = new MovementInputs[players.Count];
        m_nextInputIndices = new int[players.Count][];
        for (int playerIndex = 0; playerIndex < players.Count; playerIndex++)
        {
            m_positions[playerIndex] = new List<Vector3>();
            m_velocities[playerIndex] = new List<Vector3>();
            m_rotations[playerIndex] = new List<Quaternion>();
            m_angularVelocities[playerIndex] = new List<float>();
            m_inputs[playerIndex] = new List<MovementInputs>();
            m_toggleFramesLeft[playerIndex] = new List<int>();
            m_toggleFramesRight[playerIndex] = new List<int>();
            m_toggleFramesBoost[playerIndex] = new List<int>();

            m_nextInputIndices[playerIndex] = new int[3];
        }
        ResetRecorder();
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
                m_rotations[playerIndex].Add(player.transform.rotation);
                m_angularVelocities[playerIndex].Add(player.Movement.AngularVelocity);
            }
            if (fixedFramesSoFar % FRAMES_PER_INPUT == 0)
            {
                m_inputs[playerIndex].Add(player.Inputs);
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
        Array.ForEach(m_nextInputIndices, array => System.Array.Clear(array, 0, array.Length));
        System.Array.Clear(m_lastFrameInputs, 0, m_lastFrameInputs.Length);
    }

    public void MoveGhosts(List<Player> players, int fixedFrameToDisplay, bool isAfterStart)
    {
        for (int playerIndex = 0; playerIndex < players.Count; playerIndex++)
        {
            Player player = players[playerIndex];
            if (fixedFrameToDisplay / FRAMES_PER_POSITION < m_positions[playerIndex].Count)
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
                //player.ProcessReplaying(true, isAfterStart, m_inputs[playerIndex][fixedFrameToDisplay / FRAMES_PER_POSITION]);

                //Proof that m_lastFrameInputs equals m_inputs
                Debug.Log(m_lastFrameInputs[playerIndex].Equals(m_inputs[playerIndex][fixedFrameToDisplay / FRAMES_PER_INPUT])
                    + ":" + m_lastFrameInputs[playerIndex].Equals(m_inputs[playerIndex][Math.Min((fixedFrameToDisplay / FRAMES_PER_INPUT) + 1, m_inputs[playerIndex].Count - 1)]));

                //if (fixedFrameToDisplay == 0)
                //{
                //    players[playerIndex].transform.position = m_positions[playerIndex][fixedFrameToDisplay / FRAMES_PER_POSITION];
                //    players[playerIndex].transform.rotation = m_rotations[playerIndex][fixedFrameToDisplay / FRAMES_PER_POSITION];
                //}

                if (fixedFrameToDisplay % FRAMES_PER_POSITION == 0)
                {
                    players[playerIndex].transform.position = m_positions[playerIndex][fixedFrameToDisplay / FRAMES_PER_POSITION];
                    players[playerIndex].Movement.Velocity = m_velocities[playerIndex][fixedFrameToDisplay / FRAMES_PER_POSITION];
                    players[playerIndex].transform.rotation = m_rotations[playerIndex][fixedFrameToDisplay / FRAMES_PER_POSITION];
                    players[playerIndex].Movement.AngularVelocity = m_angularVelocities[playerIndex][fixedFrameToDisplay / FRAMES_PER_POSITION];
                    //do we need to fix cameras or forget interpolators here?
                    //could also leave behind a visual follower that lerps toward the teleporting real ghost
                }
            }
            else
            {
                player.ProcessReplaying(true, isAfterStart, new MovementInputs());
            }
        }
    }

    public static byte[] ToByteArray(RaceRecording recording)
    {
        using (var stream = new MemoryStream())
        {
            MakeFormatter().Serialize(stream, recording);
            return stream.ToArray();
        }
    }

    public static RaceRecording FromByteArray(byte[] bytes)
    {
        using (var memStream = new MemoryStream(bytes))
        {
            return (RaceRecording)(MakeFormatter().Deserialize(memStream));
        }
    }

    public static BinaryFormatter MakeFormatter()
    {
        SurrogateSelector ss = new SurrogateSelector();

        ss.AddSurrogate(typeof(Vector3),
                        new StreamingContext(StreamingContextStates.All),
                        new Vector3SerializationSurrogate());

        ss.AddSurrogate(typeof(Quaternion),
                        new StreamingContext(StreamingContextStates.All),
                        new QuaternionSerializationSurrogate());

        return new BinaryFormatter
        {
            SurrogateSelector = ss
        };
    }
}

sealed class Vector3SerializationSurrogate : ISerializationSurrogate
{

    // Method called to serialize a Vector3 object
    public void GetObjectData(System.Object obj,
                              SerializationInfo info, StreamingContext context)
    {

        Vector3 v3 = (Vector3)obj;
        info.AddValue("x", v3.x);
        info.AddValue("y", v3.y);
        info.AddValue("z", v3.z);
        //Debug.Log(v3);
    }

    // Method called to deserialize a Vector3 object
    public System.Object SetObjectData(System.Object obj,
                                       SerializationInfo info, StreamingContext context,
                                       ISurrogateSelector selector)
    {

        Vector3 v3 = (Vector3)obj;
        v3.x = (float)info.GetSingle("x");
        v3.y = (float)info.GetSingle("y");
        v3.z = (float)info.GetSingle("z");
        obj = v3;
        return obj;   // Formatters ignore this return value //Seems to have been fixed!
    }
}

sealed class QuaternionSerializationSurrogate : ISerializationSurrogate
{
    public void GetObjectData(System.Object obj,
                              SerializationInfo info, StreamingContext context)
    {

        Quaternion quaternion = (Quaternion)obj;
        info.AddValue("x", quaternion.x);
        info.AddValue("y", quaternion.y);
        info.AddValue("z", quaternion.z);
        info.AddValue("w", quaternion.w);
    }

    public System.Object SetObjectData(System.Object obj,
                                       SerializationInfo info, StreamingContext context,
                                       ISurrogateSelector selector)
    {

        Quaternion quaternion = (Quaternion)obj;
        quaternion.x = (float)info.GetSingle("x");
        quaternion.y = (float)info.GetSingle("y");
        quaternion.z = (float)info.GetSingle("z");
        quaternion.w = (float)info.GetSingle("w");
        obj = quaternion;
        return obj;   // Formatters ignore this return value //Seems to have been fixed!
    }
}