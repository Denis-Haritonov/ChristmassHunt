using System.Linq.Expressions;
using Common.Repository;
using Events.Models;

namespace Events.Specifications;

public class FiftyNewestEventsSpecification : ISpecification<Event> 
{
    public FiftyNewestEventsSpecification()
    {
        Criteria = null;
        OrderBy = null;
        OrderByDescending = @event => @event.EventTime.Start;
        Take = 500;
        Skip = 0;
    }
    
    public Expression<Func<Event, bool>>? Criteria { get; }
    public Expression<Func<Event, object>>? OrderBy { get; }
    public Expression<Func<Event, object>>? OrderByDescending { get; }
    public int? Take { get; }
    public int? Skip { get; }
}