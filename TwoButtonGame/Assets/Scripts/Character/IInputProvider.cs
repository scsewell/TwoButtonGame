using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public interface IInputProvider
{
    void ResetProvider();
    void UpdateProvider();
    void FixedUpdateProvider();
    MovementInputs GetInput();
}

