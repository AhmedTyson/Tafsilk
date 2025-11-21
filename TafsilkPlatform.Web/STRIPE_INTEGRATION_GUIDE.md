# 💳 Stripe Payment Integration Guide - Tafsilk Platform

## ✅ CURRENT STATUS

**Build Status:** ✅ Ready for Stripe Integration  
**Current Payment Methods:** Cash on Delivery ✅  
**Stripe Support:** Infrastructure Ready, Awaiting Configuration

---

## 📋 WHAT'S BEEN PREPARED

### 1. **Payment Infrastructure** ✅ COMPLETE

#### Payment Model Enhanced
**File:** `TafsilkPlatform.Web\Models\Payment.cs`

**New Fields Added:**
- `ProviderTransactionId` - Stripe PaymentIntent ID
- `ProviderCustomerId` - Stripe Customer ID (for saved cards)
- `Provider` - Payment provider name (Internal, Stripe, PayPal)
- `ProviderMetadata` - Provider-specific data (JSON)
- `CardLast4` - Last 4 digits for display
- `CardBrand` - Visa, MasterCard, Amex, etc.
- `Currency` - SAR, USD, EUR, etc.
- `ThreeDSecureUsed` - 3D Secure authentication flag
- `FailureReason` - Error details if payment fails
- `RefundedAmount` - Partial/full refund tracking
- `RefundedAt` - Refund timestamp
- `Notes` - Additional information
- `CreatedAt` / `UpdatedAt` - Audit trail

#### Payment Processor Service
**File:** `TafsilkPlatform.Web\Services\Payment\PaymentProcessorService.cs`

**Features:**
- ✅ Cash on Delivery support (active)
- ✅ Stripe integration placeholders (ready for activation)
- ✅ Payment validation
- ✅ Refund processing
- ✅ Payment status tracking
- ✅ Automatic error handling via BaseService
- ✅ Transaction management via UnitOfWork

**Methods:**
```csharp
// Process any payment type (Cash, Stripe, etc.)
Task<Result<PaymentProcessingResult>> ProcessPaymentAsync(PaymentProcessingRequest request)

// Create Stripe PaymentIntent (frontend confirms)
Task<Result<string>> CreatePaymentIntentAsync(CreatePaymentIntentRequest request)

// Handle Stripe webhooks
Task<Result> ConfirmStripePaymentAsync(string paymentIntentId, string signature)

// Process refunds
Task<Result<RefundResult>> ProcessRefundAsync(Guid paymentId, decimal amount, string reason)

// Check payment status
Task<Result<PaymentStatusResult>> GetPaymentStatusAsync(Guid paymentId)
```

### 2. **Configuration System** ✅ READY

**File:** `TafsilkPlatform.Web\appsettings.Payment.json`

**Configuration Template:**
```json
{
  "Payment": {
    "Stripe": {
      "Enabled": false,              // ⚠️ Set to true when ready
      "SecretKey": "",               // ⚠️ Add your Stripe secret key
      "PublishableKey": "",          // ⚠️ Add your Stripe publishable key
      "WebhookSecret": "",           // ⚠️ Add webhook signing secret
      "Currency": "SAR",
      "Require3DSecure": true,
      "WebhookUrl": "/api/webhooks/stripe"
    },
    "CashOnDelivery": {
      "Enabled": true,
      "MinimumOrderAmount": 0,
      "MaximumOrderAmount": 10000
    }
  }
}
```

### 3. **Database Migration** ✅ TEMPLATE PROVIDED

**File:** `TafsilkPlatform.Web\Migrations\TEMPLATE_AddStripeFieldsToPayment.cs`

**To Apply:**
```bash
# 1. Create migration from template
dotnet ef migrations add AddStripeFieldsToPayment

# 2. Review the generated migration

# 3. Apply to database
dotnet ef database update
```

---

## 🚀 HOW TO INTEGRATE STRIPE (WHEN READY)

### Step 1: Install Stripe.NET Package

```bash
dotnet add package Stripe.net
```

### Step 2: Configure Stripe Keys

**Option A: Development (User Secrets - Recommended)**
```bash
dotnet user-secrets set "Payment:Stripe:SecretKey" "sk_test_your_key_here"
dotnet user-secrets set "Payment:Stripe:PublishableKey" "pk_test_your_key_here"
dotnet user-secrets set "Payment:Stripe:WebhookSecret" "whsec_your_webhook_secret"
dotnet user-secrets set "Payment:Stripe:Enabled" "true"
```

**Option B: Production (Environment Variables)**
```bash
# Set in Azure App Service, AWS, etc.
PAYMENT__STRIPE__SECRETKEY=sk_live_your_key_here
PAYMENT__STRIPE__PUBLISHABLEKEY=pk_live_your_key_here
PAYMENT__STRIPE__WEBHOOKSECRET=whsec_your_webhook_secret
PAYMENT__STRIPE__ENABLED=true
```

### Step 3: Run Database Migration

```bash
dotnet ef migrations add AddStripeFieldsToPayment
dotnet ef database update
```

### Step 4: Uncomment Stripe Code

**File:** `TafsilkPlatform.Web\Services\Payment\PaymentProcessorService.cs`

Look for comments like:
```csharp
// ✅ READY FOR STRIPE INTEGRATION
// TODO: When Stripe is integrated, uncomment and implement:
/*
var stripe = new StripeClient(_stripeSecretKey);
...
*/
```

### Step 5: Create Stripe Webhook Controller

**File:** `TafsilkPlatform.Web\Controllers\WebhooksController.cs` (Create New)

```csharp
using Microsoft.AspNetCore.Mvc;
using TafsilkPlatform.Web.Services.Payment;

namespace TafsilkPlatform.Web.Controllers;

[ApiController]
[Route("api/webhooks")]
public class WebhooksController : ControllerBase
{
    private readonly IPaymentProcessorService _paymentProcessor;
    private readonly ILogger<WebhooksController> _logger;

    public WebhooksController(
        IPaymentProcessorService paymentProcessor,
        ILogger<WebhooksController> logger)
    {
        _paymentProcessor = paymentProcessor;
        _logger = logger;
    }

    [HttpPost("stripe")]
    public async Task<IActionResult> StripeWebhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var stripeSignature = Request.Headers["Stripe-Signature"];

        try
        {
            var result = await _paymentProcessor.ConfirmStripePaymentAsync(
                json, 
                stripeSignature!);

            if (result.IsSuccess)
            {
                return Ok();
            }

            _logger.LogWarning("Stripe webhook failed: {Error}", result.Error);
            return BadRequest();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Stripe webhook");
            return StatusCode(500);
        }
    }
}
```

### Step 6: Configure Stripe Dashboard

1. **Go to:** https://dashboard.stripe.com
2. **Get API Keys:** Developers → API Keys
3. **Set up Webhooks:** Developers → Webhooks → Add endpoint
   - URL: `https://your-domain.com/api/webhooks/stripe`
   - Events to listen for:
     - `payment_intent.succeeded`
     - `payment_intent.payment_failed`
     - `charge.refunded`
4. **Get Webhook Secret:** Copy the signing secret

---

## 📊 CURRENT CHECKOUT FLOW

### Cash on Delivery (Active Now)

```
Customer → Checkout → Select "Cash on Delivery" → Submit Order
    ↓
Order Created (Status: Confirmed)
    ↓
Payment Created (Status: Pending)
    ↓
Redirect to Order History (Order shows as "In Progress")
    ↓
Driver Delivers → Customer Pays Cash → Admin marks as Completed
```

### Credit Card (When Stripe is Configured)

```
Customer → Checkout → Select "Credit Card" → Enter Card Details
    ↓
Create Stripe PaymentIntent
    ↓
Frontend: Stripe.js confirms payment
    ↓
3D Secure Authentication (if required)
    ↓
Stripe Webhook: payment_intent.succeeded
    ↓
Order Created (Status: Confirmed)
    ↓
Payment Created (Status: Completed)
    ↓
Redirect to Order History (Order shows as "Paid")
```

---

## 🎯 FRONTEND INTEGRATION (WHEN READY)

### 1. Add Stripe.js to Layout

**File:** `Views/Shared/_Layout.cshtml` or checkout page

```html
<script src="https://js.stripe.com/v3/"></script>
```

### 2. Create Checkout Form

**File:** `Views/Store/Checkout.cshtml` (or your checkout page)

```html
<form id="payment-form">
    <!-- Card Element will be inserted here -->
    <div id="card-element"></div>
    
    <!-- Display errors -->
    <div id="card-errors" role="alert"></div>
    
    <button type="submit" id="submit-button">
        Pay SAR <span id="total-amount">@Model.Cart.Total</span>
    </button>
</form>

<script>
    // Get publishable key from backend
    var stripe = Stripe('@ViewBag.StripePublishableKey');
    var elements = stripe.elements();
    
    // Create card element
    var cardElement = elements.create('card');
    cardElement.mount('#card-element');
    
    // Handle form submission
    var form = document.getElementById('payment-form');
    form.addEventListener('submit', async function(event) {
        event.preventDefault();
        
        // Create PaymentIntent on backend
        var response = await fetch('/api/payment/create-intent', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                orderId: '@Model.OrderId',
                amount: @Model.Cart.Total,
                currency: 'sar'
            })
        });
        
        var data = await response.json();
        
        // Confirm payment with Stripe
        var result = await stripe.confirmCardPayment(data.clientSecret, {
            payment_method: {
                card: cardElement,
                billing_details: {
                    name: '@Model.Customer.FullName',
                    email: '@Model.Customer.Email'
                }
            }
        });
        
        if (result.error) {
            // Show error
            document.getElementById('card-errors').textContent = result.error.message;
        } else {
            // Payment successful - redirect to order confirmation
            window.location.href = '/orders/' + '@Model.OrderId';
        }
    });
</script>
```

---

## 🔐 SECURITY BEST PRACTICES

### 1. **Never Store Card Details**
✅ Let Stripe handle all card data  
✅ Only store: `CardLast4`, `CardBrand`, `ProviderTransactionId`

### 2. **Use Webhook Signatures**
✅ Verify all webhooks with `Stripe-Signature` header  
✅ Reject unverified requests

### 3. **HTTPS Only**
✅ Force HTTPS in production (already configured)  
✅ Stripe requires HTTPS for webhooks

### 4. **Secure API Keys**
✅ Use User Secrets in development  
✅ Use Environment Variables in production  
✅ Never commit keys to source control

### 5. **Idempotency**
✅ Use `PaymentId` as idempotency key  
✅ Prevent duplicate charges

---

## 📈 TESTING STRIPE (WHEN CONFIGURED)

### Test Card Numbers

```
✅ Success: 4242 4242 4242 4242
❌ Decline: 4000 0000 0000 0002
🔐 3D Secure: 4000 0027 6000 3184
```

**Expiry:** Any future date  
**CVC:** Any 3 digits  
**ZIP:** Any 5 digits

### Test Webhooks Locally

```bash
# Install Stripe CLI
brew install stripe/stripe-cli/stripe  # macOS
scoop install stripe                   # Windows

# Login to Stripe
stripe login

# Forward webhooks to local server
stripe listen --forward-to https://localhost:7186/api/webhooks/stripe

# Trigger test events
stripe trigger payment_intent.succeeded
```

---

## 🎯 WHAT HAPPENS AFTER CHECKOUT?

### Current Behavior (Cash on Delivery)

1. ✅ Customer submits order
2. ✅ Order created with status `Confirmed`
3. ✅ Payment created with status `Pending`
4. ✅ Cart cleared
5. ✅ Customer redirected to `/Customer/Orders` (Order History)
6. ✅ Order shows in "Active Orders" section
7. ✅ Customer sees: "Payment: Cash on Delivery (Pending)"

### Future Behavior (Stripe)

1. ✅ Customer submits order with card details
2. ✅ Stripe PaymentIntent created
3. ✅ Frontend: 3D Secure authentication (if required)
4. ✅ Stripe webhook: `payment_intent.succeeded`
5. ✅ Order created with status `Confirmed`
6. ✅ Payment created with status `Completed`
7. ✅ Cart cleared
8. ✅ Customer redirected to `/Customer/Orders`
9. ✅ Order shows in "Active Orders" section
10. ✅ Customer sees: "Payment: Card •••• 4242 (Paid)"

---

## 🧪 VERIFICATION CHECKLIST

### Before Enabling Stripe

- [ ] Install `Stripe.net` package
- [ ] Configure API keys (User Secrets or Environment Variables)
- [ ] Run database migration
- [ ] Uncomment Stripe code in `PaymentProcessorService.cs`
- [ ] Create `WebhooksController.cs`
- [ ] Test with Stripe test mode
- [ ] Set up webhook endpoint in Stripe Dashboard
- [ ] Test 3D Secure flow
- [ ] Test refund flow
- [ ] Verify payment history displays correctly

### Production Checklist

- [ ] Switch to Stripe Live keys
- [ ] Update webhook URL to production domain
- [ ] Enable HTTPS enforcement
- [ ] Set `Payment:Stripe:Enabled` to `true`
- [ ] Test with real card (small amount)
- [ ] Monitor Stripe Dashboard for events
- [ ] Set up email notifications for failed payments
- [ ] Configure automatic receipt emails

---

## 📚 FILES CREATED/MODIFIED

### Created:
1. ✅ `Services/Payment/IPaymentProcessorService.cs` - Payment service interface
2. ✅ `Services/Payment/PaymentProcessorService.cs` - Payment implementation
3. ✅ `appsettings.Payment.json` - Stripe configuration template
4. ✅ `Migrations/TEMPLATE_AddStripeFieldsToPayment.cs` - Database migration template

### Modified:
1. ✅ `Models/Payment.cs` - Added Stripe fields
2. ✅ `Program.cs` - Registered PaymentProcessorService

### To Create (When Stripe is Ready):
1. ⚠️ `Controllers/WebhooksController.cs` - Stripe webhook handler
2. ⚠️ `Controllers/PaymentController.cs` - Payment API endpoints
3. ⚠️ Frontend: Update checkout page with Stripe.js

---

## 💡 TIPS & RECOMMENDATIONS

### 1. **Start with Test Mode**
- Use Stripe test keys first
- Test all scenarios (success, decline, 3D Secure)
- Only switch to live mode when confident

### 2. **Monitor Everything**
- Set up Stripe Dashboard alerts
- Log all payment attempts
- Track failed payments for retry

### 3. **Handle Edge Cases**
- Network timeouts
- Webhook delays
- Duplicate webhook events
- Partial refunds

### 4. **User Experience**
- Show clear payment status
- Send email confirmations
- Allow payment retry if failed
- Display card last 4 digits in order history

### 5. **Compliance**
- PCI DSS: Stripe handles compliance
- GDPR: Don't store unnecessary data
- Local regulations: Check Saudi payment laws

---

## 🚀 QUICK START (TL;DR)

**When you're ready to enable Stripe:**

```bash
# 1. Install package
dotnet add package Stripe.net

# 2. Set up secrets
dotnet user-secrets set "Payment:Stripe:SecretKey" "sk_test_..."
dotnet user-secrets set "Payment:Stripe:PublishableKey" "pk_test_..."
dotnet user-secrets set "Payment:Stripe:Enabled" "true"

# 3. Run migration
dotnet ef migrations add AddStripeFieldsToPayment
dotnet ef database update

# 4. Uncomment Stripe code in PaymentProcessorService.cs

# 5. Create WebhooksController.cs

# 6. Test!
```

---

## ✅ CURRENT STATUS SUMMARY

**What's Working Now:**
- ✅ Cash on Delivery payments
- ✅ Order creation after checkout
- ✅ Redirect to Order History
- ✅ Payment tracking (Pending status)
- ✅ Order shows in "Active Orders"

**What's Ready for Stripe:**
- ✅ Database schema (after migration)
- ✅ Payment processor service
- ✅ Configuration system
- ✅ Error handling & logging
- ✅ Transaction management
- ✅ Refund support

**What's Needed for Stripe:**
- ⚠️ Stripe.net package installation
- ⚠️ API key configuration
- ⚠️ Code uncommented in service
- ⚠️ Webhook controller created
- ⚠️ Frontend Stripe.js integration
- ⚠️ Stripe Dashboard configuration

---

**Platform:** .NET 9.0 with ASP.NET Core  
**Payment Provider:** Stripe (when configured)  
**Current Mode:** Cash Only  
**Status:** ✅ **READY FOR STRIPE INTEGRATION**

**Last Updated:** Automated Generation
