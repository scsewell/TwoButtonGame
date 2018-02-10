using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

public class CameraRig : MonoBehaviour
{
    [SerializeField]
    private Transform m_fovBone;

    private Animator m_anim;
    private Camera m_cam;
    private LevelConfig m_levelConfig;

    private PlayableGraph m_graph;
    private PlayableOutput m_animOutput;
    private AnimationClipPlayable m_currentClip;
    private Queue<LevelConfig.CameraShot> m_shotsToPlay;

    public bool IsPlaying
    {
        get { return m_shotsToPlay.Count > 0 || !m_graph.IsDone(); }
    }

    private void Awake()
    {
        m_anim = GetComponentInChildren<Animator>();
        m_cam = m_anim.GetComponentInChildren<Camera>();

        SettingManager.Instance.ConfigureCamera(m_cam, false);

        m_shotsToPlay = new Queue<LevelConfig.CameraShot>();
    }

    public CameraRig Init(LevelConfig config)
    {
        m_levelConfig = config;
        return this;
    }

    private void OnDestroy()
    {
        m_graph.Destroy();
    }

    public float GetIntroSequenceLength()
    {
        return m_levelConfig.IntroSequence.Sum(s => s.Clip.length / s.Speed);
    }

    public void PlayIntroSequence()
    {
        if (m_graph.IsValid())
        {
            m_graph.Destroy();
        }
        
        m_graph = PlayableGraph.Create();
        m_graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

        m_animOutput = AnimationPlayableOutput.Create(m_graph, "IntroAnimation", m_anim);

        foreach (LevelConfig.CameraShot shot in m_levelConfig.IntroSequence)
        {
            m_shotsToPlay.Enqueue(shot);
        }
    }

    public void StopIntroSequence()
    {
        m_shotsToPlay.Clear();
    }

    public void UpdateCamera(bool enableCamera)
    {
        if (m_shotsToPlay.Count > 0)
        {
            if (m_shotsToPlay.Count == 0 || m_graph.IsDone())
            {
                m_graph.Stop();
                LevelConfig.CameraShot shot = m_shotsToPlay.Dequeue();
                
                m_currentClip = AnimationClipPlayable.Create(m_graph, shot.Clip);
                m_currentClip.SetDuration(shot.Clip.length);
                m_currentClip.SetTime(0);
                m_currentClip.SetSpeed(shot.Speed);

                m_animOutput.SetSourcePlayable(m_currentClip);
                m_graph.Play();
            }
        }

        m_cam.enabled = IsPlaying && enableCamera;
    }

    private void LateUpdate()
    {
        m_cam.fieldOfView = m_fovBone.localScale.y;
    }
}
