using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public interface IInputProvider
{
    void ResetProvider();
    void LateUpdateProvider();
    void FixedUpdateProvider();
    MovementInputs GetInput();
}

