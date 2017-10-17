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
}