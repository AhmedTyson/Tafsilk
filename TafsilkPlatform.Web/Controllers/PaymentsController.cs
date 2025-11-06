#if FALSE // ⚠️ DISABLED: PaymentService doesn't match current Payment model
// TODO: Refactor PaymentService to align with existing Payment entity before re-enabling
// See: Docs/BUILD_ERROR_ANALYSIS.md for details

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TafsilkPlatform.Web.Controllers.Base;
using TafsilkPlatform.Web.Data;
using TafsilkPlatform.Web.Models;
using TafsilkPlatform.Web.Services;
using TafsilkPlatform.Web.ViewModels.Payments;

namespace TafsilkPlatform.Web.Controllers;

/// <summary>
/// Controller for handling payments and wallet operations
/// </summary>
public class PaymentsController : BaseController
{
    private readonly AppDbContext _db;
    private readonly IPaymentService _paymentService;

    public PaymentsController(
  AppDbContext db,
        IPaymentService paymentService,
    ILogger<PaymentsController> logger) : base(logger)
{
     _db = db ?? throw new ArgumentNullException(nameof(db));
   _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
    }

    // ============================================
    // PAYMENT PROCESSING
  // ============================================

    /// <summary>
    /// Display payment page for an order
    /// GET: /Payments/Pay/{orderId}
    /// </summary>
    [HttpGet]
    [Route("Payments/Pay/{orderId:guid}")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> Pay(Guid orderId)
    {
        try
        {
var customerId = GetUserId();

            // Get order details
     var order = await _db.Orders
    .Include(o => o.Tailor)
           .FirstOrDefaultAsync(o => o.OrderId == orderId && o.CustomerId == customerId);

        if (order == null)
   {
 TempData["Error"] = "الطلب غير موجود";
       return RedirectToAction("MyOrders", "Orders");
            }

       // Get wallet balance
      var walletBalance = await _paymentService.GetWalletBalanceAsync(customerId);

       var viewModel = new PaymentViewModel
         {
         OrderId = orderId,
         OrderNumber = orderId.ToString().Substring(0, 8).ToUpper(),
 Amount = (decimal)order.TotalPrice,
         Currency = "EGP",
    TailorName = order.Tailor?.ShopName ?? "Unknown",
        ServiceType = order.OrderType,
OrderDate = order.CreatedAt.DateTime,
     WalletBalance = walletBalance.Data,
        PaymentMethods = GetAvailablePaymentMethods()
     };

     return View(viewModel);
        }
 catch (Exception ex)
   {
     _logger.LogError(ex, "Error loading payment page for order {OrderId}", orderId);
            TempData["Error"] = "حدث خطأ أثناء تحميل صفحة الدفع";
        return RedirectToAction("MyOrders", "Orders");
        }
 }

    /// <summary>
    /// Process payment
    /// POST: /Payments/Process
    /// </summary>
  [HttpPost]
    [Route("Payments/Process")]
  [Authorize(Roles = "Customer")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Process(PaymentRequest request)
    {
 try
 {
   if (!ModelState.IsValid)
  {
           TempData["Error"] = "يرجى تصحيح الأخطاء في النموذج";
        return RedirectToAction(nameof(Pay), new { orderId = request.OrderId });
            }

        var customerId = GetUserId();

      // Initiate payment
      var initiateResult = await _paymentService.InitiatePaymentAsync(
        request.OrderId,
   customerId,
 request);

          if (!initiateResult.IsSuccess)
     {
          TempData["Error"] = initiateResult.ErrorMessage;
                return RedirectToAction(nameof(Pay), new { orderId = request.OrderId });
  }

            var paymentId = initiateResult.Data;

 // Process payment based on method
      switch (request.PaymentMethod)
            {
                case PaymentMethod.Wallet:
     case PaymentMethod.CashOnDelivery:
              // Process immediately
      var processResult = await _paymentService.ProcessPaymentAsync(
     paymentId,
    new PaymentProcessRequest());

      if (!processResult.IsSuccess)
  {
        TempData["Error"] = processResult.ErrorMessage;
      return RedirectToAction(nameof(Pay), new { orderId = request.OrderId });
      }

          TempData["Success"] = "تم الدفع بنجاح!";
          return RedirectToAction(nameof(PaymentSuccess), new { paymentId });

             case PaymentMethod.Card:
        case PaymentMethod.VodafoneCash:
   case PaymentMethod.OrangeCash:
       case PaymentMethod.EtisalatCash:
           // Redirect to payment gateway
          return RedirectToAction(nameof(PaymentGateway), new { paymentId });

           case PaymentMethod.BankTransfer:
         return RedirectToAction(nameof(BankTransferInstructions), new { paymentId });

      default:
          TempData["Error"] = "طريقة الدفع غير مدعومة";
      return RedirectToAction(nameof(Pay), new { orderId = request.OrderId });
     }
        }
        catch (Exception ex)
    {
   _logger.LogError(ex, "Error processing payment for order {OrderId}", request.OrderId);
     TempData["Error"] = "حدث خطأ أثناء معالجة الدفع";
  return RedirectToAction(nameof(Pay), new { orderId = request.OrderId });
        }
    }

    /// <summary>
    /// Payment success page
    /// GET: /Payments/Success/{paymentId}
    /// </summary>
    [HttpGet]
    [Route("Payments/Success/{paymentId:guid}")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> PaymentSuccess(Guid paymentId)
    {
   try
        {
      var payment = await _paymentService.GetPaymentDetailsAsync(paymentId);

            if (payment == null)
      {
    return NotFound("Payment not found");
     }

        return View(payment);
        }
        catch (Exception ex)
        {
     _logger.LogError(ex, "Error loading payment success page for {PaymentId}", paymentId);
            return RedirectToAction("MyOrders", "Orders");
        }
    }

    /// <summary>
    /// Payment gateway redirect page
    /// GET: /Payments/Gateway/{paymentId}
    /// </summary>
    [HttpGet]
    [Route("Payments/Gateway/{paymentId:guid}")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> PaymentGateway(Guid paymentId)
    {
    try
        {
            var payment = await _db.Payments.FindAsync(paymentId);

        if (payment == null)
      {
        return NotFound("Payment not found");
  }

     // TODO: Integrate with actual payment gateway (Stripe/Fawry/PayMob)
   // For now, show placeholder
            TempData["Info"] = "Payment gateway integration is under development. Payment marked as pending.";
            return RedirectToAction(nameof(Pay), new { orderId = payment.OrderId });
        }
        catch (Exception ex)
        {
     _logger.LogError(ex, "Error redirecting to payment gateway for {PaymentId}", paymentId);
            TempData["Error"] = "حدث خطأ أثناء الانتقال لبوابة الدفع";
        return RedirectToAction("MyOrders", "Orders");
        }
    }

    /// <summary>
    /// Bank transfer instructions
    /// GET: /Payments/BankTransfer/{paymentId}
    /// </summary>
    [HttpGet]
    [Route("Payments/BankTransfer/{paymentId:guid}")]
    [Authorize(Roles = "Customer")]
  public async Task<IActionResult> BankTransferInstructions(Guid paymentId)
    {
     try
   {
            var payment = await _paymentService.GetPaymentDetailsAsync(paymentId);

      if (payment == null)
    {
 return NotFound("Payment not found");
      }

 return View(payment);
        }
        catch (Exception ex)
  {
            _logger.LogError(ex, "Error loading bank transfer instructions for {PaymentId}", paymentId);
        return RedirectToAction("MyOrders", "Orders");
        }
    }

    // ============================================
    // PAYMENT HISTORY
    // ============================================

    /// <summary>
    /// View payment history
    /// GET: /Payments/History
    /// </summary>
    [HttpGet]
    [Route("Payments/History")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> History(int page = 1)
    {
        try
        {
    var userId = GetUserId();
            var viewModel = await _paymentService.GetPaymentHistoryAsync(userId, page);

     return View(viewModel);
        }
        catch (Exception ex)
     {
            _logger.LogError(ex, "Error loading payment history");
            TempData["Error"] = "حدث خطأ أثناء تحميل سجل المدفوعات";
        return RedirectToAction("Index", "Home");
        }
    }

    /// <summary>
    /// View payment details
    /// GET: /Payments/Details/{paymentId}
    /// </summary>
    [HttpGet]
    [Route("Payments/Details/{paymentId:guid}")]
    [Authorize]
    public async Task<IActionResult> Details(Guid paymentId)
    {
    try
   {
            var payment = await _paymentService.GetPaymentDetailsAsync(paymentId);

      if (payment == null)
 {
      return NotFound("Payment not found");
    }

          return View(payment);
        }
        catch (Exception ex)
  {
            _logger.LogError(ex, "Error loading payment details for {PaymentId}", paymentId);
  TempData["Error"] = "حدث خطأ أثناء تحميل تفاصيل الدفع";
  return RedirectToAction(nameof(History));
        }
    }

    // ============================================
    // WALLET OPERATIONS
    // ============================================

    /// <summary>
 /// View wallet
    /// GET: /Payments/Wallet
    /// </summary>
    [HttpGet]
    [Route("Payments/Wallet")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> Wallet()
    {
        try
        {
       var userId = GetUserId();
        var balance = await _paymentService.GetWalletBalanceAsync(userId);

          var viewModel = new WalletViewModel
   {
     UserId = userId,
         Balance = balance.Data,
    Transactions = new List<WalletTransactionDto>()
            };

        return View(viewModel);
    }
        catch (Exception ex)
  {
      _logger.LogError(ex, "Error loading wallet");
            TempData["Error"] = "حدث خطأ أثناء تحميل المحفظة";
      return RedirectToAction("Index", "Home");
        }
    }

    /// <summary>
    /// Add funds to wallet
    /// POST: /Payments/Wallet/Add
 /// </summary>
    [HttpPost]
    [Route("Payments/Wallet/Add")]
    [Authorize(Roles = "Customer")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddToWallet(AddToWalletRequest request)
    {
        try
        {
       if (!ModelState.IsValid)
   {
     return BadRequest(ModelState);
      }

            var userId = GetUserId();
      var result = await _paymentService.AddToWalletAsync(userId, request.Amount, "Manual deposit");

    if (!result.IsSuccess)
  {
           TempData["Error"] = result.ErrorMessage;
            }
 else
   {
            TempData["Success"] = result.Message;
            }

      return RedirectToAction(nameof(Wallet));
    }
      catch (Exception ex)
    {
 _logger.LogError(ex, "Error adding to wallet");
       TempData["Error"] = "حدث خطأ أثناء إضافة الأموال";
            return RedirectToAction(nameof(Wallet));
        }
    }

    // ============================================
    // REFUNDS
    // ============================================

    /// <summary>
  /// Request refund
 /// POST: /Payments/Refund/Request
    /// </summary>
    [HttpPost]
    [Route("Payments/Refund/Request")]
    [Authorize(Roles = "Customer")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RequestRefund(RefundRequest request)
    {
        try
        {
         if (!ModelState.IsValid)
            {
         TempData["Error"] = "يرجى تصحيح الأخطاء في النموذج";
       return RedirectToAction(nameof(Details), new { paymentId = request.PaymentId });
            }

          var userId = GetUserId();
            var result = await _paymentService.RequestRefundAsync(request.PaymentId, userId, request);

if (!result.IsSuccess)
     {
            TempData["Error"] = result.ErrorMessage;
      }
     else
            {
 TempData["Success"] = "تم إرسال طلب الاسترداد بنجاح";
         }

            return RedirectToAction(nameof(Details), new { paymentId = request.PaymentId });
        }
        catch (Exception ex)
        {
        _logger.LogError(ex, "Error requesting refund for payment {PaymentId}", request.PaymentId);
TempData["Error"] = "حدث خطأ أثناء طلب الاسترداد";
            return RedirectToAction(nameof(Details), new { paymentId = request.PaymentId });
        }
    }

    // ============================================
    // HELPER METHODS
    // ============================================

    private List<PaymentMethodOption> GetAvailablePaymentMethods()
    {
      return new List<PaymentMethodOption>
        {
        new()
            {
    Method = PaymentMethod.Wallet,
    Name = "المحفظة",
  Description = "ادفع من رصيد محفظتك",
          Icon = "fa-wallet",
     IsAvailable = true
   },
  new()
  {
                Method = PaymentMethod.CashOnDelivery,
          Name = "الدفع عند الاستلام",
 Description = "ادفع نقداً عند استلام الطلب",
    Icon = "fa-money-bill-wave",
    IsAvailable = true
  },
new()
     {
      Method = PaymentMethod.Card,
        Name = "بطاقة ائتمان",
        Description = "ادفع ببطاقة Visa أو Mastercard",
    Icon = "fa-credit-card",
 IsAvailable = true
         },
   new()
            {
  Method = PaymentMethod.VodafoneCash,
    Name = "فودافون كاش",
         Description = "ادفع من حسابك على فودافون كاش",
 Icon = "fa-mobile-alt",
        IsAvailable = true
   },
          new()
            {
       Method = PaymentMethod.OrangeCash,
    Name = "أورنج كاش",
    Description = "ادفع من حسابك على أورنج كاش",
                Icon = "fa-mobile-alt",
  IsAvailable = true
     },
      new()
            {
      Method = PaymentMethod.EtisalatCash,
  Name = "اتصالات كاش",
         Description = "ادفع من حسابك على اتصالات كاش",
        Icon = "fa-mobile-alt",
     IsAvailable = true
            },
         new()
    {
                Method = PaymentMethod.BankTransfer,
    Name = "تحويل بنكي",
       Description = "حول المبلغ إلى حسابنا البنكي",
          Icon = "fa-university",
         IsAvailable = true
            }
        };
    }
}
#endif
