namespace TafsilkPlatform.Web.Models
{
    /// <summary>
    /// Order lifecycle statuses aligned with Tafsilk customer journey workflow
    /// </summary>
    public enum OrderStatus
    {
        /// <summary>
        /// Customer submitted order, awaiting tailor quote/confirmation
        /// </summary>
        QuotePending = 0,
        
        /// <summary>
        /// Tailor confirmed order and provided quote
        /// </summary>
        Confirmed = 1,
   
        /// <summary>
        /// Order is being worked on by the tailor
        /// </summary>
        InProgress = 2,
        
        /// <summary>
        /// Order completed and ready for customer pickup or delivery
        /// </summary>
        ReadyForPickup = 3,
        
        /// <summary>
        /// Customer received and accepted the order
        /// </summary>
        Completed = 4,
     
        /// <summary>
        /// Order cancelled by customer or tailor
        /// </summary>
        Cancelled = 5,
 
        // Legacy statuses (maintain for backward compatibility)
        [Obsolete("Use QuotePending instead")]
        Pending = 0,
        
        [Obsolete("Use InProgress instead")]
        Processing = 2,
        
        [Obsolete("Use ReadyForPickup instead")]
        Shipped = 3,
   
        [Obsolete("Use Completed instead")]
        Delivered = 4
    }
}
