using DietiEstate.Shared.Enums;

namespace DietiEstate.Shared.Dtos.Filters;

public class WorkItemFilterDto
{
    public WorkItemStatus? Status { get; set; }
    
    public WorkItemType? Type { get; set; }
    
    public DateTime? ScheduledAtMin { get; set; }
    
    public DateTime? ScheduledAtMax { get; set; }
    
    public string SortBy { get; init; } = "scheduled_at";

    public string SortOrder { get; init; } = "asc";
}