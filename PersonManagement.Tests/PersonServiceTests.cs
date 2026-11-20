using Xunit;
using PersonManagement.Application.Abstractions;
using PersonManagement.Application.Models;
using PersonManagement.Application.Services;
using PersonManagement.Domain.Entities;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging.Abstractions;
namespace PersonManagement.Tests;
public sealed class PersonServiceTests
{
    private readonly FakeRepository _repo = new();
    private readonly PersonService _service;
    public PersonServiceTests() => _service = new(_repo, new TestValidator(), NullLogger<PersonService>.Instance);
    [Fact] public async Task Creates_valid_person() { var result = await _service.CreateAsync(Data(), default); Assert.Equal(1, result.Id); }
    [Fact] public async Task Rejects_invalid_national_code() { await Assert.ThrowsAsync<ValidationException>(() => _service.CreateAsync(Data(code:"12"), default)); }
    [Fact] public async Task Rejects_duplicate_national_code() { await _service.CreateAsync(Data(), default); await Assert.ThrowsAsync<DuplicateNationalCodeException>(() => _service.CreateAsync(Data(), default)); }
    [Fact] public async Task Retrieves_person() { var created = await _service.CreateAsync(Data(), default); Assert.Equal(created, await _service.GetAsync(created.Id, default)); }
    [Fact] public async Task Missing_person_throws() => await Assert.ThrowsAsync<PersonNotFoundException>(() => _service.GetAsync(12, default));
    [Fact] public async Task Updates_person() { var p = await _service.CreateAsync(Data(), default); var u = await _service.UpdateAsync(p.Id, Data(first:"Jane"), default); Assert.Equal("Jane", u.FirstName); }
    [Fact] public async Task Deletes_person() { var p = await _service.CreateAsync(Data(), default); await _service.DeleteAsync(p.Id, default); await Assert.ThrowsAsync<PersonNotFoundException>(() => _service.GetAsync(p.Id, default)); }
    [Fact] public async Task Rejects_future_birth_date() => await Assert.ThrowsAsync<ValidationException>(() => _service.CreateAsync(Data(birth:DateTime.UtcNow.AddDays(1)), default));
    private static PersonData Data(string first="John", string code="1234567890", DateTime? birth=null) => new(first,"Smith",code,birth ?? new DateTime(1990,1,1));
    private sealed class TestValidator : AbstractValidator<PersonData>
    { public TestValidator() { RuleFor(x=>x.FirstName).NotEmpty(); RuleFor(x=>x.LastName).NotEmpty(); RuleFor(x=>x.NationalCode).Matches("^[0-9]{10}$"); RuleFor(x=>x.BirthDate).Must(x=>x.Date <= DateTime.UtcNow.Date); } }
    private sealed class FakeRepository : IPersonRepository
    {
        private readonly List<Person> _items=[]; private int _id;
        public Task<Person?> GetByIdAsync(int id,CancellationToken ct) => Task.FromResult(_items.FirstOrDefault(x=>x.Id==id));
        public Task<IReadOnlyList<Person>> GetAllAsync(CancellationToken ct) => Task.FromResult<IReadOnlyList<Person>>(_items.ToArray());
        public Task<bool> NationalCodeExistsAsync(string code,int? excludingId,CancellationToken ct) => Task.FromResult(_items.Any(x=>x.NationalCode==code && x.Id!=excludingId));
        public Task AddAsync(Person person,CancellationToken ct) { typeof(Person).GetProperty(nameof(Person.Id))!.SetValue(person,++_id); _items.Add(person); return Task.CompletedTask; }
        public Task DeleteAsync(Person person,CancellationToken ct) { _items.Remove(person); return Task.CompletedTask; }
        public Task SaveChangesAsync(CancellationToken ct) => Task.CompletedTask;
    }
}


