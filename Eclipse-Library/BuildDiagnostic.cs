using System;
using System.Collections.Generic;
using System.Text;

namespace Eclipse_Library
{
    public sealed class BuildDiagnostic
    {
        public BuildDiagnostic(BuildDiagnosticSeverity severity, string code, string message)
        {
            Severity = severity;
            Code = code;
            Message = message;
        }

        public BuildDiagnostic(
            BuildDiagnosticSeverity severity,
            string code,
            string message,
            int? sourceLevel,
            string? sourcePurchaseDescription = null)
            : this(severity, code, message)
        {
            if (sourceLevel is < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(sourceLevel), sourceLevel, "Source level must be >= 1.");
            }

            SourceLevel = sourceLevel;
            SourcePurchaseDescription = sourcePurchaseDescription;
        }

        public BuildDiagnostic(
            BuildDiagnosticSeverity severity,
            string code,
            string message,
            BuildDiagnosticContext context)
            : this(
                severity,
                code,
                message,
                context?.Level,
                context?.PurchaseDescription)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public BuildDiagnosticSeverity Severity { get; }
        public string Code { get; }
        public string Message { get; }
        public int? SourceLevel { get; }
        public string? SourcePurchaseDescription { get; }
        public BuildDiagnosticContext? Context { get; }

        public BuildDiagnostic WithContext(BuildDiagnosticContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return new BuildDiagnostic(Severity, Code, Message, context);
        }
    }
}
