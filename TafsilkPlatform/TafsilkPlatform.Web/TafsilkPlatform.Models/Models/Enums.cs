namespace TafsilkPlatform.Models.Models
{
    public class Enums
    {
        /// <summary>
        /// Payment methods supported by Tafsilk platform
        /// </summary>
        public enum PaymentType
        {
            /// <summary>Credit/Debit Card</summary>
            Card = 0,

            /// <summary>Generic Mobile Wallet</summary>
            Wallet = 1,

            /// <summary>Bank Transfer</summary>
            BankTransfer = 2,

            /// <summary>Cash on Delivery/Pickup</summary>
            Cash = 3,



            /// <summary>Other payment methods</summary>
            Other = 99
        }

        public enum PaymentStatus
        {
            Pending = 0,
            Completed = 1,
            Failed = 2,
            Refunded = 3,
            Cancelled = 4,

            /// <summary>Partial payment made (e.g., deposit)</summary>
            PartiallyPaid = 5
        }

        /// <summary>
        /// Transaction direction
        /// </summary>
        public enum TransactionType
        {
            /// <summary>Money coming in (customer pays)</summary>
            Credit = 0,

            /// <summary>Money going out (refund to customer)</summary>
            Debit = 1,

            /// <summary>Deposit payment (partial upfront)</summary>
            Deposit = 2,

            /// <summary>Final/remaining payment</summary>
            FinalPayment = 3
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

