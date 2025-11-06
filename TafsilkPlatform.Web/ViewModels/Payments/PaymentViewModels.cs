#if FALSE // ⚠️ DISABLED: Payment ViewModels disabled until PaymentService is refactored
// TODO: Update to use Enums.PaymentType instead of PaymentMethod
// See: Docs/BUILD_ERROR_ANALYSIS.md for details

using System.ComponentModel.DataAnnotations;
using TafsilkPlatform.Web.Models;

namespace TafsilkPlatform.Web.ViewModels.Payments;

// ============================================
// PAYMENT REQUEST
// ============================================

/// <summary>
/// Request to initiate a payment
/// </summary>
public class PaymentRequest
{
    [Required]
    public Guid OrderId { get; set; }

    [Required]
    [Range(0.01, 1000000, ErrorMessage = "Amount must be between 0.01 and 1,000,000")]
    public decimal Amount { get; set; }

    [Required]
    public PaymentMethod PaymentMethod { get; set; }

    public string? Notes { get; set; }
}


/// <summary>
    /// Request to process a payment
    /// </summary>
public class PaymentProcessRequest
{
    public string? TransactionReference { get; set; }
    public string? PaymentGatewayResponse { get; set; }
}

// ============================================
// PAYMENT DISPLAY
// ============================================

/// <summary>
/// ViewModel for payment page
/// </summary>
public class PaymentViewModel
{
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
public string Currency { get; set; } = "EGP";
    
    // Available payment methods
    public List<PaymentMethodOption> PaymentMethods { get; set; } = new();
    
    // Order details
    public string TailorName { get; set; } = string.Empty;
    public string ServiceType { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    
    // Wallet balance
    public decimal WalletBalance { get; set; }
    public bool CanPayWithWallet => WalletBalance >= Amount;
}


/// <summary>
    /// Payment method option
    /// </summary>
public class PaymentMethodOption
{
    public PaymentMethod Method { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public bool IsAvailable { get; set; } = true;
    public decimal? Fee { get; set; }
}


/// <summary>
    /// Payment details view
    /// </summary>
public class PaymentDetailsViewModel
{
    public Guid PaymentId { get; set; }
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public PaymentStatus Status { get; set; }
    public string? TransactionReference { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Notes { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    
    // Computed properties
    public string StatusDisplay => GetStatusDisplay(Status);
    public string MethodDisplay => GetMethodDisplay(PaymentMethod);
    public string DurationMinutes => CompletedAt.HasValue 
      ? (CompletedAt.Value - CreatedAt).TotalMinutes.ToString("F0") 
   : "N/A";
    
    private string GetStatusDisplay(PaymentStatus status) => status switch
  {
        PaymentStatus.Pending => "قيد الانتظار",
    PaymentStatus.Processing => "جارِ المعالجة",
        PaymentStatus.Completed => "مكتمل",
        PaymentStatus.Failed => "فشل",
        PaymentStatus.Cancelled => "ملغي",
 PaymentStatus.Refunded => "مسترد",
        _ => "غير معروف"
    };
    
    private string GetMethodDisplay(PaymentMethod method) => method switch
    {
   PaymentMethod.CashOnDelivery => "الدفع عند الاستلام",
     PaymentMethod.Card => "بطاقة ائتمان",
        PaymentMethod.Wallet => "المحفظة",
        PaymentMethod.VodafoneCash => "فودافون كاش",
        PaymentMethod.OrangeCash => "أورنج كاش",
        PaymentMethod.EtisalatCash => "اتصالات كاش",
      PaymentMethod.BankTransfer => "تحويل بنكي",
        _ => "غير معروف"
    };
}

// ============================================
// PAYMENT HISTORY
// ============================================

/// <summary>
/// Payment history view
/// </summary>
public class PaymentHistoryViewModel
{
    public List<PaymentTransactionDto> Transactions { get; set; } = new();
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    
    // Statistics
    public decimal TotalSpent { get; set; }
    public int TotalTransactions { get; set; }
    public int CompletedTransactions { get; set; }
}

/// <summary>
/// Payment transaction DTO
/// </summary>
public class PaymentTransactionDto
{
    public Guid PaymentId { get; set; }
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public PaymentStatus Status { get; set; }
    public string? TransactionReference { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
  
    public string StatusBadgeClass => Status switch
    {
     PaymentStatus.Completed => "badge-success",
     PaymentStatus.Pending => "badge-warning",
        PaymentStatus.Processing => "badge-info",
   PaymentStatus.Failed => "badge-danger",
        PaymentStatus.Cancelled => "badge-secondary",
        PaymentStatus.Refunded => "badge-primary",
        _ => "badge-secondary"
    };
}

// ============================================
// WALLET
// ============================================

/// <summary>
/// Wallet view model
/// </summary>
public class WalletViewModel
{
    public Guid UserId { get; set; }
    public decimal Balance { get; set; }
    public List<WalletTransactionDto> Transactions { get; set; } = new();
    
    // Statistics
    public decimal TotalDeposits { get; set; }
    public decimal TotalWithdrawals { get; set; }
    public int TransactionCount { get; set; }
}

/// <summary>
/// Wallet transaction DTO
/// </summary>
public class WalletTransactionDto
{
    public Guid TransactionId { get; set; }
    public string Type { get; set; } = string.Empty; // Deposit, Withdrawal, Payment
    public decimal Amount { get; set; }
    public decimal BalanceAfter { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    
    public bool IsDeposit => Type == "Deposit";
    public string AmountDisplay => IsDeposit ? $"+{Amount:C}" : $"-{Amount:C}";
}


/// <summary>
    /// Add funds to wallet request
    /// </summary>
public class AddToWalletRequest
{
  [Required]
    [Range(10, 10000, ErrorMessage = "Amount must be between 10 and 10,000 EGP")]
    public decimal Amount { get; set; }

  [Required]
    public PaymentMethod PaymentMethod { get; set; }
}

// ============================================
// REFUND
// ============================================

/// <summary>
/// Refund request
/// </summary>
public class RefundRequest
{
    [Required]
    public Guid PaymentId { get; set; }

    [Required]
    [StringLength(500, MinimumLength = 10, ErrorMessage = "Reason must be between 10 and 500 characters")]
    public string Reason { get; set; } = string.Empty;

    public decimal? RefundAmount { get; set; }
}

/// <summary>
/// Refund details view
/// </summary>
public class RefundDetailsViewModel
{
    public Guid RefundId { get; set; }
    public Guid PaymentId { get; set; }
  public Guid OrderId { get; set; }
    public decimal RefundAmount { get; set; }
    public string Reason { get; set; } = string.Empty;
  public string Status { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? AdminNotes { get; set; }
}

// ============================================
// PAYMENT GATEWAY
// ============================================

/// <summary>
/// Payment gateway redirect data
/// </summary>
public class PaymentGatewayRedirectViewModel
{
    public string GatewayUrl { get; set; } = string.Empty;
    public Dictionary<string, string> Parameters { get; set; } = new();
public string Method { get; set; } = "POST";
}

/// <summary>
/// Payment gateway callback
/// </summary>
public class PaymentGatewayCallbackDto
{
    public string TransactionReference { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string? Signature { get; set; }
    public Dictionary<string, string> AdditionalData { get; set; } = new();
}

// ============================================
// PAYMENT STATISTICS (ADMIN)
// ============================================

/// <summary>
/// Payment statistics for admin dashboard
/// </summary>
public class PaymentStatisticsViewModel
{
    public decimal TotalRevenue { get; set; }
    public decimal RevenueToday { get; set; }
    public decimal RevenueThisMonth { get; set; }
    
    public int TotalTransactions { get; set; }
    public int CompletedTransactions { get; set; }
    public int PendingTransactions { get; set; }
    public int FailedTransactions { get; set; }
    
    // Payment method breakdown
    public Dictionary<PaymentMethod, int> PaymentMethodDistribution { get; set; } = new();
    public Dictionary<PaymentMethod, decimal> RevenueByMethod { get; set; } = new();
 
    // Daily revenue chart data (last 30 days)
    public List<DailyRevenueDto> DailyRevenue { get; set; } = new();
}

public class DailyRevenueDto
{
    public DateOnly Date { get; set; }
    public decimal Revenue { get; set; }
    public int TransactionCount { get; set; }
    
    public string DateLabel => Date.ToString("dd MMM");
}
#endif
