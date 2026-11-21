using FluentValidation;
using Microsoft.Extensions.Logging;
using PersonManagement.Application.Abstractions;
using PersonManagement.Application.Models;
using PersonManagement.Domain.Entities;

namespace PersonManagement.Application.Services;

public sealed class PersonService(IPersonRepository repository, IValidator<PersonData> validator, ILogger<PersonService> logger) : IPersonService
{
    public async Task<PersonResult> CreateAsync(PersonData data, CancellationToken ct)
    {
        await Validate(data, null, ct);
        var entity = new Person(data.FirstName.Trim(), data.LastName.Trim(), data.NationalCode, data.BirthDate);
        await repository.AddAsync(entity, ct); await repository.SaveChangesAsync(ct);
        logger.LogInformation("Created person {PersonId}", entity.Id); return Map(entity);
    }

    public async Task<PersonResult> GetAsync(int id, CancellationToken ct) { 
        ValidateId(id); 
        return Map(await Find(id, ct));
    }

    public async Task<IReadOnlyList<PersonResult>> GetAllAsync(CancellationToken ct) => (await repository.GetAllAsync(ct)).Select(Map).ToArray();

    public async Task<PersonResult> UpdateAsync(int id, PersonData data, CancellationToken ct)
    {
        ValidateId(id); var entity = await Find(id, ct); await Validate(data, id, ct);
        entity.Update(data.FirstName.Trim(), data.LastName.Trim(), data.NationalCode, data.BirthDate);
        await repository.SaveChangesAsync(ct); logger.LogInformation("Updated person {PersonId}", id); return Map(entity);
    }

    public async Task DeleteAsync(int id, CancellationToken ct)
    { 
        ValidateId(id); 
        var entity = await Find(id, ct);
        await repository.DeleteAsync(entity, ct);
        await repository.SaveChangesAsync(ct); 
        logger.LogInformation("Deleted person {PersonId}", id); 
    }

    private async Task Validate(PersonData data, int? excludingId, CancellationToken ct)
    {
        await validator.ValidateAndThrowAsync(data, ct);
        if (await repository.NationalCodeExistsAsync(data.NationalCode, excludingId, ct))
            throw new DuplicateNationalCodeException();
    }

    private async Task<Person> Find(int id, CancellationToken ct) => await repository.GetByIdAsync(id, ct) ?? throw new PersonNotFoundException();

    private static void ValidateId(int id) { 
        if (id <= 0) throw new ValidationException("Person ID must be positive.");
    }

    private static PersonResult Map(Person p) => new(p.Id, p.FirstName, p.LastName, p.NationalCode, p.BirthDate);
}
public class PersonNotFoundException : Exception { }
public class DuplicateNationalCodeException : Exception { }


