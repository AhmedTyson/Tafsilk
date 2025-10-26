namespace TafsilkPlatform.Core.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentResult> ProcessCashPaymentAsync(Guid orderId, decimal amount);
        Task<PaymentResult> ProcessDigitalPaymentAsync(Guid orderId, decimal amount, string provider);
        Task<PaymentResult> ProcessWalletPaymentAsync(Guid userId, Guid orderId, decimal amount);
        Task<bool> RefundPaymentAsync(Guid paymentId, decimal amount);
    }

    public class PaymentResult
    {
        public bool Success { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
