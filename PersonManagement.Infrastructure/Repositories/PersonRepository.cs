using Microsoft.EntityFrameworkCore;
using PersonManagement.Application.Abstractions;
using PersonManagement.Domain.Entities;
using PersonManagement.Infrastructure.Persistence;
namespace PersonManagement.Infrastructure.Repositories;
public sealed class PersonRepository(PersonDbContext db) : IPersonRepository
{
    public Task<Person?> GetByIdAsync(int id, CancellationToken ct) => db.Persons.SingleOrDefaultAsync(x => x.Id == id, ct);
    public async Task<IReadOnlyList<Person>> GetAllAsync(CancellationToken ct) => await db.Persons.AsNoTracking().OrderBy(x => x.Id).ToListAsync(ct);
    public Task<bool> NationalCodeExistsAsync(string code, int? excludingId, CancellationToken ct) => db.Persons.AnyAsync(x => x.NationalCode == code && (!excludingId.HasValue || x.Id != excludingId.Value), ct);
    public async Task AddAsync(Person person, CancellationToken ct) => await db.Persons.AddAsync(person, ct);
    public Task DeleteAsync(Person person, CancellationToken ct) { db.Persons.Remove(person); return Task.CompletedTask; }
    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
