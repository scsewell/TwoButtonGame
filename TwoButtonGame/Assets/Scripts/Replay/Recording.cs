using System;
using System.Collections.Generic;
using System.Linq;

using BoostBlasters.Characters;
using BoostBlasters.Levels;
using BoostBlasters.Profiles;
using BoostBlasters.Races;
using BoostBlasters.Races.Racers;

using Framework;
using Framework.IO;

using UnityEngine;

namespace BoostBlasters.Replays
{
    /// <summary>
    /// Holds replay data generated during a race.
    /// </summary>
    public class Recording : SerializableData
    {
        protected override FourCC SerializerType { get; } = new FourCC("RECO");
        protected override ushort SerializerVersion => 1;

        private const int FRAMES_PER_KEYFRAME = 10;


        private struct Keyframe
        {
            private Vector3 m_position;
            private float m_rotation;
            private Vector3 m_velocity;
            private float m_angularVelocity;

            public Keyframe(Racer racer)
            {
                m_position = racer.transform.position;
                m_rotation = racer.transform.rotation.eulerAngles.y;
                m_velocity = racer.Movement.Velocity;
                m_angularVelocity = racer.Movement.AngularVelocity;
            }

            public void Apply(Racer racer)
            {
                racer.transform.SetPositionAndRotation(m_position, Quaternion.Euler(0f, m_rotation, 0f));
                racer.Movement.Velocity = m_velocity;
                racer.Movement.AngularVelocity = m_angularVelocity;
                racer.Interpolator.ForgetPreviousValues();
            }
        }

        private List<Keyframe>[] m_keyframes;
        private List<Inputs>[] m_inputs;
        private bool m_hasReplayContents;

        /// <summary>
        /// The configuration of the recorded race.
        /// </summary>
        public RaceParameters Params { get; private set; }

        /// <summary>
        /// The outcomes of the recorded race.
        /// </summary>
        public RaceResult[] Results { get; private set; }

        /// <summary>
        /// The number of recorded frames. 
        /// </summary>
        public float FrameCount
        {
            get
            {
                var frameCount = 0;
                if (m_inputs != null)
                {
                    for (var i = 0; i < m_inputs.Length; i++)
                    {
                        frameCount = Mathf.Max(frameCount, m_inputs[i].Count);
                    }
                }
                return frameCount;
            }
        }

        /// <summary>
        /// The game time duration of the recorded race. 
        /// </summary>
        public float Duration => FrameCount * Time.fixedDeltaTime;


        /// <summary>
        /// Creates a new recording.
        /// </summary>
        /// <param name="raceParams">The configuration of the race.</param>
        /// <param name="results">The outcomes of the race for all the racers.</param>
        public Recording(RaceParameters raceParams, RaceResult[] results)
        {
            m_hasReplayContents = true;

            Params = raceParams;
            Results = results;

            m_keyframes = new List<Keyframe>[raceParams.Racers.Length];
            m_inputs = new List<Inputs>[raceParams.Racers.Length];

            for (var i = 0; i < raceParams.Racers.Length; i++)
            {
                m_keyframes[i] = new List<Keyframe>();
                m_inputs[i] = new List<Inputs>();
            }
        }

        /// <summary>
        /// Deserializes a race recording.
        /// </summary>
        /// <param name="reader">A data reader at a serialized recording.</param>
        /// <param name="headerOnly">Will the body or the recording be read.</param>
        public Recording(DataReader reader, bool headerOnly = false)
        {
            m_hasReplayContents = !headerOnly;

            Deserialize(reader);
        }

        protected override void OnSerialize(DataWriter writer)
        {
            // write recording header
            writer.Write(Params.Level.Guid);
            writer.Write(Params.Laps);

            writer.Write(Params.Racers.Length);
            for (var i = 0; i < Params.Racers.Length; i++)
            {
                var racer = Params.Racers[i];

                writer.Write(racer.Character.Guid);
                writer.Write(racer.Profile.Guid);
                writer.Write(racer.Profile.Name);

                Results[i].Serialize(writer);
            }

            // write recording contents
            using (var bodyWriter = new DataWriter())
            {
                for (var i = 0; i < Params.Racers.Length; i++)
                {
                    var keyframes = m_keyframes[i].ToArray();
                    bodyWriter.Write(keyframes.Length);
                    bodyWriter.Write(keyframes);

                    var inputs = m_inputs[i].ToArray();
                    bodyWriter.Write(inputs.Length);
                    bodyWriter.Write(inputs);
                }

                var body = Compression.Compress(bodyWriter.GetBytes());
                writer.Write(body.Length);
                writer.Write(body);
            }
        }

        protected override void OnDeserialize(DataReader reader, ushort version)
        {
            // load the header data
            var level = LevelManager.GetByGUID(reader.Read<Guid>());
            var laps = reader.Read<int>();

            var racerCount = reader.Read<int>();

            var racers = new RacerConfig[racerCount];
            Results = new RaceResult[racerCount];

            for (var i = 0; i < racerCount; i++)
            {
                var character = CharacterManager.GetByGUID(reader.Read<Guid>());

                var guid = reader.Read<Guid>();
                var name = reader.ReadString();
                //var profile = ProfileManager.CreateTemporaryProfile(name, false);

                //racers[i] = new ReplayRacerConfig(character, profile);
                Results[i] = new RaceResult(reader);
            }

            Params = new RaceParameters(level, laps, racers);

            // only load the replay data if needed
            if (!m_hasReplayContents)
            {
                return;
            }

            var bodySize = reader.Read<int>();
            var body = Compression.Decompress(reader.Read<byte>(bodySize));

            using (var bodyReader = new DataReader(body))
            {
                m_keyframes = new List<Keyframe>[Params.Racers.Length];
                m_inputs = new List<Inputs>[Params.Racers.Length];

                for (var i = 0; i < Params.Racers.Length; i++)
                {
                    m_keyframes[i] = bodyReader.Read<Keyframe>(bodyReader.Read<int>()).ToList();
                    m_inputs[i] = bodyReader.Read<Inputs>(bodyReader.Read<int>()).ToList();
                }
            }
        }

        /// <summary>
        /// Records the current race state.
        /// </summary>
        /// <param name="fixedFrame">The game frame number to record.</param>
        /// <param name="racers">The racers to record the state of.</param>
        public void Record(int fixedFrame, List<Racer> racers)
        {
            for (var i = 0; i < Params.Racers.Length; i++)
            {
                var racer = racers[i];

                if (!racer.RaceResult.Finished)
                {
                    if (fixedFrame % FRAMES_PER_KEYFRAME == 0)
                    {
                        m_keyframes[i].Add(new Keyframe(racer));
                    }
                    m_inputs[i].Add(racer.Inputs);
                }
            }
        }

        /// <summary>
        /// Applies the frame of a recording.
        /// </summary>
        /// <param name="fixedFrame">The game frame number from the recording to apply.</param>
        /// <param name="racers">The racers to apply the recorded state to.</param>
        /// <param name="cameras">The racer cameras to apply the recorded state to.</param>
        /// <param name="isAfterStart">Has the race began.</param>
        public void ApplyRecordedFrame(int fixedFrame, List<Racer> racers, List<RacerCamera> cameras, bool isAfterStart)
        {
            var isKeyframe = fixedFrame % FRAMES_PER_KEYFRAME == 0;
            var keyframeIndex = fixedFrame / FRAMES_PER_KEYFRAME;

            for (var i = 0; i < Params.Racers.Length; i++)
            {
                var racer = racers[i];

                if (isKeyframe && keyframeIndex < m_keyframes[i].Count)
                {
                    var lastPos = racer.transform.position;

                    m_keyframes[i][keyframeIndex].Apply(racer);

                    if (cameras.Count > i)
                    {
                        var camera = cameras[i];
                        camera.transform.position += lastPos - racer.transform.position;
                        camera.Interpolator.ForgetPreviousValues();
                    }
                }

                var inputs = fixedFrame < m_inputs[i].Count ? m_inputs[i][fixedFrame] : new Inputs();

                racer.ProcessReplaying(true, isAfterStart, inputs);
            }
        }
    }
}
