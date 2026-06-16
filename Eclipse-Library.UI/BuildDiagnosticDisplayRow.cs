using System;
using Eclipse_Library;

namespace Eclipse_Library.UI
{
    public sealed class BuildDiagnosticDisplayRow
    {
        public BuildDiagnosticDisplayRow(BuildDiagnostic diagnostic)
        {
            if (diagnostic is null)
            {
                throw new ArgumentNullException(nameof(diagnostic));
            }

            Severity = diagnostic.Severity.ToString();
            Level = diagnostic.Context?.Level ?? diagnostic.SourceLevel;
            Source = string.IsNullOrWhiteSpace(diagnostic.Context?.PurchaseDescription)
                ? diagnostic.SourcePurchaseDescription ?? ""
                : diagnostic.Context.PurchaseDescription;
            Code = diagnostic.Code;
            Message = diagnostic.Message;
        }

        public string Severity { get; }
        public int? Level { get; }
        public string Source { get; }
        public string Code { get; }
        public string Message { get; }
    }
}
