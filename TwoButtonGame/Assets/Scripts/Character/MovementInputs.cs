public struct MovementInputs
{
    public float h;
    public float v;
    public bool boost;

    public MovementInputs(float h, float v, bool boost)
    {    
        this.h = h;
        this.v = v;
        this.boost = boost;
    }
}
