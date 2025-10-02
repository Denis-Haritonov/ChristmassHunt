namespace Events.Models;

public class EventTime
{
    /// <summary>
    /// When the event starts (or occurs, if momentary).
    /// </summary>
    public DateTime Start { get; set; }

    /// <summary>
    /// When the event ends (null for momentary events).
    /// </summary>
    public DateTime? End { get; set; }

    /// <summary>
    /// True if the event is a single moment (no duration).
    /// </summary>
    public bool IsMomentary => !End.HasValue || Start == End;

    /// <summary>
    /// True if the event spans a duration.
    /// </summary>
    public bool IsRange => End.HasValue && Start < End;
}