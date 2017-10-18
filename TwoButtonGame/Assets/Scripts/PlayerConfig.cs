using UnityEngine;

[CreateAssetMenu(fileName = "PlayerConfig", menuName = "PlayerConfig", order = 1)]
public class PlayerConfig : ScriptableObject
{
    [SerializeField]
    private string m_name;
    public string Name { get { return m_name; } }

    [SerializeField]
    private Sprite m_preview;
    public Sprite Preview { get { return m_preview; } }

    [SerializeField]
    private Player m_playerPrefab;
    public Player PlayerPrefab { get { return m_playerPrefab; } }

    [SerializeField]
    private PhysicMaterial m_physicsMat;
    public PhysicMaterial PhysicsMat { get { return m_physicsMat; } }

    [SerializeField] [Range(0, 2)]
    private float m_linearDrag = 0.35f;
    public float LinearDrag { get { return m_linearDrag; } }

    [SerializeField] [Range(0, 20)]
    private float m_angularDrag = 4.0f;
    public float AngularDrag { get { return m_angularDrag; } }

    [SerializeField]
    [Range(0, 100)]
    private float m_forwardAccel = 15.0f;
    public float ForwardAccel { get { return m_forwardAccel; } }

    [SerializeField]
    [Range(0, 100)]
    private float m_verticalAccel = 7.5f;
    public float VerticalAccel { get { return m_verticalAccel; } }

    [SerializeField]
    [Range(0, 1)]
    private float m_turnRatio = 0.05f;
    public float TurnRatio { get { return m_turnRatio; } }

    [SerializeField]
    [Range(0, 10)]
    private float m_gravityFac = 2.0f;
    public float GravityFac { get { return m_gravityFac; } }
}
