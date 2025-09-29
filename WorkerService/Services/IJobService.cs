using DietiEstate.Shared.Models;

namespace DietiEstate.WorkerService.Services;

public interface IJobService
{
    Task ExecuteWorkItemAsync(WorkItem workItem, CancellationToken stoppingToken);
}