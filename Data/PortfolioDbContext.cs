using Microsoft.EntityFrameworkCore;

namespace PortfolioSite.Data;

public sealed class PortfolioDbContext : DbContext
{
    public PortfolioDbContext(DbContextOptions<PortfolioDbContext> options) : base(options)
    {
    }

    public DbSet<PortfolioContentRecord> PortfolioContents => Set<PortfolioContentRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PortfolioContentRecord>(entity =>
        {
            entity.ToTable("portfolio_contents");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedNever();
            entity.Property(x => x.JsonContent).HasColumnType("longtext").IsRequired();
            entity.Property(x => x.UpdatedAtUtc).HasColumnType("datetime(6)").IsRequired();
        });
    }
}
