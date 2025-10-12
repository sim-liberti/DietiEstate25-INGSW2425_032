using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using DietiEstate.Shared.Enums;

namespace DietiEstate.Shared.Models;

public class WorkItem
{
    [Key]
    public Guid Id { get; init; } = Guid.NewGuid();
    
    [Required]
    public WorkItemStatus Status { get; set; } = WorkItemStatus.Pending;
    
    [Required]
    public WorkItemType Type { get; set; }
    
    [Required]
    public string Data { get; set; } = "{}";
    
    [Required]
    public DateTime ScheduledAt { get; set; }
    
    [Required]
    public DateTime StartedAt { get; set; }
    
    [Required]
    public DateTime CompletedAt { get; set; }
    
    [Required]
    public string ErrorMessage { get; set; } = string.Empty;

    public T GetData<T>() where T : class
    {
        return JsonSerializer.Deserialize<T>(Data)!;
    }

    public void SetData<T>(T data) where T : class
    {
        Data = JsonSerializer.Serialize(data);
    }
}