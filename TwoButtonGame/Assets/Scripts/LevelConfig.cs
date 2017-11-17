using UnityEngine;

[CreateAssetMenu(fileName = "LevelConfig", menuName = "LevelConfig", order = 2)]
public class LevelConfig : ScriptableObject
{
    [SerializeField]
    private int m_id = 1000;
    public int Id { get { return m_id; } }

    [SerializeField]
    private int m_sortOrder = 100;
    public int SortOrder { get { return m_sortOrder; } }

    [SerializeField]
    private string m_name;
    public string Name { get { return m_name; } }

    public enum Difficulty
    {
        Easy,
        Moderate,
        Hard,
        Intense,
    }

    [SerializeField]
    private Difficulty m_difficulty = Difficulty.Moderate;
    public Difficulty LevelDifficulty { get { return m_difficulty; } }

    [SerializeField]
    private Sprite m_preview;
    public Sprite Preview { get { return m_preview; } }
    
    [SerializeField]
    private string m_sceneName;
    public string SceneName { get { return m_sceneName; } }

    [SerializeField]
    private MusicParams m_music;
    public MusicParams Music { get { return m_music; } }

    [SerializeField] [Range(0, 5)]
    private float m_musicDelay = 0.5f;
    public float MusicDelay { get { return m_musicDelay; } }

    [SerializeField]
    private CameraShot[] m_introSequence;
    public CameraShot[] IntroSequence { get { return m_introSequence; } }

    [System.Serializable]
    public class CameraShot
    {
        [SerializeField]
        private AnimationClip m_clip;
        public AnimationClip Clip { get { return m_clip; } }

        [SerializeField] [Range(0, 10)]
        private float m_speed = 1;
        public float Speed { get { return m_speed; } }
    }
}