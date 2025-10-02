namespace Events.API.ViewModels;

public record EventViewModel
{
    public int Id { get; set; }
    
    public string Title { get; set; } = "";
    
    public DateTime StartsAtUtc { get; set; }
    
    public DateTime EndsAtUtc { get; set; }
    
    public bool IsAllDay { get; set; }
    
    public string? Location { get; set; }
    
    public string? Description { get; set; }
    
    public DateTime CreatedUtc { get; set; }
}