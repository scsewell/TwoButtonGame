using System;

using UnityEngine;

using Framework;

namespace BoostBlasters.Characters
{
    /// <summary>
    /// The properties that constitute a character.
    /// </summary>
    [CreateAssetMenu(fileName = "character", menuName = "Character", order = 1)]
    public class Character : ScriptableObject
    {
        [SerializeField]
        private UnityGuid m_guid;
        public Guid Guid => m_guid;

        [SerializeField]
        private int m_sortOrder = 100;
        public int SortOrder => m_sortOrder;

        [Header("Description")]

        [SerializeField]
        private string m_name = string.Empty;
        public string Name => m_name;

        [SerializeField]
        [TextArea]
        private string m_description = string.Empty;
        public string Description => m_description;

        [SerializeField]
        [Range(1, 10)]
        private int m_speedRating = 5;
        public int SpeedRating => m_speedRating;

        [SerializeField]
        [Range(1, 10)]
        private int m_agilityRating = 5;
        public int AgilityRating => m_agilityRating;

        [Header("Graphics")]

        [SerializeField]
        private GameObject m_characterGraphics = null;
        public GameObject CharacterGraphics => m_characterGraphics;

        [SerializeField]
        private Vector3 m_graphicsOffset = Vector3.zero;
        public Vector3 GraphicsOffset => m_graphicsOffset;

        [Header("Physics")]

        [SerializeField]
        private PhysicMaterial m_physicsMat = null;
        public PhysicMaterial PhysicsMat => m_physicsMat;

        [SerializeField]
        [Range(0f, 2f)]
        private float m_linearDrag = 0.35f;
        public float LinearDrag => m_linearDrag;

        [SerializeField]
        [Range(0f, 2f)]
        private float m_brakeDrag = 1.0f;
        public float BrakeDrag => m_brakeDrag;

        [SerializeField]
        [Range(0f, 20f)]
        private float m_angularDrag = 4.0f;
        public float AngularDrag => m_angularDrag;

        [SerializeField]
        [Range(0f, 10f)]
        private float m_gravityFac = 2.0f;
        public float GravityFac => m_gravityFac;

        [SerializeField]
        [Range(0f, 100f)]
        private float m_forwardAccel = 15.0f;
        public float ForwardAccel => m_forwardAccel;

        [SerializeField]
        [Range(0f, 100f)]
        private float m_verticalAccel = 7.5f;
        public float VerticalAccel => m_verticalAccel;

        [SerializeField]
        [Range(0f, 1f)]
        private float m_turnRatio = 0.05f;
        public float TurnRatio => m_turnRatio;

        [Header("Energy")]

        [SerializeField]
        [Range(0f, 500f)]
        private float m_energyCap = 200.0f;
        public float EnergyCap => m_energyCap;

        [SerializeField]
        [Range(0f, 100f)]
        private float m_energyRechargeRate = 10.0f;
        public float EnergyRechargeRate => m_energyRechargeRate;

        [Header("Boosting")]

        [SerializeField]
        [Range(0f, 500f)]
        private float m_boostEnergyUseRate = 100.0f;
        public float BoostEnergyUseRate => m_boostEnergyUseRate;

        [SerializeField]
        [Range(1f, 200f)]
        private float m_boostSoftCap = 80.0f;
        public float BoostSoftCap => m_boostSoftCap;

        [SerializeField]
        [Range(10f, 500f)]
        private float m_boostAcceleration = 200.0f;
        public float BoostAcceleration => m_boostAcceleration;

        public override string ToString() => Name;
    }
}
