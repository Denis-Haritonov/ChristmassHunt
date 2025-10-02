namespace Common.Repository;

public static class SpecificationEvaluator
{
    public static IQueryable<T> GetQuery<T>(
        IQueryable<T> inputQuery, 
        ISpecification<T> spec)
    {
        var query = inputQuery;

        if (spec.Criteria != null)
            query = query.Where(spec.Criteria);

        if (spec.OrderBy != null)
            query = query.OrderBy(spec.OrderBy);

        if (spec.OrderByDescending != null)
            query = query.OrderByDescending(spec.OrderByDescending);

        if (spec.Skip.HasValue)
            query = query.Skip(spec.Skip.Value);

        if (spec.Take.HasValue)
            query = query.Take(spec.Take.Value);

        return query;
    }
}