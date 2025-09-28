using DietiEstate.Shared.Dtos.Filters;
using DietiEstate.Shared.Models;
using DietiEstate.WorkerService.Data;
using DietiEstate.WorkerService.Extensions;
using DietiEstate.WorkerService.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DietiEstate.WorkerService.Repositories.Implementations;

public class WorkItemRepository(DietiEstateDbContext context) : IWorkItemRepository
{
    public async Task<IEnumerable<WorkItem?>> GetWorkItemsAsync(WorkItemFilterDto filters)
    {
        return await context.WorkItem
            .ApplyFilters(filters)
            .ApplySorting(filters.SortBy, filters.SortOrder)
            .ToListAsync();
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