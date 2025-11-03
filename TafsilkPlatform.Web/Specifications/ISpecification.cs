using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace TafsilkPlatform.Web.Specifications;

/// <summary>
/// Advanced specification interface for complex queries
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public interface ISpecification<T>
{
    Expression<Func<T, bool>>? Criteria { get; }
    List<Expression<Func<T, object>>> Includes { get; }
    List<string> IncludeStrings { get; }
Expression<Func<T, object>>? OrderBy { get; }
    Expression<Func<T, object>>? OrderByDescending { get; }
    Expression<Func<T, object>>? GroupBy { get; }
    
    int Take { get; }
    int Skip { get; }
    bool IsPagingEnabled { get; }
}

/// <summary>
/// Base specification implementation
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public abstract class Specification<T> : ISpecification<T>
{
    protected Specification()
    {
    }

    protected Specification(Expression<Func<T, bool>> criteria)
    {
        Criteria = criteria;
    }

    public Expression<Func<T, bool>>? Criteria { get; private set; }
 public List<Expression<Func<T, object>>> Includes { get; } = new();
    public List<string> IncludeStrings { get; } = new();
    public Expression<Func<T, object>>? OrderBy { get; private set; }
    public Expression<Func<T, object>>? OrderByDescending { get; private set; }
    public Expression<Func<T, object>>? GroupBy { get; private set; }

    public int Take { get; private set; }
    public int Skip { get; private set; }
    public bool IsPagingEnabled { get; private set; }

    protected virtual void AddInclude(Expression<Func<T, object>> includeExpression)
    {
        Includes.Add(includeExpression);
    }

    protected virtual void AddInclude(string includeString)
    {
IncludeStrings.Add(includeString);
 }

    protected virtual void ApplyPaging(int skip, int take)
    {
     Skip = skip;
        Take = take;
        IsPagingEnabled = true;
    }

    protected virtual void ApplyOrderBy(Expression<Func<T, object>> orderByExpression)
    {
   OrderBy = orderByExpression;
    }

    protected virtual void ApplyOrderByDescending(Expression<Func<T, object>> orderByDescExpression)
{
        OrderByDescending = orderByDescExpression;
    }

    protected virtual void ApplyGroupBy(Expression<Func<T, object>> groupByExpression)
    {
    GroupBy = groupByExpression;
    }

    protected virtual void ApplyCriteria(Expression<Func<T, bool>> criteria)
{
Criteria = criteria;
    }
}

/// <summary>
/// Extension to apply specifications to IQueryable
/// </summary>
public static class SpecificationEvaluator
{
    public static IQueryable<T> GetQuery<T>(IQueryable<T> inputQuery, ISpecification<T> specification) where T : class
    {
        var query = inputQuery;

        // Apply criteria
      if (specification.Criteria != null)
        {
            query = query.Where(specification.Criteria);
     }

        // Include navigation properties
     query = specification.Includes.Aggregate(query, (current, include) => current.Include(include));

        // Include string-based includes
    query = specification.IncludeStrings.Aggregate(query, (current, include) => current.Include(include));

   // Apply ordering
        if (specification.OrderBy != null)
        {
    query = query.OrderBy(specification.OrderBy);
    }
    else if (specification.OrderByDescending != null)
      {
          query = query.OrderByDescending(specification.OrderByDescending);
        }

// Apply grouping
        if (specification.GroupBy != null)
        {
         query = query.GroupBy(specification.GroupBy).SelectMany(x => x);
        }

        // Apply paging
        if (specification.IsPagingEnabled)
        {
            query = query.Skip(specification.Skip).Take(specification.Take);
        }

        return query;
    }
}
