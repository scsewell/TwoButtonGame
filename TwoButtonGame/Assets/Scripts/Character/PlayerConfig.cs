using UnityEngine;

[CreateAssetMenu(fileName = "PlayerConfig", menuName = "PlayerConfig", order = 1)]
public class PlayerConfig : ScriptableObject
{
    [Header("Description")]
    [SerializeField]
    private int m_sortOrder = 100;
    public int SortOrder { get { return m_sortOrder; } }

    [SerializeField]
    private string m_name;
    public string Name { get { return m_name; } }
    
    [Header("Graphics")]
    [SerializeField]
    private GameObject m_characterGraphics;
    public GameObject CharacterGraphics { get { return m_characterGraphics; } }

    [SerializeField]
    private Vector3 m_graphicsOffset;
    public Vector3 GraphicsOffset { get { return m_graphicsOffset; } }

    [Header("Physics")]
    [SerializeField]
    private PhysicMaterial m_physicsMat;
    public PhysicMaterial PhysicsMat { get { return m_physicsMat; } }

    [SerializeField] [Range(0, 2)]
    private float m_linearDrag = 0.35f;
    public float LinearDrag { get { return m_linearDrag; } }

    [SerializeField] [Range(0, 20)]
    private float m_angularDrag = 4.0f;
    public float AngularDrag { get { return m_angularDrag; } }
    
    [SerializeField] [Range(0, 10)]
    private float m_gravityFac = 2.0f;
    public float GravityFac { get { return m_gravityFac; } }

    [SerializeField] [Range(0, 100)]
    private float m_forwardAccel = 15.0f;
    public float ForwardAccel { get { return m_forwardAccel; } }

    [SerializeField] [Range(0, 100)]
    private float m_verticalAccel = 7.5f;
    public float VerticalAccel { get { return m_verticalAccel; } }

    [SerializeField] [Range(0, 1)]
    private float m_turnRatio = 0.05f;
    public float TurnRatio { get { return m_turnRatio; } }

    [Header("Energy")]
    [SerializeField] [Range(0, 500)]
    private float m_energyCap = 200.0f;
    public float EnergyCap { get { return m_energyCap; } }

    [SerializeField] [Range(0, 100)]
    private float m_energyRechargeRate = 10.0f;
    public float EnergyRechargeRate { get { return m_energyRechargeRate; } }

    [Header("Boosting")]
    [SerializeField] [Range(0, 500)]
    private float m_boostEnergyUseRate = 100.0f;
    public float BoostEnergyUseRate { get { return m_boostEnergyUseRate; } }

    [SerializeField] [Range(1, 200)]
    private float m_boostSoftCap = 80.0f;
    public float BoostSoftCap { get { return m_boostSoftCap; } }

    [SerializeField] [Range(10, 500)]
    private float m_boostAcceleration = 200.0f;
    public float BoostAcceleration { get { return m_boostAcceleration; } }
}
