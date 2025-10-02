using Common.Repository;
using Events.Models;

namespace Events.Specifications;

public static class EventSpecs
{
    public static readonly ISpecification<Event> FiftyNewestEventsSpecification =
        new FiftyNewestEventsSpecification();
}