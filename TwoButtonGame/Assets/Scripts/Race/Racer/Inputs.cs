namespace BoostBlasters.Races.Racers
{
    /// <summary>
    /// The movements a racer can attempt to take.
    /// </summary>
    public struct Inputs
    {
        public float h;
        public float v;
        public bool boost;

        public Inputs(float h, float v, bool boost)
        {
            this.h = h;
            this.v = v;
            this.boost = boost;
        }
    }
}
