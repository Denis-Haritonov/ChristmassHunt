using Common.Repository;

namespace Events.Models;

public class Event : IEntity
{
    public int Id { get; init;  }
    
    public string Title { get; set; }
        
    public string Description { get; set; }
    
    public string PhotoPath { get; set; }
    
    public EventTime EventTime { get; set; }
    
    public Address Address { get; set; }
}