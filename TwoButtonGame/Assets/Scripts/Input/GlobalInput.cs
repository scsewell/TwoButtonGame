using System.Linq;

using UnityEngine.InputSystem;

namespace BoostBlasters.Input
{
    /// <summary>
    /// The input which is the combined inputs of all players.
    /// </summary>
    /// <remarks>
    /// This is useful for cases where all users should be able to
    /// control the same thing (ex. Main Menu UI).
    /// </remarks>
    public class GlobalInput : BaseInput
    {
    }
}
