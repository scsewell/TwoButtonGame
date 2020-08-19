using UnityEngine.InputSystem;

namespace BoostBlasters.Input
{
    /// <summary>
    /// The input of a single player.
    /// </summary>
    public class UserInput : BaseInput
    {
        /// <summary>
        /// The player input backing this user.
        /// </summary>
        public PlayerInput Player { get; private set; }


        protected override void OnEnable()
        {
            Player = GetComponent<PlayerInput>();
            transform.SetParent(PlayerInputManager.instance.transform, false);

            base.OnEnable();

            Player.actions = Actions.asset;
        }

        /// <summary>
        /// Unjoins this user.
        /// </summary>
        public void Leave()
        {
            Destroy(gameObject);
        }
    }
}
