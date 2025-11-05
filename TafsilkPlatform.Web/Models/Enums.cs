namespace TafsilkPlatform.Web.Models
{
    public class Enums
    {
        public enum PaymentType
        {
            Card = 0,
            Wallet = 1,
            BankTransfer = 2,
            Cash = 3,
            Other = 99
        }

        public enum PaymentStatus
        {
            Pending = 0,
            Completed = 1,
            Failed = 2,
            Refunded = 3,
            Cancelled = 4
        }

        public enum TransactionType
        {
            Credit = 0,
            Debit = 1
        }

        public enum DisputeStatus
        {
            Open = 0,
            UnderReview = 1,
            Resolved = 2,
            Rejected = 3
        }

        public enum RefundStatus
        {
            Requested = 0,
            InProgress = 1,
            Completed = 2,
            Rejected = 3
        }
    }

}

