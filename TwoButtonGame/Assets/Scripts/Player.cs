using UnityEngine;
using Framework.Interpolation;

[RequireComponent(typeof(MemeBoots))]
[RequireComponent(typeof(TransformInterpolator))]
public class Player : MonoBehaviour
{
    private MemeBoots m_movement;
    private PlayerAnimation m_animation;
    private RacePath m_racePath;

    private int m_playerNum = -1;
    public int PlayerNum { get { return m_playerNum; } }

    private int m_waypointsCompleted = 0;
    public int WaypointsCompleted { get { return m_waypointsCompleted; } }
    
    private bool m_isFinished = false;
    public bool IsFinished { get { return m_isFinished; } }

    private float m_finishTime = float.MaxValue;
    public float FinishTime { get { return m_finishTime; } }

    public Waypoint NextWaypoint
    {
        get { return m_racePath.GetWaypoint(m_waypointsCompleted); }
    }

    public Waypoint SecondNextWaypoint
    {
        get { return m_racePath.GetWaypoint(m_waypointsCompleted + 1); }
    }

    private void Awake()
    {
        m_movement = GetComponentInChildren<MemeBoots>();
    }

    public Player Init(int playerNum, PlayerInput input, PlayerConfig config)
    {
        m_playerNum = playerNum;
        
        m_racePath = Main.Instance.RaceManager.RacePath;

        m_movement.Init(input, config);

        return this;
    }

    public void FixedUpdatePlayer(bool canAcceptInput)
    {
        m_movement.Move(!m_isFinished && canAcceptInput);

        bool finished = m_racePath.IsFinished(m_waypointsCompleted);
        if (finished != m_isFinished)
        {
            m_finishTime = Time.time;
            m_isFinished = finished;
        }
    }

    public void UpdatePlayer()
    {
        m_movement.UpdateMovement();

        if (m_animation == null)
        {
            m_animation = GetComponentInChildren<PlayerAnimation>();
        }

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
        if (m_animation != null)
        {
            m_animation.LateUpdateAnimation(NextWaypoint);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Waypoint waypoint = other.GetComponent<Waypoint>();
        if (waypoint == NextWaypoint)
        {
            m_waypointsCompleted++;
        }
    }

    public Color GetColor()
    {
        return Consts.PLAYER_COLORS[m_playerNum];
    }
}
