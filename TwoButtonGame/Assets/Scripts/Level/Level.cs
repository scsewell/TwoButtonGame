using System;

using UnityEngine;

using Framework.Audio;

namespace BoostBlasters.Levels
{
    /// <summary>
    /// Defines a level and its required properties.
    /// </summary>
    [CreateAssetMenu(fileName = "New Level", menuName = "Level", order = 2)]
    public class Level : ScriptableObject
    {
        [SerializeField]
        private int m_id = 1000;
        public int Id => m_id;

        [SerializeField]
        private int m_sortOrder = 100;
        public int SortOrder => m_sortOrder;

        [SerializeField]
        private string m_name = string.Empty;
        public string Name => m_name;

        public enum Difficulty
        {
            Easy,
            Moderate,
            Hard,
            Intense,
        }

        [SerializeField]
        private Difficulty m_difficulty = Difficulty.Moderate;
        public Difficulty LevelDifficulty => m_difficulty;

        [SerializeField]
        private Sprite m_preview = null;
        public Sprite Preview => m_preview;

        [SerializeField]
        private GameObject m_preview3d = null;
        public GameObject Preview3d => m_preview3d;

        [SerializeField]
        private string m_sceneName = string.Empty;
        public string SceneName => m_sceneName;

        [SerializeField]
        private MusicParams m_music = null;
        public MusicParams Music => m_music;

        [SerializeField]
        [Range(0f, 5f)]
        private float m_musicDelay = 0.5f;
        public float MusicDelay => m_musicDelay;

        [Serializable]
        public class CameraShot
        {
            [SerializeField]
            private AnimationClip m_clip = null;
            public AnimationClip Clip => m_clip;

            [SerializeField]
            [Range(0f, 10f)]
            private float m_speed = 1f;
            public float Speed => m_speed;
        }

        [SerializeField]
        private CameraShot[] m_introSequence = null;
        public CameraShot[] IntroSequence => m_introSequence;

        public override string ToString() => Name;
    }
}
