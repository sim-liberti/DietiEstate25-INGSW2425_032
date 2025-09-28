using DietiEstate.Shared.Models;
using DietiEstate.Shared.Models.ListingModels;
using DietiEstate.Shared.Models.Shared;
using DietiEstate.Shared.Models.UserModels;
using Microsoft.EntityFrameworkCore;

namespace DietiEstate.WebApi.Data;

/// <summary>
/// Represents the Entity Framework database context for the DietiEstate application, providing access to the database
/// and defining sets for the application's main entities.
/// </summary>
/// <remarks>
/// This class inherits from <see cref="DbContext"/> and is configured with Entity Framework Core.
/// It includes DbSet properties corresponding to database tables for handling application data.
/// </remarks>
public class DietiEstateDbContext(DbContextOptions<DietiEstateDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Paramteless constructor for testing purposes.
    /// </summary>
    public DietiEstateDbContext() : this(new DbContextOptions<DietiEstateDbContext>()) {}
    
    /// <summary>
    /// Gets or sets the DbSet for managing <see cref="Listing"/> entities.
    /// </summary>
    public virtual DbSet<Listing> Listing { get; set; }
    
    /// <summary>
    /// Gets or sets the DbSet for managing <see cref="Image"/> entities.
    /// </summary>
    public virtual DbSet<Image> Image { get; set; }
    
    /// <summary>
    /// Gets or sets the DbSet for managing <see cref="Service"/> entities.
    /// </summary>
    public virtual DbSet<Service> Service { get; set; }
    
    /// <summary>
    /// Gets or sets the DbSet for managing <see cref="Tag"/> entities.
    /// </summary>
    public virtual DbSet<Tag> Tag { get; set; }
    
    /// <summary>
    /// Gets or sets the DbSet for managing <see cref="PropertyType"/> entities.
    /// </summary>
    public virtual DbSet<PropertyType> PropertyType { get; set; }
    
    /// <summary>
    /// Gets or sets the DbSet for managing <see cref="User"/> entities.
    /// </summary>
    public virtual DbSet<User> User { get; set; }
    
    /// <summary>
    /// Gets or sets the DbSet for managing <see cref="UserSession"/> entities.
    /// </summary>
    public virtual DbSet<UserSession> UserSession { get; set; }
    
    /// <summary>
    /// Gets or sets the DbSet for managing <see cref="UserVerification"/> entities.
    /// </summary>
    public virtual DbSet<UserVerification> UserVerification { get; set; }
    
    /// <summary>
    /// Gets or sets the DbSet for managing <see cref="WorkItem"/> entities.
    /// </summary>
    public virtual DbSet<WorkItem> WorkItem { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Listing>()
            .HasMany(l => l.ListingImages)
            .WithMany(i => i.Listings)
            .UsingEntity("ListingImage");
        
        modelBuilder.Entity<Image>(entity =>
        {
            entity.HasIndex(i => i.Url)
                .IsUnique();
        });
    }

}