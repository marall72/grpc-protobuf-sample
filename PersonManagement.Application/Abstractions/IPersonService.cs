using PersonManagement.Application.Models;
namespace PersonManagement.Application.Abstractions;
public interface IPersonService
{
    Task<PersonResult> CreateAsync(PersonData data, CancellationToken cancellationToken);
    Task<PersonResult> GetAsync(int id, CancellationToken cancellationToken);
    Task<IReadOnlyList<PersonResult>> GetAllAsync(CancellationToken cancellationToken);
    Task<PersonResult> UpdateAsync(int id, PersonData data, CancellationToken cancellationToken);
    Task DeleteAsync(int id, CancellationToken cancellationToken);
}
