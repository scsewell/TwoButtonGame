using System;

using Framework;
using Framework.AssetBundles;
using Framework.Audio;

using UnityEngine;

namespace BoostBlasters.Levels
{
    /// <summary>
    /// Defines a level and its required properties.
    /// </summary>
    [CreateAssetMenu(fileName = "level", menuName = "BoostBlasters/Level", order = 0)]
    public class Level : ScriptableObject
    {
        [SerializeField]
        [Tooltip("The unique identifier for this level.")]
        private UnityGuid m_guid = default;

        /// <summary>
        /// The unique identifier for this level.
        /// </summary>
        public Guid Guid => m_guid;

        [SerializeField]
        [Tooltip("The sorting order of the level in the level selection menu. " +
            "Levels with lesser sorting orders appear first.")]
        private int m_sortOrder = 100;

        /// <summary>
        /// The sorting order of the level in the level selection menu. Levels with lesser
        /// sorting orders appear first.
        /// </summary>
        public int SortOrder => m_sortOrder;

        [Header("Description")]

        [SerializeField]
        [Tooltip("The display name of this level.")]
        private string m_name = string.Empty;

        /// <summary>
        /// The display name of this level.
        /// </summary>
        public string Name => m_name;

        [SerializeField]
        [Tooltip("The preview image for this level.")]
        private Sprite m_preview = null;

        /// <summary>
        /// The preview image for this level.
        /// </summary>
        public Sprite Preview => m_preview;

        [SerializeField]
        [Tooltip("The 3D track preview graphics.")]
        private GameObject m_preview3d = null;

        /// <summary>
        /// The 3D track preview graphics.
        /// </summary>
        public GameObject Preview3d => m_preview3d;

        [Header("Scene")]

        [SerializeField]
        [Tooltip("The scene containing the level.")]
        private AssetBundleSceneReference m_scene = null;

        /// <summary>
        /// The scene containing the level.
        /// </summary>
        public AssetBundleSceneReference Scene => m_scene;

        [SerializeField]
        [Tooltip("The music to play on this level.")]
        private AssetBundleMusicReference m_music = null;

        /// <summary>
        /// The music to play on this level.
        /// </summary>
        public AssetBundleMusicReference Music => m_music;

        [SerializeField]
        [Tooltip("The time in seconds after starting the level intro to wait before starting the music.")]
        [Range(0f, 5f)]
        private float m_musicDelay = 0.5f;

        /// <summary>
        /// The time in seconds after starting the level intro to wait before starting the music.
        /// </summary>
        public float MusicDelay => m_musicDelay;


        public override string ToString() => Name;
    }
}
