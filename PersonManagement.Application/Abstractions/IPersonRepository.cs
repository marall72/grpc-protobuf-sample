using PersonManagement.Domain.Entities;
namespace PersonManagement.Application.Abstractions;
public interface IPersonRepository
{
    Task<Person?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Person>> GetAllAsync(CancellationToken cancellationToken);
    Task<bool> NationalCodeExistsAsync(string code, int? excludingId, CancellationToken cancellationToken);
    Task AddAsync(Person person, CancellationToken cancellationToken);
    Task DeleteAsync(Person person, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
