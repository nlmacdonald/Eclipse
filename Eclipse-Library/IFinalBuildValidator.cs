using System.Collections.Generic;

namespace Eclipse_Library
{
    public interface IFinalBuildValidator
    {
        IEnumerable<BuildDiagnostic> Validate(FinalBuildValidationContext context);
    }
}

