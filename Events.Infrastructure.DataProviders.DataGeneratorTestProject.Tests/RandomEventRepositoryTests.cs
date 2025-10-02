namespace DataGenerator.Tests;

using System;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;
using static System.Math;

public class RandomEventRepositoryTests
{
    // ---- Address factory adaptable to your Address shape ----
    private static readonly Func<double, double, string?, Address> AddressFactory = MakeAddress;

    private static Address MakeAddress(double lat, double lon, string? description)
    {
        var t = typeof(Address);
        var inv = CultureInfo.InvariantCulture;
        var slat = lat.ToString("F6", inv);
        var slon = lon.ToString("F6", inv);

        // Try a few common constructor shapes
        var ctor =
            t.GetConstructor(new[] { typeof(int), typeof(string), typeof(string), typeof(string) }) ??
            t.GetConstructor(new[] { typeof(string), typeof(string), typeof(string) }) ??
            t.GetConstructor(new[] { typeof(double), typeof(double), typeof(string) }) ??
            t.GetConstructor(new[] { typeof(double), typeof(double) });

        if (ctor != null)
        {
            var ps = ctor.GetParameters().Select(p => p.ParameterType).ToArray();
            object?[] args = ps switch
            {
                var a when a.Length == 4 && a[0] == typeof(int) => new object?[] { 0, description, slat, slon },
                var a when a.Length == 3 && a[0] == typeof(string) => new object?[] { description, slat, slon },
                var a when a.Length == 3 && a[0] == typeof(double) => new object?[] { lat, lon, description },
                var a when a.Length == 2 && a[0] == typeof(double) => new object?[] { lat, lon },
                _ => Array.Empty<object?>()
            };
            return (Address)ctor.Invoke(args);
        }

        // Fallback: default ctor + settable properties
        var def = t.GetConstructor(Type.EmptyTypes)
                  ?? throw new InvalidOperationException("Could not construct Address. Adjust MakeAddress to your Address type.");
        var inst = def.Invoke(null);

        var latProp = t.GetProperty("Latitude");
        var lonProp = t.GetProperty("Longitude");
        var descProp = t.GetProperty("Description");

        if (latProp != null)
        {
            if (latProp.PropertyType == typeof(string)) latProp.SetValue(inst, slat);
            else if (latProp.PropertyType == typeof(double)) latProp.SetValue(inst, lat);
            else if (latProp.PropertyType == typeof(decimal)) latProp.SetValue(inst, (decimal)lat);
        }
        if (lonProp != null)
        {
            if (lonProp.PropertyType == typeof(string)) lonProp.SetValue(inst, slon);
            else if (lonProp.PropertyType == typeof(double)) lonProp.SetValue(inst, lon);
            else if (lonProp.PropertyType == typeof(decimal)) lonProp.SetValue(inst, (decimal)lon);
        }
        if (descProp != null && descProp.CanWrite) descProp.SetValue(inst, description);

        return (Address)inst;
    }

    // ---- Helpers ----
    private static double Deg2Rad(double d) => d * PI / 180.0;
    private static double HaversineMeters(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371000.0;
        var dLat = Deg2Rad(lat2 - lat1);
        var dLon = Deg2Rad(lon2 - lon1);
        var a = Sin(dLat / 2) * Sin(dLat / 2) +
                Cos(Deg2Rad(lat1)) * Cos(Deg2Rad(lat2)) *
                Sin(dLon / 2) * Sin(dLon / 2);
        var c = 2 * Atan2(Sqrt(a), Sqrt(1 - a));
        return R * c;
    }
    private static double ParseLat(Address a)
    {
        var p = a.GetType().GetProperty("Latitude") 
                ?? throw new InvalidOperationException("Address.Latitude not found");
        return Convert.ToDouble(p.GetValue(a), CultureInfo.InvariantCulture);
    }
    private static double ParseLon(Address a)
    {
        var p = a.GetType().GetProperty("Longitude") 
                ?? throw new InvalidOperationException("Address.Longitude not found");
        return Convert.ToDouble(p.GetValue(a), CultureInfo.InvariantCulture);
    }

    // ---------- TESTS ----------

    [Fact]
    public async Task ListAsync_GeneratesRequestedCount()
    {
        var repo = new RandomEventRepository(AddressFactory);
        var spec = new GenerateNearPastEventsSpec(51.1079, 17.0385, count: 25, radiusMeters: 1000, seed: 42);
        var list = await repo.ListAsync(spec);
        Assert.Equal(25, list.Count);
    }

    [Fact]
    public async Task ListAsync_CoordinatesWithinRadius()
    {
        double lat = 51.1079, lon = 17.0385, radius = 500;
        var repo = new RandomEventRepository(AddressFactory);
        var spec = new GenerateNearPastEventsSpec(lat, lon, count: 100, radiusMeters: radius, seed: 123);

        var list = await repo.ListAsync(spec);

        Assert.All(list, e =>
        {
            var d = HaversineMeters(lat, lon, ParseLat(e.Address), ParseLon(e.Address));
            Assert.True(d <= radius + 1.0, $"Distance {d:N2}m exceeds radius {radius}m");
        });
    }

    [Fact]
    public async Task ListAsync_TimesAreInPast_AndValidDuration()
    {
        var now = DateTime.UtcNow;
        var repo = new RandomEventRepository(AddressFactory);
        var spec = new GenerateNearPastEventsSpec(51.1, 17.0, count: 50, radiusMeters: 800, seed: 7);

        var list = await repo.ListAsync(spec);

        Assert.All(list, e =>
        {
            Assert.True(e.EventTime.Start <= now);
            if (e.EventTime.End is DateTime end)
            {
                Assert.True(end <= now);
                Assert.True(end >= e.EventTime.Start);
                Assert.True(end - e.EventTime.Start <= TimeSpan.FromHours(24) + TimeSpan.FromSeconds(1));
            }
        });
    }

    [Fact]
    public async Task ListAsync_AllMomentary_WhenProbabilityIsOne()
    {
        var repo = new RandomEventRepository(AddressFactory);
        var spec = new GenerateNearPastEventsSpec(51, 17, count: 20, radiusMeters: 500, probabilityMomentary: 1.0, seed: 11);

        var list = await repo.ListAsync(spec);

        Assert.All(list, e => Assert.Null(e.EventTime.End));
    }

    [Fact]
    public async Task ListAsync_AllRanges_WhenProbabilityIsZero()
    {
        var repo = new RandomEventRepository(AddressFactory);
        var spec = new GenerateNearPastEventsSpec(51, 17, count: 20, radiusMeters: 500, probabilityMomentary: 0.0, seed: 22);

        var list = await repo.ListAsync(spec);

        Assert.All(list, e =>
        {
            Assert.NotNull(e.EventTime.End);
            Assert.True(e.EventTime.End!.Value > e.EventTime.Start);
            Assert.True(e.EventTime.End!.Value - e.EventTime.Start <= TimeSpan.FromHours(24) + TimeSpan.FromSeconds(1));
        });
    }

    [Fact]
    public async Task ListAsync_AppliesOrderByAndTake()
    {
        var repo = new RandomEventRepository(AddressFactory);
        // After generation, order ascending by Start and take 10
        var spec = new GenerateNearPastEventsSpec(
            51.1079, 17.0385, count: 100, radiusMeters: 1000, seed: 33,
            orderBy: e => e.EventTime.Start, take: 10);

        var list = await repo.ListAsync(spec);

        Assert.Equal(10, list.Count);
        var starts = list.Select(e => e.EventTime.Start).ToArray();
        var sorted = starts.OrderBy(x => x).ToArray();
        Assert.Equal(sorted, starts);
    }

    [Fact]
    public async Task ListAsync_AppliesCriteria()
    {
        var now = DateTime.UtcNow;
        var threshold = now.AddDays(-3);
        var repo = new RandomEventRepository(AddressFactory);

        // Generate many, then filter to last 3 days
        var spec = new GenerateNearPastEventsSpec(
            51.0, 17.0, count: 100, radiusMeters: 1000, seed: 44,
            criteria: e => e.EventTime.Start >= threshold);

        var list = await repo.ListAsync(spec);

        Assert.All(list, e => Assert.True(e.EventTime.Start >= threshold));
        Assert.True(list.Count <= 100);
    }

    [Fact]
    public async Task ListAsync_DeterministicWithSeed()
    {
        var repo = new RandomEventRepository(AddressFactory);

        var spec1 = new GenerateNearPastEventsSpec(51.1, 17.0, count: 10, radiusMeters: 300, seed: 555);
        var a = await repo.ListAsync(spec1);

        var spec2 = new GenerateNearPastEventsSpec(51.1, 17.0, count: 10, radiusMeters: 300, seed: 555);
        var b = await repo.ListAsync(spec2);

        Assert.Equal(a.Select(x => x.Title), b.Select(x => x.Title));
        Assert.Equal(a.Select(x => x.EventTime.Start), b.Select(x => x.EventTime.Start));
        Assert.Equal(a.Select(x => x.EventTime.End ?? DateTime.MinValue),
                     b.Select(x => x.EventTime.End ?? DateTime.MinValue));
        Assert.Equal(a.Select(x => ParseLat(x.Address)), b.Select(x => ParseLat(x.Address)));
        Assert.Equal(a.Select(x => ParseLon(x.Address)), b.Select(x => ParseLon(x.Address)));
    }

    [Fact]
    public async Task ListAsync_DifferentSeedsProduceDifferentResults()
    {
        var repo = new RandomEventRepository(AddressFactory);
        var a = await repo.ListAsync(new GenerateNearPastEventsSpec(51, 17, 10, 300, seed: 1));
        var b = await repo.ListAsync(new GenerateNearPastEventsSpec(51, 17, 10, 300, seed: 2));

        // Titles (or starts) should differ
        Assert.NotEqual(a.Select(x => x.Title), b.Select(x => x.Title));
    }

    [Fact]
    public async Task ListAsync_ThrowsOnNullOrUnsupportedSpec()
    {
        var repo = new RandomEventRepository(AddressFactory);

        await Assert.ThrowsAsync<NotSupportedException>(() => repo.ListAsync(null));

        var otherSpec = new DummySpec(); // not GenerateNearPastEventsSpec
        await Assert.ThrowsAsync<NotSupportedException>(() => repo.ListAsync(otherSpec));
    }

    // Minimal dummy spec to trigger NotSupportedException
    private sealed class DummySpec : ISpecification<Event>
    {
        public Expression<Func<Event, bool>>? Criteria => null;
        public Expression<Func<Event, object>>? OrderBy => null;
        public Expression<Func<Event, object>>? OrderByDescending => null;
        public int? Skip => null;
        public int? Take => null;
    }
}