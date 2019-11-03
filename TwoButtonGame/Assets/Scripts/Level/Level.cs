using System;
using System.Collections.Generic;

using UnityEngine;

using Framework;
using Framework.AssetBundles;

namespace BoostBlasters.Levels
{
    /// <summary>
    /// Defines a level and its required properties.
    /// </summary>
    [CreateAssetMenu(fileName = "config", menuName = "Level", order = 2)]
    public class Level : ScriptableObject
    {
        [SerializeField]
        private int m_id = 1000;
        public int Id => m_id;

        [SerializeField]
        private int m_sortOrder = 100;
        public int SortOrder => m_sortOrder;

        [Header("Description")]

        [SerializeField]
        private string m_name = string.Empty;
        public string Name => m_name;

        [SerializeField]
        private Sprite m_preview = null;
        public Sprite Preview => m_preview;

        [SerializeField]
        private GameObject m_preview3d = null;
        public GameObject Preview3d => m_preview3d;

        [Header("Scene")]

        [SerializeField]
        private string m_sceneName = string.Empty;
        public string SceneName => m_sceneName;

        [SerializeField]
        private AssetBundleMusicReference m_music = null;
        public AssetBundleMusicReference Music => m_music;

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

        [Serializable]
        public class CameraShotArray : ReorderableArray<CameraShot>
        {
        }

        [SerializeField]
        [Reorderable]
        private CameraShotArray m_introSequence = null;

        /// <summary>
        /// The animations used for the intro camera.
        /// </summary>
        public IEnumerable<CameraShot> IntroSequence => m_introSequence;

        public override string ToString() => Name;
    }
}
