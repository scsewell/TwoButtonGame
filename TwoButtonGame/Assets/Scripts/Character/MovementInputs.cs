using System;
using System.Collections.Generic;
using System.Linq;

public struct MovementInputs
{
    public bool left;
    public bool right;
    public bool boost;
    public bool boostHold;

    public MovementInputs(bool left, bool right, bool boost, bool boostHold)
    {    
        this.left = left;
        this.right = right;
        this.boost = boost;
        this.boostHold = boostHold;
    }
}
