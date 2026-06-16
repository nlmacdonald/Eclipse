using System;

namespace Eclipse_Library
{
    public sealed class AppliedPurchaseRecord
    {
        public AppliedPurchaseRecord(
            Guid purchaseId,
            int sourceLevel,
            int purchaseIndex,
            string purchaseDescription,
            bool success,
            IPurchase purchase,
            string? failureMessage = null)
        {
            if (sourceLevel < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(sourceLevel), sourceLevel, "Source level must be >= 1.");
            }

            if (purchaseIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(purchaseIndex), purchaseIndex, "Purchase index must be >= 0.");
            }

            PurchaseId = purchaseId;
            SourceLevel = sourceLevel;
            PurchaseIndex = purchaseIndex;
            PurchaseDescription = purchaseDescription ?? throw new ArgumentNullException(nameof(purchaseDescription));
            Success = success;
            Purchase = purchase ?? throw new ArgumentNullException(nameof(purchase));
            FailureMessage = failureMessage;
        }

        public Guid PurchaseId { get; }
        public int SourceLevel { get; }
        public int PurchaseIndex { get; }
        public string PurchaseDescription { get; }
        public bool Success { get; }
        public string? FailureMessage { get; }
        public IPurchase Purchase { get; }
    }
}
