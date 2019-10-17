using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using Framework.IO;

using BoostBlasters.Players;
using BoostBlasters.Character;
using BoostBlasters.Races;
using BoostBlasters.Levels;

namespace BoostBlasters.Replays
{
    public class RaceRecording
    {
        private static readonly int FRAMES_PER_POSITION = 10;
        private List<Vector3>[] m_positions;
        private List<Vector3>[] m_velocities;
        private List<float>[] m_rotations;
        private List<float>[] m_angularVelocities;

        private static readonly int FRAMES_PER_INPUT = 1;
        private List<sbyte>[] m_h;
        private List<sbyte>[] m_v;
        private List<int>[] m_toggleFramesBoost;

        private MovementInputs[] m_lastFrameInputs;
        private int[][] m_nextInputIndices;

        private RaceParameters m_raceParams;
        public RaceParameters RaceParams => m_raceParams;

        private int RacerCount => RaceParams.RacerCount;

        public float Duration => m_positions.Max(player => player.Count) * FRAMES_PER_POSITION * Time.fixedDeltaTime;

        public RaceRecording(RaceParameters raceParams)
        {
            m_raceParams = raceParams;
            CreateBuffers();
        }
     
        public RaceRecording(byte[] bytes)
        {
            BinaryReader reader = new BinaryReader(bytes);
        
            RaceResult[] results;
            ParseHeader(reader, out m_raceParams, out results);

            CreateBuffers();
        
            byte[] compressed = new byte[bytes.Length - reader.GetReadPointer()];
            Array.Copy(bytes, reader.GetReadPointer(), compressed, 0, compressed.Length);
            reader = new BinaryReader(Compression.Decompress(compressed));
        
            for (int i = 0; i < RacerCount; i++)
            {
                m_positions[i]          = reader.ReadArray<Vector3>().ToList();
                m_velocities[i]         = reader.ReadArray<Vector3>().ToList();
                m_rotations[i]          = reader.ReadArray<float>().ToList();
                m_angularVelocities[i]  = reader.ReadArray<float>().ToList();
                m_h[i]                  = reader.ReadArray<sbyte>().ToList();
                m_v[i]                  = reader.ReadArray<sbyte>().ToList();
                m_toggleFramesBoost[i]  = reader.ReadArray<int>().ToList();
            }
        }

        public static void ParseHeader(BinaryReader reader, out RaceParameters raceParams, out RaceResult[] results)
        {
            LevelConfig level   = Main.Instance.GetLevelConfig(reader.ReadInt());
            int laps            = reader.ReadInt();
            int humanCount      = reader.ReadInt();
            int aiCount         = reader.ReadInt();
            List<PlayerConfig> playerConfigs = reader.ReadArray<int>().Select(id => Main.Instance.GetPlayerConfig(id)).ToList();

            List<PlayerProfile> proflies = new List<PlayerProfile>();
            for (int i = 0; i < humanCount + aiCount; i++)
            {
                long id = reader.ReadLong();
                string name = reader.ReadString();
                proflies.Add(PlayerProfileManager.Instance.GetGuestProfile(name, false));
            }

            List<PlayerBaseInput> inputs = InputManager.Instance.PlayerInputs.ToList();
            List<int> playerindicies = new List<int>();

            raceParams = new RaceParameters(level, laps, humanCount, aiCount, playerConfigs, proflies, inputs, playerindicies);
       
            results = new RaceResult[raceParams.RacerCount];
            for (int i = 0; i < raceParams.RacerCount; i++)
            {
                results[i] = new RaceResult(reader.ReadArray<byte>(), raceParams.Profiles[i]);
            }
        }

        public byte[] ToBytes(List<Player> players)
        {
            BinaryWriter headerWriter = new BinaryWriter();
        
            headerWriter.WriteValue(m_raceParams.LevelConfig.Id);
            headerWriter.WriteValue(m_raceParams.Laps);
            headerWriter.WriteValue(m_raceParams.HumanCount);
            headerWriter.WriteValue(m_raceParams.AICount);
            headerWriter.WriteArray(m_raceParams.PlayerConfigs.Select(c => c.Id).ToArray());

            for (int i = 0; i < RacerCount; i++)
            {
                Player player = players[i];
                headerWriter.WriteValue(player.Profile.UniqueId);
                headerWriter.WriteValue(player.Profile.Name);
            }

            for (int i = 0; i < RacerCount; i++)
            {
                headerWriter.WriteArray(players[i].RaceResult.GetBytes());
            }

            BinaryWriter bodyWriter = new BinaryWriter();

            for (int i = 0; i < RacerCount; i++)
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
            m_positions             = new List<Vector3>[RacerCount];
            m_velocities            = new List<Vector3>[RacerCount];
            m_rotations             = new List<float>[RacerCount];
            m_angularVelocities     = new List<float>[RacerCount];
            m_h                     = new List<sbyte>[RacerCount];
            m_v                     = new List<sbyte>[RacerCount];
            m_toggleFramesBoost     = new List<int>[RacerCount];

            m_lastFrameInputs       = new MovementInputs[RacerCount];
            m_nextInputIndices      = new int[RacerCount][];

            for (int i = 0; i < RacerCount; i++)
            {
                m_positions[i]          = new List<Vector3>();
                m_velocities[i]         = new List<Vector3>();
                m_rotations[i]          = new List<float>();
                m_angularVelocities[i]  = new List<float>();
                m_h[i]                  = new List<sbyte>();
                m_v[i]                  = new List<sbyte>();
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
            for (int i = 0; i < RacerCount; i++)
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
                        m_h[i].Add(PackFloat(player.Inputs.h));
                        m_v[i].Add(PackFloat(player.Inputs.v));
                        if (m_lastFrameInputs[i].boost != player.Inputs.boost)
                        {
                            m_lastFrameInputs[i].boost ^= true;
                            m_toggleFramesBoost[i].Add(fixedFramesSoFar);
                        }
                    }
                }
            }
        }

        public void MoveGhosts(int fixedFrameToDisplay, List<Player> players, List<RacerCamera> cameras, bool isAfterStart)
        {
            int fixIndex    = fixedFrameToDisplay / FRAMES_PER_POSITION;
            int inputIndex  = fixedFrameToDisplay / FRAMES_PER_INPUT;

            for (int i = 0; i < RacerCount; i++)
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
                            RacerCamera camera = cameras[i];
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
                    m_lastFrameInputs[i].h = UnpackFloat(m_h[i][inputIndex]);
                    m_lastFrameInputs[i].v = UnpackFloat(m_v[i][inputIndex]);
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

        private sbyte PackFloat(float val)
        {
            return (sbyte)Mathf.Clamp(Mathf.RoundToInt(val * sbyte.MaxValue), sbyte.MinValue, sbyte.MaxValue);
        }

        public float UnpackFloat(sbyte val)
        {
            return (float)val / sbyte.MaxValue;
        }
    }
}