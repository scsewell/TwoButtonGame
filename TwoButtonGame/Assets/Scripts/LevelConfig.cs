using UnityEngine;

[CreateAssetMenu(fileName = "LevelConfig", menuName = "LevelConfig", order = 2)]
public class LevelConfig : ScriptableObject
{
    [SerializeField]
    private string m_name;
    public string Name { get { return m_name; } }

    [SerializeField]
    private Sprite m_preview;
    public Sprite Preview { get { return m_preview; } }
    
    [SerializeField]
    private string m_sceneName;
    public string SceneName { get { return m_sceneName; } }

    [SerializeField] [Range(0, 5)]
    private float m_startSpacing = 2.0f;
    public float StartSpacing { get { return m_startSpacing; } }

    [SerializeField] [Range(0, 10)]
    private int m_countdownDuration = 5;
    public int CountdownDuration { get { return m_countdownDuration; } }

    [SerializeField]
    private AudioClip m_countdownSound;
    public AudioClip CountdownSound { get { return m_countdownSound; } }
    
    [SerializeField]
    private AudioClip m_goSound;
    public AudioClip GoSound { get { return m_goSound; } }
}