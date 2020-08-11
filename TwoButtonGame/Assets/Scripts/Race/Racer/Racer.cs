using System;

using UnityEngine;

using Framework.Interpolation;

using BoostBlasters.Profiles;
using BoostBlasters.Characters;
using BoostBlasters.Levels.Elements;

namespace BoostBlasters.Races.Racers
{
    /// <summary>
    /// The main component used to manage a racer.
    /// </summary>
    [RequireComponent(typeof(RacerMovement))]
    [RequireComponent(typeof(TransformInterpolator))]
    public class Racer : MonoBehaviour
    {
        [Header("Sound")]

        [SerializeField]
        private AudioClip m_gateCompleteSound = null;
        [SerializeField]
        [Range(0f, 1f)]
        private float m_gateCompleteVolume = 1.0f;

        [SerializeField]
        private AudioClip m_lapCompleteSound = null;
        [SerializeField]
        [Range(0f, 1f)]
        private float m_lapCompleteVolume = 1.0f;

        [SerializeField]
        private AudioClip m_finishSound = null;
        [SerializeField]
        [Range(0f, 1f)]
        private float m_finishVolume = 1.0f;

        [SerializeField]
        private AudioClip m_energyFailSound = null;
        [SerializeField]
        [Range(0f, 1f)]
        private float m_energyFailVolume = 1.0f;

        // Configuration

        /// <summary>
        /// The index of this racer in the racer array.
        /// </summary>
        public int RacerNum { get; private set; }

        /// <summary>
        /// The color used to identify this racer.
        /// </summary>
        public Color Color { get; private set; }

        /// <summary>
        /// The configuration of the racer.
        /// </summary>
        public RacerConfig Config { get; private set; }

        // Level Progress
        private int m_waypointsCompleted;
        public int WaypointsCompleted => m_waypointsCompleted;

        public Waypoint NextWaypoint => m_racePath.GetWaypoint(m_waypointsCompleted);

        public Waypoint SecondNextWaypoint => m_racePath.GetWaypoint(m_waypointsCompleted + 1);

        public int CurrentLap => m_racePath.GetCurrentLap(m_waypointsCompleted);

        public RaceResult RaceResult { get; private set; }

        // Energy
        public delegate void EnergyGainedHandler(float total, float delta);
        public event EnergyGainedHandler EnergyGained;

        public delegate void EnergyUseFailHandler();
        public event EnergyUseFailHandler EnergyUseFailed;

        public float MaxEnergy => Config.Character.Energy.Cap;

        private float m_energy;
        public float Energy => m_energy;

        // General
        private TransformInterpolator m_interpolator;
        public TransformInterpolator Interpolator => m_interpolator;

        private RacerMovement m_movement;
        public RacerMovement Movement => m_movement;

        public Inputs Inputs => m_inputProvider.GetInput();

        private IInputProvider m_inputProvider;
        private RacerAnimation m_animation;
        private RacePath m_racePath;
        private Vector3 m_lastPos;
        private Vector3 m_spawnPosition;
        private Quaternion m_spawnRotation;

        private void Awake()
        {
            m_interpolator = GetComponentInChildren<TransformInterpolator>();
            m_movement = GetComponentInChildren<RacerMovement>();
        }

        /// <summary>
        /// Initializes this racer.
        /// </summary>
        /// <param name="racerNum">The index of this racer in the racer array.</param>
        /// <param name="config">The configuration of the racer.</param>
        /// <returns>The initialized racer.</returns>
        public Racer Init(int racerNum, RacerConfig config)
        {
            RacerNum = racerNum;
            Color = Consts.GetRacerColor(racerNum);
            Config = config;

            switch (config.Type)
            {
                case RacerType.Player:
                    m_inputProvider = new PlayerInputProvider(config.Input);
                    break;
                case RacerType.AI:
                    m_inputProvider = new AIInputProvider(this);
                    break;
                case RacerType.Replay:
                    break;
                default:
                    throw new ArgumentException("Unsupported racer config type", nameof(config));
            }

            //RaceResult = new RaceResult();

            m_animation = GetComponentInChildren<RacerAnimation>();
            m_racePath = Main.Instance.RaceManager.RacePath;

            m_lastPos = transform.position;
            m_spawnPosition = transform.position;
            m_spawnRotation = transform.rotation;

            return this;
        }

        /// <summary>
        /// Resets this 
        /// </summary>
        public void ResetRacer()
        {
            m_movement.ResetMovement();
            m_inputProvider.ResetProvider();

            if (m_animation != null)
            {
                m_animation.ResetAnimation();
            }

            //RaceResult.Reset();

            // reset the racer properties
            m_energy = 0f;
            m_waypointsCompleted = 0;

            // move back to the spawn
            transform.SetPositionAndRotation(m_spawnPosition, m_spawnRotation);
            m_lastPos = m_spawnPosition;

            m_interpolator.ForgetPreviousValues();
        }

        public void ProcessPlaying(bool isAfterIntro, bool isAfterStart)
        {
            m_inputProvider.FixedUpdateProvider();
            ProcessReplaying(isAfterIntro, isAfterStart, m_inputProvider.GetInput());
        }

        public void ProcessReplaying(bool isAfterIntro, bool isAfterStart, Inputs inputs)
        {
            m_movement.FixedUpdateMovement(inputs, isAfterIntro && !RaceResult.Finished, !isAfterStart);

            if (isAfterStart && !RaceResult.Finished && !m_movement.IsBoosting)
            {
                m_energy = Mathf.Min(m_energy + (Config.Character.Energy.RechargeRate * Time.deltaTime), MaxEnergy);
            }

            int previousLap = CurrentLap;
            Waypoint wp = NextWaypoint;
            Vector3 disp = transform.position - m_lastPos;
            if (wp != null && disp.magnitude > 0.0001f && wp.Trigger.Raycast(new Ray(m_lastPos, disp.normalized), out RaycastHit hit, disp.magnitude))
            {
                m_waypointsCompleted++;
                m_racePath.ResetEnergyGates(this);

                bool finished = m_racePath.IsFinished(m_waypointsCompleted);
                if (finished != RaceResult.Finished)
                {
                    //RaceResult.Finished = true;
                    RecordLapTime();
                }

                bool completedLap = previousLap != CurrentLap;

                if (completedLap)
                {
                    RecordLapTime();
                }

                // only play gate progression sounds if a local player
                if (Config.Type == RacerType.Player)
                {
                    if (finished)
                    {
                        AudioManager.Instance.PlaySound(m_finishSound, m_finishVolume);
                    }
                    else if (completedLap)
                    {
                        AudioManager.Instance.PlaySound(m_lapCompleteSound, m_lapCompleteVolume);
                    }
                    else
                    {
                        AudioManager.Instance.PlaySound(m_gateCompleteSound, m_gateCompleteVolume);
                    }
                }
            }
            m_lastPos = transform.position;
        }

        public void UpdateRacer()
        {
            if (m_animation != null)
            {
                m_animation.UpdateAnimation(m_movement);
            }

            if (NextWaypoint != null)
            {
                Debug.DrawLine(transform.position, NextWaypoint.Position, Color.magenta);
            }
        }

        public void LateUpdateRacer()
        {
            m_inputProvider.LateUpdateProvider();

            if (m_animation != null)
            {
                m_animation.LateUpdateAnimation(NextWaypoint);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            BoostGate boostGate = other.GetComponentInParent<BoostGate>();
            //if (boostGate != null && boostGate.UseGate(this))
            //{
            //    OnEnergyGained(boostGate.Energy);
            //}
        }

        public void OnEnergyUseFail()
        {
            if (EnergyUseFailed != null)
            {
                AudioManager.Instance.PlaySound(m_energyFailSound, m_energyFailVolume);
                EnergyUseFailed();
            }
        }

        private void OnEnergyGained(float amountGained)
        {
            float delta = Mathf.Min(m_energy + amountGained, MaxEnergy) - m_energy;

            if (delta > 0)
            {
                m_energy += delta;
                EnergyGained?.Invoke(m_energy, delta);
            }
        }

        public float ConsumeEnergy(float amountLost)
        {
            float delta = Mathf.Max(m_energy - amountLost, 0f) - m_energy;
            m_energy += delta;
            return Mathf.Abs(delta);
        }

        private void RecordLapTime()
        {
            //float currentTime = Main.Instance.RaceManager.GetStartRelativeTime(Time.time);
            //RaceResult.LapTimes.Add(currentTime - RaceResult.FinishTime);
        }
    }
}
