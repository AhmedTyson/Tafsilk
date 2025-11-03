using TafsilkPlatform.Web.Models;
using TafsilkPlatform.Web.Specifications.Base;

namespace TafsilkPlatform.Web.Specifications.OrderSpecifications;

/// <summary>
/// Specification for getting orders by tailor ID
/// </summary>
public class OrdersByTailorSpecification : BaseSpecification<Order>
{
    public OrdersByTailorSpecification(Guid tailorId, bool includeRelated = true)
    : base(o => o.TailorId == tailorId)
    {
        if (includeRelated)
        {
     AddInclude(o => o.Customer);
            AddInclude("Customer.User");
            AddInclude(o => o.Items);
   }

     ApplyOrderByDescending(o => o.CreatedAt);
    }
}

/// <summary>
/// Specification for getting recent orders
/// </summary>
public class RecentOrdersSpecification : BaseSpecification<Order>
{
    public RecentOrdersSpecification(
   Guid tailorId, 
     int take = 5,
        bool includeRelated = true)
        : base(o => o.TailorId == tailorId)
    {
if (includeRelated)
   {
 AddInclude(o => o.Customer);
      AddInclude("Customer.User");
        }

      ApplyOrderByDescending(o => o.CreatedAt);
   ApplyPaging(0, take);
    }
}

/// <summary>
/// Specification for pending orders
/// </summary>
public class PendingOrdersSpecification : BaseSpecification<Order>
{
  public PendingOrdersSpecification(Guid tailorId)
        : base(o => o.TailorId == tailorId && o.Status == OrderStatus.Pending)
    {
    AddInclude(o => o.Customer);
     AddInclude("Customer.User");
        ApplyOrderBy(o => o.CreatedAt);
    }
}

/// <summary>
/// Specification for active orders (Processing or Shipped)
/// </summary>
public class ActiveOrdersSpecification : BaseSpecification<Order>
{
    public ActiveOrdersSpecification(Guid tailorId)
     : base(o => o.TailorId == tailorId && 
     (o.Status == OrderStatus.Processing || o.Status == OrderStatus.Shipped))
  {
  ApplyOrderByDescending(o => o.CreatedAt);  // ✅ Fixed: Use CreatedAt instead of UpdatedAt
    }
}

/// <summary>
/// Specification for completed orders
/// </summary>
public class CompletedOrdersSpecification : BaseSpecification<Order>
{
    public CompletedOrdersSpecification(Guid tailorId)
        : base(o => o.TailorId == tailorId && o.Status == OrderStatus.Delivered)
    {
ApplyOrderByDescending(o => o.CreatedAt);  // ✅ Fixed: Use CreatedAt instead of UpdatedAt
    }
}

/// <summary>
/// Specification for orders in date range
/// </summary>
public class OrdersByDateRangeSpecification : BaseSpecification<Order>
{
    public OrdersByDateRangeSpecification(
        Guid tailorId,
        DateTimeOffset startDate,
        DateTimeOffset endDate)
        : base(o => o.TailorId == tailorId && 
     o.CreatedAt >= startDate && 
      o.CreatedAt <= endDate)
    {
ApplyOrderByDescending(o => o.CreatedAt);
    }
}

/// <summary>
/// Specification for orders with status filter
/// </summary>
public class OrdersWithStatusSpecification : BaseSpecification<Order>
{
    public OrdersWithStatusSpecification(
        Guid tailorId,
        OrderStatus status,
   bool includeRelated = false)
      : base(o => o.TailorId == tailorId && o.Status == status)
    {
        if (includeRelated)
        {
AddInclude(o => o.Customer);
            AddInclude("Customer.User");
   AddInclude(o => o.Items);
     }

        ApplyOrderByDescending(o => o.CreatedAt);
    }
}

/// <summary>
/// Specification for orders with pagination
/// </summary>
public class OrdersPaginatedSpecification : BaseSpecification<Order>
{
    public OrdersPaginatedSpecification(
     Guid tailorId,
        int pageNumber,
        int pageSize,
   OrderStatus? status = null)
   : base(o => o.TailorId == tailorId && 
   (status == null || o.Status == status.Value))
    {
        AddInclude(o => o.Customer);
 AddInclude("Customer.User");
 
        ApplyOrderByDescending(o => o.CreatedAt);
        ApplyPaging((pageNumber - 1) * pageSize, pageSize);
  }
}

/// <summary>
/// Specification for customer orders
/// </summary>
public class OrdersByCustomerSpecification : BaseSpecification<Order>
{
  public OrdersByCustomerSpecification(Guid customerId, bool includeRelated = true)
  : base(o => o.CustomerId == customerId)
 {
if (includeRelated)
        {
   AddInclude(o => o.Tailor);
            AddInclude("Tailor.User");
      AddInclude(o => o.Items);
        }

      ApplyOrderByDescending(o => o.CreatedAt);
    }
}

/// <summary>
/// Specification for orders needing attention (pending or processing for too long)
/// </summary>
public class OrdersNeedingAttentionSpecification : BaseSpecification<Order>
{
    public OrdersNeedingAttentionSpecification(Guid tailorId, int daysThreshold = 3)
        : base(o => o.TailorId == tailorId && 
     (o.Status == OrderStatus.Pending || o.Status == OrderStatus.Processing) &&
         o.CreatedAt < DateTimeOffset.UtcNow.AddDays(-daysThreshold))
    {
   AddInclude(o => o.Customer);
AddInclude("Customer.User");
      ApplyOrderBy(o => o.CreatedAt);
    }
}

/// <summary>
/// Specification for orders with search term
/// </summary>
public class OrdersSearchSpecification : BaseSpecification<Order>
{
    public OrdersSearchSpecification(
        Guid tailorId,
        string searchTerm)
: base(o => o.TailorId == tailorId && 
  (o.OrderType.Contains(searchTerm) || 
        o.Customer.User.Email.Contains(searchTerm) ||
      o.Customer.FullName.Contains(searchTerm)))
    {
AddInclude(o => o.Customer);
  AddInclude("Customer.User");
        ApplyOrderByDescending(o => o.CreatedAt);
    }
}
