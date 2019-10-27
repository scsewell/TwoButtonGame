namespace BoostBlasters.Races.Racers
{
    /// <summary>
    /// A system which provides input for racer movement when requested.
    /// </summary>
    public interface IInputProvider
    {
        void ResetProvider();
        void FixedUpdateProvider();
        void LateUpdateProvider();
        Inputs GetInput();
    }
}
