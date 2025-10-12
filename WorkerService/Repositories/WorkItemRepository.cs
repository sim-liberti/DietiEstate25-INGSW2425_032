using DietiEstate.Shared.Dtos.Filters;
using DietiEstate.Shared.Enums;
using DietiEstate.Shared.Models;
using DietiEstate.WorkerService.Data;
using DietiEstate.WorkerService.Extensions;
using Microsoft.EntityFrameworkCore;

namespace DietiEstate.WorkerService.Repositories;

public class WorkItemRepository(DietiEstateDbContext context) : IWorkItemRepository
{
    public async Task<IEnumerable<WorkItem?>> GetWorkItemsAsync(WorkItemFilterDto filters)
    {
        return await context.WorkItem
            .ApplyFilters(filters)
            .ApplySorting(filters.SortBy, filters.SortOrder)
            .ToListAsync();
    }

    public async Task<IEnumerable<WorkItem>> GetPendingWorkItemsAsync(CancellationToken cancellationToken)
    {
        // //// INFO: Temporary code to test email sending //// //
        // var emailWorkItem = new WorkItem
        // {
        //     Type = WorkItemType.SendEmail,
        //     Data = "This is a test email message from DietiEstate Worker Service.",
        //     ScheduledAt = DateTime.UtcNow,
        //     StartedAt = DateTime.UtcNow,
        //     CompletedAt = DateTime.UtcNow,
        //     Status = WorkItemStatus.Pending,
        //     ErrorMessage = string.Empty
        // };
        // IEnumerable<WorkItem> workItems = new List<WorkItem> { emailWorkItem };
        //
        // return workItems;
        return await context.WorkItem
            .Where(wi => wi.Status == WorkItemStatus.Pending && wi.ScheduledAt <= DateTime.UtcNow)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<WorkItem?> GetWorkItemByIdAsync(Guid workItemId)
    {
        return await context.WorkItem.FindAsync(workItemId);
    }

    public async Task AddWorkItemAsync(WorkItem workItem)
    {
        await context.Database.BeginTransactionAsync();
        context.WorkItem.Add(workItem);
        await context.SaveChangesAsync();
        await context.Database.CommitTransactionAsync();
    }

    public async Task UpdateWorkItemAsync(WorkItem workItem)
    {
        await context.Database.BeginTransactionAsync();
        context.WorkItem.Update(workItem);
        await context.SaveChangesAsync();
        await context.Database.CommitTransactionAsync();
    }

    public async Task DeleteWorkItemAsync(WorkItem workItem)
    {
        await context.Database.BeginTransactionAsync();
        context.WorkItem.Remove(workItem);
        await context.SaveChangesAsync();
        await context.Database.CommitTransactionAsync();
    }

}