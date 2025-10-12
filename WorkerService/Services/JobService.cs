using DietiEstate.Shared.Enums;
using DietiEstate.Shared.Models;
using DietiEstate.WorkerService.Repositories;

namespace DietiEstate.WorkerService.Services;

public class JobService(
    IEmailService emailService,
    ILogger<JobService> logger,
    IWorkItemRepository workItemRepository
    ) : IJobService
{
    public async Task ExecuteWorkItemAsync(WorkItem workItem, CancellationToken stoppingToken)
    {
        try
        {
            workItem.Status = WorkItemStatus.Processing;
            workItem.StartedAt = DateTime.UtcNow;
            await workItemRepository.UpdateWorkItemAsync(workItem);

            switch (workItem.Type)
            {
                case WorkItemType.SendEmail:
                    await emailService.SendEmailAsync(workItem.Data, stoppingToken);
                    break;
                case WorkItemType.Cleanup:
                    // TODO: Research which cleanup tasks to perform
                    break;
                case WorkItemType.Report:
                    // TODO: Implement report generation
                    break;
                default:
                    throw new NotSupportedException($"Job type {workItem.Type} not supported");
            }
            
            workItem.Status = WorkItemStatus.Completed;
            workItem.CompletedAt = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Work item {itemId} processing failed", workItem.Id);
            workItem.Status = WorkItemStatus.Failed;
            workItem.ErrorMessage = ex.Message;   
        }
        finally
        {
            await workItemRepository.UpdateWorkItemAsync(workItem);
        }
    }
}