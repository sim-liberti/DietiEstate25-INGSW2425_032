using DietiEstate.Shared.Dtos.Filters;
using DietiEstate.Shared.Models;

namespace DietiEstate.WorkerService.Repositories;

public interface IWorkItemRepository
{
    Task<IEnumerable<WorkItem?>> GetWorkItemsAsync(WorkItemFilterDto filters);
    
    Task<IEnumerable<WorkItem>> GetPendingWorkItemsAsync(CancellationToken cancellationToken);

    Task<WorkItem?> GetWorkItemByIdAsync(Guid workItemId);

    Task AddWorkItemAsync(WorkItem workItem);

    Task UpdateWorkItemAsync(WorkItem workItem);

    Task DeleteWorkItemAsync(WorkItem workItem);
}