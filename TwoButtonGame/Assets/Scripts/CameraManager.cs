using UnityEngine;
using Framework.Interpolation;

[RequireComponent(typeof(TransformInterpolator))]
public class CameraManager : MonoBehaviour
{
    [SerializeField] [Range(0, 20)]
    private float m_smoothness = 10.0f;
    [SerializeField]
    private Vector2 m_positionOffset = new Vector2(12, 2);
    [SerializeField]
    private float m_lookOffset = 2.0f;

    private Camera m_cam;
    public Camera Camera { get { return m_cam; } }

    private Player m_player;
    public Player Owner { get { return m_player; } }

    private TransformInterpolator m_tInterpolator;

    private void Awake()
    {
        m_cam = GetComponent<Camera>();
        m_tInterpolator = GetComponent<TransformInterpolator>();
    }

    public CameraManager Init(Player player, int playerCount)
    {
        m_player = player;

        m_cam.rect = GetSplitscreen(player.PlayerNum, playerCount);

        SettingManager.Instance.ConfigureCamera(m_cam, true);
        
        transform.position = GetPosTarget();
        transform.rotation = GetRotTarget();
        m_tInterpolator.ForgetPreviousValues();

        return this;
    }

    public void MainUpdate()
    {
        if (m_player != null)
        {
            Vector3 playerVelocity = m_player.GetComponent<MemeBoots>().Velocity;

            transform.position = Vector3.SmoothDamp(transform.position, GetPosTarget(), ref playerVelocity, m_smoothness * Time.deltaTime);
            transform.rotation = GetRotTarget();
        }
    }
    
    private Vector3 GetPosTarget()
    {
        Vector3 forwardLook = m_player.transform.forward;
        return m_player.transform.position - m_positionOffset.x * forwardLook + m_positionOffset.y * Vector3.Cross(forwardLook, transform.right);
    }

    private Quaternion GetRotTarget()
    {
        Vector3 lookTarget = m_player.transform.position + m_lookOffset * Vector3.up;
        return Quaternion.LookRotation(lookTarget - transform.position, Vector3.up);
    }

    public static Rect GetSplitscreen(int playerNum, int playerCount)
    {
        const float pixelMargin = 5.0f;
        Vector2 m = new Vector2(pixelMargin / Screen.width, pixelMargin / Screen.height) / 2;

        Rect FULL       = new Rect(0,           0,              1,              1);
        Rect TOP        = new Rect(0,           0.5f + m.y,     1,              0.5f - m.y);
        Rect BOTTOM     = new Rect(0,           0,              1,              0.5f - m.y);
        Rect TOP_L      = new Rect(0,           0.5f + m.y,     0.5f - m.x,     0.5f - m.y);
        Rect TOP_R      = new Rect(0.5f + m.x,  0.5f + m.y,     0.5f - m.x,     0.5f - m.y);
        Rect BOTTOM_L   = new Rect(0,           0,              0.5f - m.x,     0.5f - m.y);
        Rect BOTTOM_R   = new Rect(0.5f + m.x,  0,              0.5f - m.x,     0.5f - m.y);

        switch (playerCount)
        {
            case 1: return FULL;
            case 2:
                switch (playerNum)
                {
                    case 0: return TOP;
                    case 1: return BOTTOM;
                }
                break;
            case 3:
                switch (playerNum)
                {
                    case 0: return TOP;
                    case 1: return BOTTOM_L;
                    case 2: return BOTTOM_R;
                }
                break;
            case 4:
                switch (playerNum)
                {
                    case 0: return TOP_L;
                    case 1: return TOP_R;
                    case 2: return BOTTOM_L;
                    case 3: return BOTTOM_R;
                }
                break;
        }
        return FULL;
    }
}
