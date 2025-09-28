using DietiEstate.Shared.Dtos.Filters;
using DietiEstate.Shared.Models;

namespace DietiEstate.WorkerService.Repositories.Interfaces;

public interface IWorkItemRepository
{
    Task<IEnumerable<WorkItem?>> GetWorkItemsAsync(WorkItemFilterDto filters);

    Task<WorkItem?> GetWorkItemByIdAsync(Guid workItemId);

    Task AddWorkItemAsync(WorkItem workItem);

    Task UpdateWorkItemAsync(WorkItem workItem);

    Task DeleteWorkItemAsync(WorkItem workItem);
}