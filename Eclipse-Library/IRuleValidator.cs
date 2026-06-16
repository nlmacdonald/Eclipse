using System;
using System.Collections.Generic;
using System.Text;

namespace Eclipse_Library
{
    public interface IRuleValidator
    {
        IEnumerable<BuildDiagnostic> Validate(Character character);
    }
}
