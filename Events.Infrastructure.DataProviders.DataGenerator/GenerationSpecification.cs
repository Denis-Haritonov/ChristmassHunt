using System.Linq.Expressions;
using Common.Repository;
using Events.Models;

namespace DataGenerator;

public sealed class GenerateNearPastEventsSpec : ISpecification<Event>
{
    public GenerateNearPastEventsSpec(
        ISpecification<Event> baseSpecification,
        double radiusMeters = 10000,
        int count = 500,
        int maxDaysAgo = 30,
        double probabilityMomentary = 0.7,
        int? seed = null)
    {
        if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
        if (radiusMeters <= 0) throw new ArgumentOutOfRangeException(nameof(radiusMeters));
        if (maxDaysAgo <= 0) throw new ArgumentOutOfRangeException(nameof(maxDaysAgo));
        if (probabilityMomentary is < 0 or > 1) throw new ArgumentOutOfRangeException(nameof(probabilityMomentary));
        
        Count = count;
        RadiusMeters = radiusMeters;
        MaxDaysAgo = maxDaysAgo;
        ProbabilityMomentary = probabilityMomentary;
        Seed = seed;

        // ISpecification parts (optional filter/sort/trim after generation)
        Criteria = baseSpecification.Criteria;
        OrderBy = baseSpecification.OrderBy;
        OrderByDescending = baseSpecification.OrderByDescending;
        Skip = baseSpecification.Skip;
        Take = baseSpecification.Take;
    }

    // Generation parameters
    public double RadiusMeters { get; }
    public int MaxDaysAgo { get; }
    public double ProbabilityMomentary { get; }
    public int? Seed { get; }

    public int Count { get; }
    
    // ISpecification<Event>
    public Expression<Func<Event, bool>>? Criteria { get; }
    public Expression<Func<Event, object>>? OrderBy { get; }
    public Expression<Func<Event, object>>? OrderByDescending { get; }
    public int? Skip { get; }
    public int? Take { get; }
}