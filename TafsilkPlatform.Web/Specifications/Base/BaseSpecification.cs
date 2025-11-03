using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace TafsilkPlatform.Web.Specifications.Base;

/// <summary>
/// Specification pattern interface for building complex queries
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public interface ISpecification<T>
{
    /// <summary>
 /// Where clause criteria
    /// </summary>
    Expression<Func<T, bool>>? Criteria { get; }

    /// <summary>
    /// Include expressions for eager loading
    /// </summary>
    List<Expression<Func<T, object>>> Includes { get; }

    /// <summary>
    /// Include strings for ThenInclude scenarios
    /// </summary>
    List<string> IncludeStrings { get; }

    /// <summary>
    /// Order by expression
    /// </summary>
    Expression<Func<T, object>>? OrderBy { get; }

    /// <summary>
    /// Order by descending expression
    /// </summary>
    Expression<Func<T, object>>? OrderByDescending { get; }

    /// <summary>
    /// Number of records to take
    /// </summary>
    int Take { get; }

    /// <summary>
/// Number of records to skip
    /// </summary>
    int Skip { get; }

    /// <summary>
 /// Is paging enabled
 /// </summary>
    bool IsPagingEnabled { get; }

    /// <summary>
  /// Is distinct query
    /// </summary>
    bool IsDistinct { get; }

    /// <summary>
    /// Group by expression
    /// </summary>
    Expression<Func<T, object>>? GroupBy { get; }
}

/// <summary>
/// Base implementation of specification pattern
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public abstract class BaseSpecification<T> : ISpecification<T>
{
    public Expression<Func<T, bool>>? Criteria { get; private set; }
    public List<Expression<Func<T, object>>> Includes { get; } = new();
    public List<string> IncludeStrings { get; } = new();
    public Expression<Func<T, object>>? OrderBy { get; private set; }
public Expression<Func<T, object>>? OrderByDescending { get; private set; }
    public int Take { get; private set; }
    public int Skip { get; private set; }
    public bool IsPagingEnabled { get; private set; }
  public bool IsDistinct { get; private set; }
    public Expression<Func<T, object>>? GroupBy { get; private set; }

    /// <summary>
    /// Constructor with criteria
    /// </summary>
    protected BaseSpecification(Expression<Func<T, bool>>? criteria = null)
    {
        Criteria = criteria;
    }

    /// <summary>
    /// Add include expression for eager loading
    /// </summary>
    protected virtual void AddInclude(Expression<Func<T, object>> includeExpression)
    {
        Includes.Add(includeExpression);
    }

    /// <summary>
    /// Add include string for complex navigation properties
    /// </summary>
    protected virtual void AddInclude(string includeString)
    {
      IncludeStrings.Add(includeString);
    }

    /// <summary>
    /// Apply order by ascending
    /// </summary>
    protected virtual void ApplyOrderBy(Expression<Func<T, object>> orderByExpression)
    {
        OrderBy = orderByExpression;
    }

 /// <summary>
    /// Apply order by descending
    /// </summary>
    protected virtual void ApplyOrderByDescending(Expression<Func<T, object>> orderByDescExpression)
    {
   OrderByDescending = orderByDescExpression;
    }

    /// <summary>
    /// Apply paging
    /// </summary>
    protected virtual void ApplyPaging(int skip, int take)
    {
    Skip = skip;
      Take = take;
        IsPagingEnabled = true;
    }

    /// <summary>
  /// Apply distinct
    /// </summary>
    protected virtual void ApplyDistinct()
    {
        IsDistinct = true;
    }

    /// <summary>
    /// Apply group by
    /// </summary>
    protected virtual void ApplyGroupBy(Expression<Func<T, object>> groupByExpression)
    {
   GroupBy = groupByExpression;
    }
}

/// <summary>
/// Specification evaluator to apply specifications to IQueryable
/// </summary>
public static class SpecificationEvaluator
{
    /// <summary>
    /// Apply specification to query
    /// </summary>
    public static IQueryable<T> GetQuery<T>(
        IQueryable<T> inputQuery,
        ISpecification<T> specification) where T : class
    {
        var query = inputQuery;

 // Apply criteria (Where clause)
 if (specification.Criteria != null)
   {
      query = query.Where(specification.Criteria);
        }

    // Apply includes
   query = specification.Includes.Aggregate(
            query,
       (current, include) => current.Include(include));

        // Apply include strings
        query = specification.IncludeStrings.Aggregate(
            query,
(current, include) => current.Include(include));

 // Apply ordering
        if (specification.OrderBy != null)
        {
query = query.OrderBy(specification.OrderBy);
        }
 else if (specification.OrderByDescending != null)
   {
          query = query.OrderByDescending(specification.OrderByDescending);
        }

        // Apply group by
        if (specification.GroupBy != null)
        {
     query = query.GroupBy(specification.GroupBy).SelectMany(x => x);
   }

        // Apply distinct
        if (specification.IsDistinct)
     {
       query = query.Distinct();
        }

        // Apply paging
     if (specification.IsPagingEnabled)
        {
   query = query.Skip(specification.Skip).Take(specification.Take);
  }

  return query;
    }
}
