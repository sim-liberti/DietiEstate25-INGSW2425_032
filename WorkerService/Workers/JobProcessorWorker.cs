using DietiEstate.WorkerService.Repositories;
using DietiEstate.WorkerService.Services;

namespace DietiEstate.WorkerService.Workers;

public class JobProcessorWorker(
    ILogger<JobProcessorWorker> logger,
    IServiceScopeFactory scopeFactory
    ) : BackgroundService
{
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(1);
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Starting job processor worker");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessJobsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in job processor worker");
            }
            await Task.Delay(_interval, stoppingToken);
        }
    }

    private async Task ProcessJobsAsync(CancellationToken stoppingToken)
    {
        using var scope = scopeFactory.CreateScope();
        var workItemRepository = scope.ServiceProvider.GetRequiredService<IWorkItemRepository>();
        var jobService = scope.ServiceProvider.GetRequiredService<IJobService>();
        var pendingWorkItems = await workItemRepository.GetPendingWorkItemsAsync(stoppingToken);

        foreach (var workItem in pendingWorkItems)
        {
            await jobService.ExecuteWorkItemAsync(workItem, stoppingToken);
        }
    }
}
