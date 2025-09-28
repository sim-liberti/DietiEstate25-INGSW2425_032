using DietiEstate.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace DietiEstate.WorkerService.Data;

public class DietiEstateDbContext(DbContextOptions<DietiEstateDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Gets or sets the DbSet for managing <see cref="WorkItem"/> entities.
    /// </summary>
    public virtual DbSet<WorkItem> WorkItem { get; set; }
}