using System;

using Framework;

using UnityEngine;

namespace BoostBlasters.Characters
{
    /// <summary>
    /// The properties that constitute a character.
    /// </summary>
    [CreateAssetMenu(fileName = "character", menuName = "BoostBlasters/Character", order = 0)]
    public class Character : ScriptableObject
    {
        [SerializeField]
        [Tooltip("The unique identifier for this character.")]
        private UnityGuid m_guid = default;

        /// <summary>
        /// The unique identifier for this character.
        /// </summary>
        public Guid Guid => m_guid;

        [SerializeField]
        [Tooltip("The sorting order of the character in the character selection menu. " +
            "Characters with lesser sorting orders appear first.")]
        private int m_sortOrder = 100;

        /// <summary>
        /// The sorting order of the character in the character selection menu. Characters with
        /// lesser sorting orders appear first.
        /// </summary>
        public int SortOrder => m_sortOrder;

        /// <summary>
        /// A class containing information about a character.
        /// </summary>
        [Serializable]
        public class MetaConfig
        {
            [SerializeField]
            [Tooltip("The display name of this character.")]
            private string m_name = string.Empty;

            /// <summary>
            /// The display name of this character.
            /// </summary>
            public string Name => m_name;

            [SerializeField]
            [Tooltip("The character description shown in character selection.")]
            [TextArea]
            private string m_description = string.Empty;

            /// <summary>
            /// The character description shown in character selection.
            /// </summary>
            public string Description => m_description;

            [SerializeField]
            [Tooltip("The rating shown during character selection indicating how fast the character moves forward.")]
            [Range(1, 10)]
            private int m_speedRating = 5;

            /// <summary>
            /// The rating shown during character selection indicating how fast the character moves forward.
            /// </summary>
            public int SpeedRating => m_speedRating;

            [SerializeField]
            [Tooltip("The rating shown during character selection indicating how fast the character turns.")]
            [Range(1, 10)]
            private int m_agilityRating = 5;

            /// <summary>
            /// The rating shown during character selection indicating how fast the character turns.
            /// </summary>
            public int AgilityRating => m_agilityRating;
        }

        [SerializeField]
        [Tooltip("The character description.")]
        private MetaConfig m_meta;

        /// <summary>
        /// The character description.
        /// </summary>
        public MetaConfig Meta => m_meta;

        /// <summary>
        /// A class describing the appearance of a character.
        /// </summary>
        [Serializable]
        public class GraphicsConfig
        {
            [SerializeField]
            [Tooltip("The character rig prefab.")]
            private GameObject m_characterRig = null;

            /// <summary>
            /// The character rig prefab.
            /// </summary>
            public GameObject Rig => m_characterRig;

            [SerializeField]
            [Tooltip("The offset of the character rig from the center of the player collider.")]
            private Vector3 m_offset = Vector3.zero;

            /// <summary>
            /// The offset of the character rig from the center of the player collider.
            /// </summary>
            public Vector3 Offset => m_offset;
        }

        [SerializeField]
        [Tooltip("The character graphics.")]
        private GraphicsConfig m_graphics;

        /// <summary>
        /// The character graphics.
        /// </summary>
        public GraphicsConfig Graphics => m_graphics;

        /// <summary>
        /// A class describing the physics properties of a character.
        /// </summary>
        [Serializable]
        public class PhysicsConfig
        {
            [SerializeField]
            [Tooltip("The physics material used by the character collider.")]
            private PhysicMaterial m_material = null;

            /// <summary>
            /// The physics material used by the character collider.
            /// </summary>
            public PhysicMaterial Material => m_material;

            [SerializeField]
            [Tooltip("The default linear drag coefficient of the character.")]
            [Range(0f, 2f)]
            private float m_linearDrag = 0.35f;

            /// <summary>
            /// The default linear drag coefficient of the character.
            /// </summary>
            public float LinearDrag => m_linearDrag;

            [SerializeField]
            [Tooltip("The linear drag coefficient of the character to used when braking.")]
            [Range(0f, 2f)]
            private float m_brakeDrag = 1.0f;

            /// <summary>
            /// The linear drag coefficient of the character to used when braking.
            /// </summary>
            public float BrakeDrag => m_brakeDrag;

            [SerializeField]
            [Tooltip("The default angular drag coefficient of the character.")]
            [Range(0f, 20f)]
            private float m_angularDrag = 4.0f;

            /// <summary>
            /// The default angular drag coefficient of the character.
            /// </summary>
            public float AngularDrag => m_angularDrag;

            [SerializeField]
            [Tooltip("The multiple of gravity applied to the character.")]
            [Range(0f, 10f)]
            private float m_gravityMultiplier = 2.0f;

            /// <summary>
            /// The multiple of gravity applied to the character.
            /// </summary>
            public float GravityMultiplier => m_gravityMultiplier;

            [SerializeField]
            [Tooltip("The max forward acceleration applied by a single engine.")]
            [Range(0f, 100f)]
            private float m_forwardAcceleration = 15.0f;

            /// <summary>
            /// The max forward acceleration applied by a single engine.
            /// </summary>
            public float ForwardAcceleration => m_forwardAcceleration;

            [SerializeField]
            [Tooltip("The max upwards acceleration applied by a single engine.")]
            [Range(0f, 100f)]
            private float m_verticalAcceleration = 7.5f;

            /// <summary>
            /// The max vertical acceleration applied by a single engine.
            /// </summary>
            public float VerticalAcceleration => m_verticalAcceleration;

            [SerializeField]
            [Tooltip("The horizontal offset of the engine force. Larger values increase turn rate.")]
            [Range(0f, 0.25f)]
            private float m_forceOffset = 0.05f;

            /// <summary>
            /// The horizontal offset of the engine force. Larger values increase turn rate.
            /// </summary>
            public float ForceOffset => m_forceOffset;
        }

        [SerializeField]
        [Tooltip("The character graphics.")]
        private PhysicsConfig m_physics;

        /// <summary>
        /// The character physics.
        /// </summary>
        public PhysicsConfig Physics => m_physics;

        /// <summary>
        /// A class describing the energy pool of a character.
        /// </summary>
        [Serializable]
        public class EnergyConfig
        {
            [SerializeField]
            [Tooltip("The max energy the character can have at once.")]
            [Range(0f, 500f)]
            private float m_cap = 200.0f;

            /// <summary>
            /// The max energy the character can have at once.
            /// </summary>
            public float Cap => m_cap;

            [SerializeField]
            [Tooltip("The base energy gained per second.")]
            [Range(0f, 100f)]
            private float m_rechargeRate = 10.0f;

            /// <summary>
            /// The base energy gained per second.
            /// </summary>
            public float RechargeRate => m_rechargeRate;
        }

        [SerializeField]
        [Tooltip("The character energy pool.")]
        private EnergyConfig m_energy;

        /// <summary>
        /// The character energy pool.
        /// </summary>
        public EnergyConfig Energy => m_energy;

        /// <summary>
        /// A class describing the boost properties of a character.
        /// </summary>
        [Serializable]
        public class BoostConfig
        {
            [SerializeField]
            [Tooltip("The energy consumed per second by boosting.")]
            [Range(0f, 500f)]
            private float m_energyUseRate = 100.0f;

            /// <summary>
            /// The energy consumed per second by boosting.
            /// </summary>
            public float EnergyUseRate => m_energyUseRate;

            [SerializeField]
            [Tooltip("The acceleration applied by boosting.")]
            [Range(10f, 500f)]
            private float m_acceleration = 200.0f;

            /// <summary>
            /// The acceleration applied by boosting.
            /// </summary>
            public float Acceleration => m_acceleration;

            [SerializeField]
            [Tooltip("How much the effect of boosting is reduced when aiming along the character's current velocity when already at high speed.")]
            [Range(1f, 200f)]
            private float m_softCap = 80.0f;

            /// <summary>
            /// How much the effect of boosting is reduced when aiming along the character's current
            /// velocity when already at high speed.
            /// </summary>
            public float SoftCap => m_softCap;
        }

        [SerializeField]
        [Tooltip("The character boost properties.")]
        private BoostConfig m_boost;

        /// <summary>
        /// The character energy pool.
        /// </summary>
        public BoostConfig Boost => m_boost;


        public override string ToString() => m_meta.Name;
    }
}
