using Microsoft.EntityFrameworkCore;

namespace IxcPosVendaWorker.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<ContratoProcessado> ContratosProcessados { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ContratoProcessado>()
            .HasKey(c => c.Id);

        modelBuilder.Entity<ContratoProcessado>()
            .HasIndex(c => c.IdContrato)
            .IsUnique();
    }
}