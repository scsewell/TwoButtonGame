using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RaceRecording
{
    private static readonly int FRAMES_PER_POSITION = 1;

    private List<Vector3>[] m_positions;
    private List<Quaternion>[] m_rotations;
    private List<MovementInputs>[] m_inputs;
    private List<int>[] m_toggleFramesLeft;
    private List<int>[] m_toggleFramesRight;
    private List<int>[] m_toggleFramesBoost;

    private MovementInputs[] m_lastFrameInputs;
    private int[][] m_nextInputIndices;

    public RaceRecording(List<Player> players)
    {
        m_positions = new List<Vector3>[players.Count];
        m_rotations = new List<Quaternion>[players.Count];
        m_inputs = new List<MovementInputs>[players.Count];
        m_toggleFramesLeft = new List<int>[players.Count];
        m_toggleFramesRight = new List<int>[players.Count];
        m_toggleFramesBoost = new List<int>[players.Count];

        m_lastFrameInputs = new MovementInputs[players.Count];
        m_nextInputIndices = new int[players.Count][];
        for (int playerIndex = 0; playerIndex < players.Count; playerIndex++)
        {
            m_positions[playerIndex] = new List<Vector3>();
            m_rotations[playerIndex] = new List<Quaternion>();
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
                m_rotations[playerIndex].Add(player.transform.rotation);
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
        Debug.Log(m_nextInputIndices);
        Debug.Log(m_lastFrameInputs);
        System.Array.Clear(m_nextInputIndices, 0, m_nextInputIndices.Length);
        System.Array.Clear(m_lastFrameInputs, 0, m_lastFrameInputs.Length);
        Debug.Log(m_nextInputIndices);
        Debug.Log(m_lastFrameInputs);
    }

    public void MoveGhosts(List<Player> players, int fixedFrameToDisplay)
    {
        for (int playerIndex = 0; playerIndex < players.Count; playerIndex++)
        {
            Player player = players[playerIndex];
            if (fixedFrameToDisplay / FRAMES_PER_POSITION < m_positions[playerIndex].Count)
            {
                //if (m_toggleFramesLeft[playerIndex][m_nextInputIndices[playerIndex, 0]] == fixedFrameToDisplay)
                //{
                //    m_nextInputIndices[playerIndex, 0]++;
                //    m_lastFrameInputs[playerIndex].left ^= true;
                //}
                //if (m_toggleFramesRight[playerIndex][m_nextInputIndices[playerIndex, 1]] == fixedFrameToDisplay)
                //{
                //    m_nextInputIndices[playerIndex, 1]++;
                //    m_lastFrameInputs[playerIndex].right ^= true;
                //}
                //if (m_toggleFramesBoost[playerIndex][m_nextInputIndices[playerIndex, 2]] == fixedFrameToDisplay)
                //{
                //    m_nextInputIndices[playerIndex, 2]++;
                //    m_lastFrameInputs[playerIndex].boost ^= true;
                //}
                player.ProcessReplaying(true, true, m_lastFrameInputs[playerIndex]);

                //if (fixedFrameToDisplay == 0)
                //{
                //    players[playerIndex].transform.position = m_positions[playerIndex][fixedFrameToDisplay / FRAMES_PER_POSITION];
                //    players[playerIndex].transform.rotation = m_rotations[playerIndex][fixedFrameToDisplay / FRAMES_PER_POSITION];
                //    players[playerIndex].GetComponent<Rigidbody>().velocity = Vector3.zero;
                //    players[playerIndex].Movement.DELETTHIS();
                //}

                ////players[playerIndex].transform.position = m_positions[playerIndex][fixedFrameToDisplay / FRAMES_PER_POSITION];
                //player.ProcessReplaying(true, true, m_inputs[playerIndex][fixedFrameToDisplay / FRAMES_PER_POSITION]);
                ////players[playerIndex].ProcessReplaying(????, ????
            }
            else
            {
                player.ProcessReplaying(true, true, new MovementInputs());
            }
        }
    }
}