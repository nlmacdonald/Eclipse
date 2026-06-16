using System;
using System.Collections.Generic;
using System.Text;

namespace Eclipse_Library
{
    public sealed class PurchaseResult
    {
        public bool Success { get; set; }
        public List<string> Errors { get; } = new();
        public List<string> AppliedPurchaseDescriptions { get; } = new();
    }
}
