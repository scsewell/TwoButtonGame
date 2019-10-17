namespace BoostBlasters.Character
{
    /// <summary>
    /// A system which provides input for character movement when requested.
    /// </summary>
    public interface IInputProvider
    {
        void ResetProvider();
        void LateUpdateProvider();
        void FixedUpdateProvider();
        MovementInputs GetInput();
    }
}
