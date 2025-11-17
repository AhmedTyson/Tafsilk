namespace TafsilkPlatform.Web.Models
{
    /// <summary>
    /// Order lifecycle statuses aligned with Tafsilk customer journey workflow
    /// </summary>
    public enum OrderStatus
    {
        /// <summary>
        /// Customer submitted order, awaiting tailor quote/confirmation
        /// For custom tailor orders: awaiting quote
        /// For store orders: order placed, payment pending
        /// </summary>
        Pending = 0,
        
        /// <summary>
        /// Tailor confirmed order and provided quote (custom orders)
        /// OR Order payment confirmed (store orders)
        /// </summary>
        Confirmed = 1,
   
        /// <summary>
        /// Order is being worked on by the tailor
        /// Applies to both custom and store orders
        /// </summary>
        Processing = 2,
    
        /// <summary>
        /// Order completed and ready for customer pickup or delivery
        /// </summary>
        ReadyForPickup = 3,
        
        /// <summary>
        /// Customer received and accepted the order
        /// Final successful state
        /// </summary>
        Delivered = 4,
     
        /// <summary>
        /// Order cancelled by customer or tailor
        /// </summary>
        Cancelled = 5,
   
        /// <summary>
        /// Order is being shipped/in transit (for delivery orders)
        /// </summary>
        Shipped = 6
    }
}
