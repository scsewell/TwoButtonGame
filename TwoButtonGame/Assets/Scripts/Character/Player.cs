using System.Linq;

using UnityEngine;

using Framework.Interpolation;

using BoostBlasters.Players;
using BoostBlasters.Races;
using BoostBlasters.Levels;

namespace BoostBlasters.Character
{
    /// <summary>
    /// The main component used to manage a player character.
    /// </summary>
    [RequireComponent(typeof(MemeBoots))]
    [RequireComponent(typeof(TransformInterpolator))]
    public class Player : MonoBehaviour
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
        private bool m_isHuman;
        public bool IsHuman => m_isHuman;

        private int m_playerNum = -1;
        public int PlayerNum => m_playerNum;

        private PlayerProfile m_profile;
        public PlayerProfile Profile => m_profile;

        private PlayerConfig m_config;
        public PlayerConfig Config => m_config;

        public Color GetColor() => Consts.GetRacerColor(m_playerNum);

        // Level Progress
        private int m_waypointsCompleted;
        public int WaypointsCompleted => m_waypointsCompleted;

        public Waypoint NextWaypoint => m_racePath.GetWaypoint(m_waypointsCompleted);

        public Waypoint SecondNextWaypoint => m_racePath.GetWaypoint(m_waypointsCompleted + 1);

        public int CurrentLap => m_racePath.GetCurrentLap(m_waypointsCompleted);

        private RaceResult m_raceResult;
        public RaceResult RaceResult => m_raceResult;

        // Energy
        public delegate void EnergyGainedHandler(float total, float delta);
        public event EnergyGainedHandler EnergyGained;

        public delegate void EnergyUseFailHandler();
        public event EnergyUseFailHandler EnergyUseFailed;

        public float MaxEnergy => m_config.EnergyCap;

        private float m_energy;
        public float Energy => m_energy;

        // General
        private TransformInterpolator m_interpolator;
        public TransformInterpolator Interpolator => m_interpolator;

        private MemeBoots m_movement;
        public MemeBoots Movement => m_movement;

        public MovementInputs Inputs => m_inputProvider.GetInput();

        private IInputProvider m_inputProvider;
        private PlayerAnimation m_animation;
        private RacePath m_racePath;
        private Vector3 m_lastPos;
        private Vector3 m_spawnPosition;
        private Quaternion m_spawnRotation;

        private void Awake()
        {
            m_interpolator = GetComponentInChildren<TransformInterpolator>();
            m_movement = GetComponentInChildren<MemeBoots>();
        }

        public Player InitHuman(int playerNum, PlayerProfile profile, PlayerConfig config, PlayerBaseInput input)
        {
            m_isHuman = true;
            m_inputProvider = new PlayerInputProvider(input);
            return Init(playerNum, profile, config);
        }

        public Player InitAI(int playerNum, PlayerProfile profile, PlayerConfig config)
        {
            m_isHuman = false;
            m_inputProvider = new PlayerAI(this);
            return Init(playerNum, profile, config);
        }

        private Player Init(int playerNum, PlayerProfile profile, PlayerConfig config)
        {
            m_playerNum = playerNum;
            m_profile = profile;
            m_config = config;

            m_raceResult = new RaceResult(profile);

            m_animation = GetComponentInChildren<PlayerAnimation>();
            m_racePath = Main.Instance.RaceManager.RacePath;

            m_lastPos = transform.position;
            m_spawnPosition = transform.position;
            m_spawnRotation = transform.rotation;
            return this;
        }

        public void ResetPlayer()
        {
            transform.position = m_spawnPosition;
            transform.rotation = m_spawnRotation;
            m_lastPos = m_spawnPosition;

            m_movement.ResetMovement();
            m_inputProvider.ResetProvider();

            if (m_animation)
            {
                m_animation.ResetAnimation();
            }

            m_interpolator.ForgetPreviousValues();

            m_energy = 0;
            m_waypointsCompleted = 0;

            m_raceResult.Reset();
        }

        public void ProcessPlaying(bool isAfterIntro, bool isAfterStart)
        {
            m_inputProvider.FixedUpdateProvider();
            ProcessReplaying(isAfterIntro, isAfterStart, m_inputProvider.GetInput());
        }

        public void ProcessReplaying(bool isAfterIntro, bool isAfterStart, MovementInputs inputs)
        {
            m_movement.FixedUpdateMovement(inputs, isAfterIntro && !m_raceResult.Finished, !isAfterStart);

            if (isAfterStart && !m_raceResult.Finished && !m_movement.IsBoosting)
            {
                m_energy = Mathf.Min(m_energy + (m_config.EnergyRechargeRate * Time.deltaTime), MaxEnergy);
            }

            int previousLap = CurrentLap;
            Waypoint wp = NextWaypoint;
            Vector3 disp = transform.position - m_lastPos;
            RaycastHit hit;
            if (wp != null && disp.magnitude > 0.0001f && wp.Trigger.Raycast(new Ray(m_lastPos, disp.normalized), out hit, disp.magnitude))
            {
                m_waypointsCompleted++;
                m_racePath.ResetEnergyGates(this);

                bool finished = m_racePath.IsFinished(m_waypointsCompleted);
                if (finished != m_raceResult.Finished)
                {
                    m_raceResult.Finished = true;
                    RecordLapTime();
                }

                bool completedLap = previousLap != CurrentLap;

                if (completedLap)
                {
                    RecordLapTime();
                }

                if (m_isHuman)
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

        public void UpdatePlayer()
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

        public void LateUpdatePlayer()
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
            if (boostGate != null && boostGate.UseGate(this))
            {
                OnEnergyGained(boostGate.Energy);
            }
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
                if (EnergyGained != null)
                {
                    EnergyGained(m_energy, delta);
                }
            }
        }

        public float ConsumeEnergy(float amountLost)
        {
            float delta = Mathf.Max(m_energy - amountLost, 0) - m_energy;
            m_energy += delta;
            return Mathf.Abs(delta);
        }

        private void RecordLapTime()
        {
            float currentTime = Main.Instance.RaceManager.GetStartRelativeTime(Time.time);
            m_raceResult.LapTimes.Add(currentTime - m_raceResult.LapTimes.Sum());
        }
    }
}
