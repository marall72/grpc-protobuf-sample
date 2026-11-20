using FluentValidation;
using Grpc.Core;
using PersonManagement.Application.Abstractions;
using PersonManagement.Application.Models;
using PersonManagement.Application.Services;
namespace PersonManagement.API.Services;
public sealed class PersonsGrpcService(IPersonService people, ILogger<PersonsGrpcService> logger) : PersonManagementService.PersonManagementServiceBase
{
    public override async Task<PersonReply> CreatePerson(PersonData request, ServerCallContext context) => ToReply(await Execute(() => people.CreateAsync(ToData(request), context.CancellationToken)));
    public override async Task<PersonReply> GetPerson(PersonId request, ServerCallContext context) => ToReply(await Execute(() => people.GetAsync(request.Id, context.CancellationToken)));
    public override async Task<PersonList> GetAllPersons(Empty request, ServerCallContext context)
    { var rows = await Execute(() => people.GetAllAsync(context.CancellationToken)); var result = new PersonList(); result.Persons.AddRange(rows.Select(ToReply)); return result; }
    public override async Task<PersonReply> UpdatePerson(UpdatePersonRequest request, ServerCallContext context)
    { var data = new PersonData { FirstName = request.FirstName, LastName = request.LastName, NationalCode = request.NationalCode, BirthDate = request.BirthDate }; return ToReply(await Execute(() => people.UpdateAsync(request.Id, ToData(data), context.CancellationToken))); }
    public override async Task<Empty> DeletePerson(PersonId request, ServerCallContext context) { await Execute(async () => { await people.DeleteAsync(request.Id, context.CancellationToken); return true; }); return new Empty(); }
    private async Task<T> Execute<T>(Func<Task<T>> action)
    {
        try { return await action(); }
        catch (OperationCanceledException) { throw; }
        catch (ValidationException ex) { logger.LogWarning("Person request validation failed"); throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Errors.FirstOrDefault()?.ErrorMessage ?? ex.Message)); }
        catch (PersonNotFoundException) { throw new RpcException(new Status(StatusCode.NotFound, "Person was not found.")); }
        catch (DuplicateNationalCodeException) { throw new RpcException(new Status(StatusCode.AlreadyExists, "NationalCode already exists.")); }
        catch (RpcException) { throw; }
        catch (Exception ex) { logger.LogError(ex, "Unexpected error processing person request"); throw new RpcException(new Status(StatusCode.Internal, "An internal error occurred.")); }
    }
    private static Application.Models.PersonData ToData(global::PersonManagement.API.PersonData request) => new(request.FirstName, request.LastName, request.NationalCode, request.BirthDate?.ToDateTime() ?? default);
    private static PersonReply ToReply(PersonResult p) => new() { Id=p.Id, FirstName=p.FirstName, LastName=p.LastName, NationalCode=p.NationalCode, BirthDate=Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.SpecifyKind(p.BirthDate, DateTimeKind.Utc)) };
}

