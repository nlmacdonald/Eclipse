using System;
using System.Collections.Generic;
using System.Text;

namespace Eclipse_Library
{
    public interface IPurchase
    {
        string Description { get; }
        void Apply(Character character);
    }
}
