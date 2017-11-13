using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public struct MovementInputs
{
    public bool left;
    public bool right;
    public bool boost;

    public MovementInputs(bool left, bool right, bool boost)
    {    
        this.left = left;
        this.right = right;
        this.boost = boost;
    }

    public override bool Equals(object obj)
    {
        if (!(obj is MovementInputs))
        {
            return false;
        }

        var inputs = (MovementInputs)obj;
        return left == inputs.left &&
               right == inputs.right &&
               boost == inputs.boost;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}
