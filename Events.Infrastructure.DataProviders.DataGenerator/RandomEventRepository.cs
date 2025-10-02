using Common.Repository;
using Events;
using Events.Models;

namespace DataGenerator;

public sealed class RandomEventRepository : IEventsRepository
{
    private Address _address;

    /// <param name="addressFactory">
    /// Adapter to build your Address from (lat, lon, description).
    /// Example for string lat/lon:
    /// (lat, lon, desc) => new AddressBuilder().WithLatitude(lat.ToString("F6", CultureInfo.InvariantCulture))...
    /// </param>
    public RandomEventRepository(string description, double latttitude, double longitude)
    {
        _address = new Address(description, latttitude, longitude);
    }

    public Task<IReadOnlyList<Event>> ListAsync(ISpecification<Event>? spec, CancellationToken ct = default)
    {
        GenerateNearPastEventsSpec s = new GenerateNearPastEventsSpec(spec);
        
        var rng = s.Seed.HasValue ? new Random(s.Seed.Value) : new Random();
        var now = DateTime.UtcNow;
        var events = new List<Event>(s.Count);

        for (int i = 0; i < s.Count; i++)
        {
            ct.ThrowIfCancellationRequested();

            var (lat, lon) = RandomOffset(_address.Latitude, _address.Longitude, s.RadiusMeters, rng);
            var start = RandomPastInstant(now, s.MaxDaysAgo, rng);

            EventTime time;
            if (rng.NextDouble() < s.ProbabilityMomentary)
            {
                time = new EventTime { Start = start, End = null };
            }
            else
            {
                var maxEnd = now.AddMinutes(-1);
                var maxSpan = maxEnd - start;
                if (maxSpan <= TimeSpan.Zero)
                {
                    time = new EventTime { Start = start, End = null };
                }
                else
                {
                    var span = TimeSpan.FromTicks(
                        (long)(rng.NextDouble() * Math.Min(maxSpan.Ticks, TimeSpan.FromHours(24).Ticks)));
                    time = new EventTime { Start = start, End = start + span };
                }
            }

            var title = RandomTitle(rng);
            var desc = $"{title}";
            var addr = _address;

            events.Add(new Event
            {
                Id = i + 1,
                Title = title,
                Description = desc,
                PhotoPath = string.Empty,
                EventTime = time,
                Address = new Address("", lat,lon)
            });
        }

        // Apply optional filtering/sorting/skip/take from the spec to the generated set
        IQueryable<Event> query = events.AsQueryable();
        if (s.Criteria != null) query = query.Where(s.Criteria);
        if (s.OrderBy != null) query = query.OrderBy(s.OrderBy);
        else if (s.OrderByDescending != null) query = query.OrderByDescending(s.OrderByDescending);
        if (s.Skip.HasValue) query = query.Skip(s.Skip.Value);
        if (s.Take.HasValue) query = query.Take(s.Take.Value);

        return Task.FromResult<IReadOnlyList<Event>>(query.ToList());
    }

    // --- All other IRepository members are intentionally not supported ---
    public Task<Event?> GetByIdAsync(int id, CancellationToken ct = default)
        => throw new NotSupportedException("RandomEventRepository only supports ListAsync.");

    public Task<bool> ExistsAsync(int id, CancellationToken ct = default)
        => throw new NotSupportedException("RandomEventRepository only supports ListAsync.");

    public Task<int> CountAsync(ISpecification<Event>? spec, CancellationToken ct = default)
        => throw new NotSupportedException("RandomEventRepository only supports ListAsync.");

    public Task<Event> AddAsync(Event entity, CancellationToken ct = default)
        => throw new NotSupportedException("RandomEventRepository only supports ListAsync.");

    public Task AddRangeAsync(IEnumerable<Event> entities, CancellationToken ct = default)
        => throw new NotSupportedException("RandomEventRepository only supports ListAsync.");

    public Task UpdateAsync(Event entity, CancellationToken ct = default)
        => throw new NotSupportedException("RandomEventRepository only supports ListAsync.");

    public Task UpdateRangeAsync(IEnumerable<Event> entities, CancellationToken ct = default)
        => throw new NotSupportedException("RandomEventRepository only supports ListAsync.");

    public Task DeleteAsync(Event entity, CancellationToken ct = default)
        => throw new NotSupportedException("RandomEventRepository only supports ListAsync.");

    public Task DeleteRangeAsync(IEnumerable<Event> entities, CancellationToken ct = default)
        => throw new NotSupportedException("RandomEventRepository only supports ListAsync.");

    // --- helpers ---
    private static (double lat, double lon) RandomOffset(double lat0, double lon0, double radiusMeters, Random rng)
    {
        // Uniform sample in a circle
        var u = rng.NextDouble();
        var r = radiusMeters * Math.Sqrt(u);
        var theta = rng.NextDouble() * 2 * Math.PI;

        const double metersPerDegLat = 111_320.0;
        var metersPerDegLon = Math.Cos(lat0 * Math.PI / 180.0) * metersPerDegLat;

        var dLat = (r * Math.Sin(theta)) / metersPerDegLat;
        var dLon = (r * Math.Cos(theta)) / metersPerDegLon;

        return (lat0 + dLat, NormalizeLongitude(lon0 + dLon));
    }

    private static double NormalizeLongitude(double lon)
    {
        lon = (lon + 180.0) % 360.0;
        if (lon < 0) lon += 360.0;
        return lon - 180.0;
    }

    private static DateTime RandomPastInstant(DateTime nowUtc, int maxDaysAgo, Random rng)
    {
        var ticksBack = (long)(rng.NextDouble() * TimeSpan.FromDays(maxDaysAgo).Ticks);
        return nowUtc - new TimeSpan(ticksBack);
    }

    private static string RandomTitle(Random rng)
    {
        string[] nouns = { "santa", "deer", "dwarf", "tree"};
        return $"{nouns[rng.Next(nouns.Length)]}";
    }
}