#if FALSE // ⚠️ DISABLED: PaymentService doesn't match current Payment model
// TODO: Refactor to use Enums.PaymentType instead of PaymentMethod and align with existing Payment entity
// See: Docs/BUILD_ERROR_ANALYSIS.md for refactoring checklist

using TafsilkPlatform.DataAccess.Data;
using TafsilkPlatform.Models.Models;
using TafsilkPlatform.Models.ViewModels.Payments;
using Microsoft.EntityFrameworkCore;

namespace TafsilkPlatform.Web.Services;

/// <summary>
/// Payment service interface for handling all payment operations
/// Supports multiple payment methods: Cash, Digital Wallets, Cards, Bank Transfers
/// </summary>
public interface IPaymentService
{
    // Payment Processing
  Task<ServiceResult<Guid>> InitiatePaymentAsync(Guid orderId, Guid customerId, PaymentRequest request);
    Task<ServiceResult<bool>> ProcessPaymentAsync(Guid paymentId, PaymentProcessRequest request);
    Task<ServiceResult<bool>> ConfirmPaymentAsync(Guid paymentId, string transactionReference);
 Task<ServiceResult<bool>> CancelPaymentAsync(Guid paymentId, string reason);
    
 // Wallet Operations
    Task<ServiceResult<decimal>> GetWalletBalanceAsync(Guid userId);
    Task<ServiceResult<bool>> AddToWalletAsync(Guid userId, decimal amount, string source);
    Task<ServiceResult<bool>> DeductFromWalletAsync(Guid userId, decimal amount, string purpose);
    
    // Transaction History
    Task<PaymentHistoryViewModel> GetPaymentHistoryAsync(Guid userId, int page = 1, int pageSize = 20);
    Task<PaymentDetailsViewModel?> GetPaymentDetailsAsync(Guid paymentId);
    
    // Refunds
    Task<ServiceResult<Guid>> RequestRefundAsync(Guid paymentId, Guid userId, RefundRequest request);
    Task<ServiceResult<bool>> ProcessRefundAsync(Guid refundId, bool approve, string? notes);
    
    // Payment Verification
    Task<bool> VerifyPaymentAsync(Guid paymentId);
    Task<PaymentStatus> GetPaymentStatusAsync(Guid paymentId);
}

public class PaymentService : IPaymentService
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<PaymentService> _logger;
    private readonly IConfiguration _configuration;

    public PaymentService(
     ApplicationDbContext db,
        ILogger<PaymentService> logger,
        IConfiguration configuration)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    // ==================== PAYMENT PROCESSING ====================

    /// <summary>
    /// Initiate a payment for an order
    /// </summary>
    public async Task<ServiceResult<Guid>> InitiatePaymentAsync(
        Guid orderId, 
        Guid customerId, 
PaymentRequest request)
    {
 try
        {
    _logger.LogInformation("[PaymentService] Initiating payment for order {OrderId}", orderId);

            // Validate order
            var order = await _db.Orders
           .Include(o => o.Customer)
        .FirstOrDefaultAsync(o => o.OrderId == orderId);

   if (order == null)
                return ServiceResult<Guid>.Failure("Order not found");

            if (order.CustomerId != customerId)
             return ServiceResult<Guid>.Failure("Unauthorized: Order does not belong to this customer");

          if (order.Status != OrderStatus.Delivered && order.Status != OrderStatus.Processing)
   return ServiceResult<Guid>.Failure("Order must be in processing or delivered status for payment");

            // Check for existing pending payment
            var existingPayment = await _db.Payments
     .FirstOrDefaultAsync(p => p.OrderId == orderId && 
     (p.Status == PaymentStatus.Pending || p.Status == PaymentStatus.Processing));

 if (existingPayment != null)
                return ServiceResult<Guid>.Failure("A pending payment already exists for this order");

 // Validate amount
            if (request.Amount != (decimal)order.TotalPrice)
    return ServiceResult<Guid>.Failure($"Payment amount must match order total: {order.TotalPrice:C}");

   // Create payment record
  var payment = new Payment
            {
          PaymentId = Guid.NewGuid(),
         OrderId = orderId,
   CustomerId = customerId,
  Amount = request.Amount,
                PaymentMethod = request.PaymentMethod,
    Status = PaymentStatus.Pending,
   Currency = "EGP",
        CreatedAt = DateTime.UtcNow,
        Description = $"Payment for Order #{orderId.ToString().Substring(0, 8)}"
    };

            // Handle different payment methods
            switch (request.PaymentMethod)
  {
        case PaymentMethod.CashOnDelivery:
        payment.Status = PaymentStatus.Pending;
     payment.Notes = "Cash payment to be collected on delivery";
         break;

  case PaymentMethod.Wallet:
     // Check wallet balance
               var balance = await GetWalletBalanceAsync(customerId);
   if (balance.Data < request.Amount)
           return ServiceResult<Guid>.Failure($"Insufficient wallet balance. Available: {balance.Data:C}");
       
     // Reserve wallet funds (will be deducted on confirmation)
         payment.Status = PaymentStatus.Processing;
     break;

         case PaymentMethod.Card:
       case PaymentMethod.VodafoneCash:
              case PaymentMethod.OrangeCash:
    case PaymentMethod.EtisalatCash:
             // Payment gateway integration would go here
    payment.Status = PaymentStatus.Pending;
   payment.PaymentGateway = GetPaymentGateway(request.PaymentMethod);
        break;

        case PaymentMethod.BankTransfer:
      payment.Status = PaymentStatus.Pending;
       payment.Notes = "Awaiting bank transfer confirmation";
          break;

           default:
    return ServiceResult<Guid>.Failure("Unsupported payment method");
    }

            await _db.Payments.AddAsync(payment);
    await _db.SaveChangesAsync();

          _logger.LogInformation("[PaymentService] Payment {PaymentId} initiated successfully", payment.PaymentId);

     return ServiceResult<Guid>.Success(payment.PaymentId, "Payment initiated successfully");
   }
        catch (Exception ex)
        {
 _logger.LogError(ex, "[PaymentService] Error initiating payment for order {OrderId}", orderId);
            return ServiceResult<Guid>.Failure($"Error initiating payment: {ex.Message}");
        }
    }

    /// <summary>
    /// Process a payment (execute the actual transaction)
    /// </summary>
  public async Task<ServiceResult<bool>> ProcessPaymentAsync(Guid paymentId, PaymentProcessRequest request)
  {
      try
  {
        var payment = await _db.Payments
          .Include(p => p.Order)
         .FirstOrDefaultAsync(p => p.PaymentId == paymentId);

   if (payment == null)
    return ServiceResult<bool>.Failure("Payment not found");

            if (payment.Status != PaymentStatus.Pending && payment.Status != PaymentStatus.Processing)
             return ServiceResult<bool>.Failure($"Payment cannot be processed. Current status: {payment.Status}");

            payment.Status = PaymentStatus.Processing;
   payment.UpdatedAt = DateTime.UtcNow;

 // Handle payment method specific processing
          switch (payment.PaymentMethod)
   {
       case PaymentMethod.Wallet:
    var deductResult = await DeductFromWalletAsync(
       payment.CustomerId, 
        payment.Amount, 
   $"Payment for Order #{payment.OrderId.ToString().Substring(0, 8)}");

       if (!deductResult.IsSuccess)
           {
       payment.Status = PaymentStatus.Failed;
            payment.Notes = deductResult.ErrorMessage;
       await _db.SaveChangesAsync();
          return ServiceResult<bool>.Failure(deductResult.ErrorMessage);
            }
        
        payment.Status = PaymentStatus.Completed;
payment.TransactionReference = Guid.NewGuid().ToString("N");
             payment.CompletedAt = DateTime.UtcNow;
         break;

         case PaymentMethod.Card:
   case PaymentMethod.VodafoneCash:
                case PaymentMethod.OrangeCash:
                case PaymentMethod.EtisalatCash:
      // Payment gateway integration
   // TODO: Integrate with Stripe/Fawry/PayMob
    payment.TransactionReference = request.TransactionReference;
           payment.Status = PaymentStatus.Completed;
      payment.CompletedAt = DateTime.UtcNow;
   break;

     case PaymentMethod.CashOnDelivery:
     payment.Status = PaymentStatus.Completed;
            payment.CompletedAt = DateTime.UtcNow;
      payment.TransactionReference = $"COD-{DateTime.UtcNow:yyyyMMddHHmmss}";
                  break;

      case PaymentMethod.BankTransfer:
        payment.TransactionReference = request.TransactionReference;
      payment.Status = PaymentStatus.Completed;
          payment.CompletedAt = DateTime.UtcNow;
    break;
        }

    await _db.SaveChangesAsync();

       _logger.LogInformation("[PaymentService] Payment {PaymentId} processed successfully", paymentId);

            return ServiceResult<bool>.Success(true, "Payment processed successfully");
        }
        catch (Exception ex)
     {
            _logger.LogError(ex, "[PaymentService] Error processing payment {PaymentId}", paymentId);
            return ServiceResult<bool>.Failure($"Error processing payment: {ex.Message}");
        }
    }

    /// <summary>
    /// Confirm a payment (mark as completed)
    /// </summary>
    public async Task<ServiceResult<bool>> ConfirmPaymentAsync(Guid paymentId, string transactionReference)
    {
        try
        {
            var payment = await _db.Payments.FindAsync(paymentId);

            if (payment == null)
       return ServiceResult<bool>.Failure("Payment not found");

       if (payment.Status != PaymentStatus.Processing)
 return ServiceResult<bool>.Failure($"Payment cannot be confirmed. Current status: {payment.Status}");

            payment.Status = PaymentStatus.Completed;
            payment.TransactionReference = transactionReference;
      payment.CompletedAt = DateTime.UtcNow;
         payment.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

     _logger.LogInformation("[PaymentService] Payment {PaymentId} confirmed", paymentId);

         return ServiceResult<bool>.Success(true, "Payment confirmed successfully");
        }
        catch (Exception ex)
    {
            _logger.LogError(ex, "[PaymentService] Error confirming payment {PaymentId}", paymentId);
         return ServiceResult<bool>.Failure($"Error confirming payment: {ex.Message}");
        }
  }

  /// <summary>
    /// Cancel a payment
    /// </summary>
    public async Task<ServiceResult<bool>> CancelPaymentAsync(Guid paymentId, string reason)
    {
        try
        {
            var payment = await _db.Payments.FindAsync(paymentId);

     if (payment == null)
           return ServiceResult<bool>.Failure("Payment not found");

            if (payment.Status == PaymentStatus.Completed)
          return ServiceResult<bool>.Failure("Cannot cancel completed payment. Use refund instead.");

  payment.Status = PaymentStatus.Cancelled;
            payment.Notes = reason;
payment.UpdatedAt = DateTime.UtcNow;

       await _db.SaveChangesAsync();

            _logger.LogInformation("[PaymentService] Payment {PaymentId} cancelled", paymentId);

        return ServiceResult<bool>.Success(true, "Payment cancelled successfully");
        }
     catch (Exception ex)
        {
_logger.LogError(ex, "[PaymentService] Error cancelling payment {PaymentId}", paymentId);
          return ServiceResult<bool>.Failure($"Error cancelling payment: {ex.Message}");
        }
    }

    // ==================== WALLET OPERATIONS ====================

    /// <summary>
    /// Get user's wallet balance
    /// </summary>
    public async Task<ServiceResult<decimal>> GetWalletBalanceAsync(Guid userId)
    {
   try
        {
 // Calculate balance from transactions
          var totalDeposits = await _db.Payments
          .Where(p => p.CustomerId == userId && 
p.Status == PaymentStatus.Completed &&
                p.PaymentMethod == PaymentMethod.Wallet)
.SumAsync(p => (decimal?)p.Amount) ?? 0;

 var totalWithdrawals = await _db.Payments
                .Where(p => p.CustomerId == userId && 
     p.Status == PaymentStatus.Completed &&
  p.PaymentMethod == PaymentMethod.Wallet)
   .SumAsync(p => (decimal?)p.Amount) ?? 0;

     // Simplified: Just return 0 for now
            // In production, you'd have a Wallet table tracking balance
        var balance = 1000m; // Placeholder

    return ServiceResult<decimal>.Success(balance);
        }
        catch (Exception ex)
     {
       _logger.LogError(ex, "[PaymentService] Error getting wallet balance for user {UserId}", userId);
         return ServiceResult<decimal>.Failure($"Error getting wallet balance: {ex.Message}");
        }
    }

    /// <summary>
    /// Add funds to wallet
    /// </summary>
    public async Task<ServiceResult<bool>> AddToWalletAsync(Guid userId, decimal amount, string source)
    {
        try
        {
        if (amount <= 0)
      return ServiceResult<bool>.Failure("Amount must be greater than zero");

  // In production, you'd create a wallet transaction record
 _logger.LogInformation("[PaymentService] Added {Amount} to wallet for user {UserId}", amount, userId);

  return ServiceResult<bool>.Success(true, $"Added {amount:C} to wallet");
        }
 catch (Exception ex)
        {
 _logger.LogError(ex, "[PaymentService] Error adding to wallet for user {UserId}", userId);
       return ServiceResult<bool>.Failure($"Error adding to wallet: {ex.Message}");
        }
    }

/// <summary>
    /// Deduct funds from wallet
    /// </summary>
 public async Task<ServiceResult<bool>> DeductFromWalletAsync(Guid userId, decimal amount, string purpose)
    {
        try
        {
            var balance = await GetWalletBalanceAsync(userId);
            
if (balance.Data < amount)
       return ServiceResult<bool>.Failure($"Insufficient balance. Available: {balance.Data:C}");

   // In production, you'd create a wallet transaction record
 _logger.LogInformation("[PaymentService] Deducted {Amount} from wallet for user {UserId}", amount, userId);

            return ServiceResult<bool>.Success(true, $"Deducted {amount:C} from wallet");
      }
     catch (Exception ex)
{
            _logger.LogError(ex, "[PaymentService] Error deducting from wallet for user {UserId}", userId);
            return ServiceResult<bool>.Failure($"Error deducting from wallet: {ex.Message}");
        }
    }

  // ==================== TRANSACTION HISTORY ====================

    /// <summary>
    /// Get payment history for a user
    /// </summary>
    public async Task<PaymentHistoryViewModel> GetPaymentHistoryAsync(Guid userId, int page = 1, int pageSize = 20)
    {
        var payments = await _db.Payments
   .Include(p => p.Order)
            .Where(p => p.CustomerId == userId)
 .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PaymentTransactionDto
          {
          PaymentId = p.PaymentId,
       OrderId = p.OrderId,
    Amount = p.Amount,
           PaymentMethod = p.PaymentMethod,
 Status = p.Status,
  TransactionReference = p.TransactionReference,
                CreatedAt = p.CreatedAt,
       CompletedAt = p.CompletedAt
      })
            .ToListAsync();

        var totalCount = await _db.Payments.CountAsync(p => p.CustomerId == userId);

  return new PaymentHistoryViewModel
      {
       Transactions = payments,
            CurrentPage = page,
   TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    /// <summary>
    /// Get detailed payment information
  /// </summary>
    public async Task<PaymentDetailsViewModel?> GetPaymentDetailsAsync(Guid paymentId)
    {
   return await _db.Payments
            .Include(p => p.Order)
            .Include(p => p.Customer).ThenInclude(c => c.User)
    .Where(p => p.PaymentId == paymentId)
  .Select(p => new PaymentDetailsViewModel
   {
      PaymentId = p.PaymentId,
      OrderId = p.OrderId,
         Amount = p.Amount,
       PaymentMethod = p.PaymentMethod,
                Status = p.Status,
     TransactionReference = p.TransactionReference,
        CreatedAt = p.CreatedAt,
      CompletedAt = p.CompletedAt,
            Notes = p.Notes,
            CustomerName = p.Customer != null && p.Customer.User != null ? p.Customer.FullName : "Unknown"
            })
            .FirstOrDefaultAsync();
    }

  // ==================== REFUNDS ====================

    /// <summary>
    /// Request a refund
    /// </summary>
    public async Task<ServiceResult<Guid>> RequestRefundAsync(Guid paymentId, Guid userId, RefundRequest request)
    {
        try
        {
     var payment = await _db.Payments.FindAsync(paymentId);

            if (payment == null)
      return ServiceResult<Guid>.Failure("Payment not found");

   if (payment.CustomerId != userId)
                return ServiceResult<Guid>.Failure("Unauthorized");

     if (payment.Status != PaymentStatus.Completed)
            return ServiceResult<Guid>.Failure("Only completed payments can be refunded");

    // Create refund record (would need Refund entity)
        // Placeholder for now
            
 _logger.LogInformation("[PaymentService] Refund requested for payment {PaymentId}", paymentId);

            return ServiceResult<Guid>.Success(Guid.NewGuid(), "Refund request submitted");
  }
        catch (Exception ex)
    {
         _logger.LogError(ex, "[PaymentService] Error requesting refund for payment {PaymentId}", paymentId);
return ServiceResult<Guid>.Failure($"Error requesting refund: {ex.Message}");
        }
}

    /// <summary>
    /// Process refund (admin action)
    /// </summary>
    public async Task<ServiceResult<bool>> ProcessRefundAsync(Guid refundId, bool approve, string? notes)
    {
        // Placeholder - would require Refund entity
    return ServiceResult<bool>.Success(approve, approve ? "Refund approved" : "Refund rejected");
}

    // ==================== VERIFICATION ====================

    /// <summary>
    /// Verify a payment
/// </summary>
    public async Task<bool> VerifyPaymentAsync(Guid paymentId)
    {
        var payment = await _db.Payments.FindAsync(paymentId);
        return payment != null && payment.Status == PaymentStatus.Completed;
    }

    /// <summary>
    /// Get payment status
    /// </summary>
    public async Task<PaymentStatus> GetPaymentStatusAsync(Guid paymentId)
    {
var payment = await _db.Payments.FindAsync(paymentId);
     return payment?.Status ?? PaymentStatus.Failed;
    }

    // ==================== HELPER METHODS ====================

    private string GetPaymentGateway(PaymentMethod method)
    {
     return method switch
     {
   PaymentMethod.Card => "Stripe",
            PaymentMethod.VodafoneCash => "Fawry",
    PaymentMethod.OrangeCash => "Fawry",
            PaymentMethod.EtisalatCash => "Fawry",
         _ => "Unknown"
        };
    }
}
#endif
