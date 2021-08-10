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
    public class MainState : RaceState
    {


        public override RaceState GetNextState()
        {
            return this;
        }
    }
}
