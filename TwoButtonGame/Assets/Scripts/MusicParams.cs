using UnityEngine;

[CreateAssetMenu(fileName = "MusicParams", menuName = "MusicParams", order = 3)]
public class MusicParams : ScriptableObject
{
    [SerializeField]
    private AudioClip m_track;
    public AudioClip Track { get { return m_track; } }
    
    [SerializeField]
    private string m_name;
    public string Name { get { return m_name; } }

    [SerializeField]
    private int m_minutes;
    [SerializeField]
    private double m_seconds;

    public double LoopDuration
    {
        get { return (m_minutes * 60) + m_seconds; }
    }
}