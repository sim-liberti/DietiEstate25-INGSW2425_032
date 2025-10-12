using DietiEstate.Shared.Enums;
using DietiEstate.WorkerService.Data;
using DietiEstate.WorkerService.Repositories;
using DietiEstate.WorkerService.Services;
using DietiEstate.WorkerService.Workers;
using Microsoft.EntityFrameworkCore;
using DotNetEnv;

namespace DietiEstate.WorkerService;

public static class Program
{
    public static async Task Main(string[] args)
    {
        Env.Load();
        var builder = Host.CreateApplicationBuilder(args);
        ConfigureServices(builder);
        builder.Services.AddHostedService<JobProcessorWorker>();

        var host = builder.Build();
        await host.RunAsync();
    }

    private static void ConfigureServices(HostApplicationBuilder builder)
    {
        builder.Services.AddDbContext<DietiEstateDbContext>(options =>
        {
            options.UseNpgsql(Environment.GetEnvironmentVariable("CONNECTION_STRING"), dboptions =>
            {
                dboptions.MapEnum<WorkItemStatus>("work_item_status")
                    .EnableRetryOnFailure();
                dboptions.MapEnum<WorkItemType>("work_item_type")
                    .EnableRetryOnFailure();
                dboptions.EnableRetryOnFailure(0);
            });
        }, ServiceLifetime.Transient);

        builder.Services.AddScoped<IWorkItemRepository, WorkItemRepository>();
        builder.Services.AddScoped<IJobService, JobService>();
        builder.Services.AddSingleton<IEmailService, EmailService>();
        
    }
}
