using System.ComponentModel.DataAnnotations;
using DietiEstate.Shared.Enums;

namespace DietiEstate.Shared.Models;

public class WorkItem
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    public WorkItemStatus Status { get; set; }
    
    [Required]
    public WorkItemType Type { get; set; }
    
    [Required]
    public string Data { get; set; } = string.Empty;
    
    [Required]
    public DateTime ScheduledAt { get; set; }
    
    [Required]
    public DateTime CreatedAt { get; set; }
}