using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using Framework;
using Framework.Audio;
using Framework.StateMachines;

using BoostBlasters.Profiles;
using BoostBlasters.Characters;
using BoostBlasters.Races.Racers;
using BoostBlasters.Replays;
using BoostBlasters.UI.RaceMenus;

namespace BoostBlasters.Races
{
    public class ReplayState : RaceState
    {
        [SerializeField]
        [Tooltip("The duration in seconds of the fade in when starting the replay.")]
        [Range(0.01f, 5f)]
        private float m_fadeInDuration = 1.0f;

        [SerializeField]
        private AssetBundleMusicReference m_replayMusic = null;


        private ReplayCamera m_replayCamera;

        public override RaceState GetNextState()
        {
            return this;
        }
    }
}
