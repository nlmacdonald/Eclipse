using System.Collections.Generic;

namespace Eclipse_Library
{
    public interface IBuildStepValidator
    {
        IEnumerable<BuildDiagnostic> Validate(BuildStepValidationContext context);
    }
}

