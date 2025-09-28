using DietiEstate.Shared.Dtos.Filters;
using DietiEstate.Shared.Models;

namespace DietiEstate.WorkerService.Extensions;

public static class WorkItemQueryExtensions
{
    public static IQueryable<WorkItem> ApplyFilters(this IQueryable<WorkItem> query, WorkItemFilterDto filters)
    {
        if (filters.Status.HasValue)
            query = query.Where(wi => wi.Status == filters.Status);
        if (filters.Type.HasValue)
            query = query.Where(wi => wi.Type == filters.Type);
        if (filters.ScheduledAtMin.HasValue)
            query = query.Where(wi => wi.ScheduledAt >= filters.ScheduledAtMin);
        if (filters.ScheduledAtMax.HasValue)
            query = query.Where(wi => wi.ScheduledAt >= filters.ScheduledAtMax);
        
        return query;
    }

    public static IQueryable<WorkItem> ApplySorting(this IQueryable<WorkItem> query, string sortBy, string sortOrder)
    {
        return sortBy.ToLower() switch
        {
            "status" => sortOrder == "desc" ? query.OrderByDescending(wi => wi.Status) : query.OrderBy(wi => wi.Status),
            "type" => sortOrder == "desc" ? query.OrderByDescending(wi => wi.Type) : query.OrderBy(wi => wi.Type),
            _ => query.OrderBy(wi => wi.ScheduledAt)
        };
    }
}