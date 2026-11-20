using Microsoft.EntityFrameworkCore;
using PersonManagement.Domain.Entities;
namespace PersonManagement.Infrastructure.Persistence;
public sealed class PersonDbContext(DbContextOptions<PersonDbContext> options) : DbContext(options)
{
    public DbSet<Person> Persons => Set<Person>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Person>(entity => {
            entity.ToTable("Persons"); entity.HasKey(x => x.Id);
            entity.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.LastName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.NationalCode).HasMaxLength(10).IsUnicode(false).IsRequired();
            entity.HasIndex(x => x.NationalCode).IsUnique(); entity.Property(x => x.BirthDate).IsRequired();
        });
    }
}
