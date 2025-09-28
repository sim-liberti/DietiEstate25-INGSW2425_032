using Microsoft.EntityFrameworkCore;

namespace DietiEstate.WorkerService.Data;

public class DietiEstateDbContext(DbContextOptions<DietiEstateDbContext> options) : DbContext(options)
{
    
}