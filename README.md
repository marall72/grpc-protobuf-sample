# Person Management gRPC API

A layered CRUD service built with ASP.NET Core gRPC, Protocol Buffers, EF Core, SQL Server, FluentValidation, Serilog, and xUnit. No REST CRUD endpoints are exposed.

## Projects

- `PersonManagement.Domain`: Person entity.
- `PersonManagement.Application`: service contracts, validation, DTOs, and use cases.
- `PersonManagement.Infrastructure`: EF Core context, SQL Server mapping, and repository.
- `PersonManagement.API`: gRPC contract and endpoint host.
- `PersonManagement.Tests`: isolated application unit tests using an in-memory fake repository.

## Requirements and configuration

Install the .NET 10 SDK and SQL Server. Configure `ConnectionStrings:PersonDatabase` in `PersonManagement.API/appsettings.json` or override it with `ConnectionStrings__PersonDatabase`. The checked-in connection string is a local development example using Windows authentication; it contains no credentials.

## Database

The schema is code-first and includes a unique index on `NationalCode`. Install the EF CLI if needed, then from the repository root:

```powershell
dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialCreate --project PersonManagement.Infrastructure --startup-project PersonManagement.API
dotnet ef database update --project PersonManagement.Infrastructure --startup-project PersonManagement.API
```

The migration command creates a migration from the current entity configuration; commit the generated migration for repeatable deployments.

## Run

```powershell
dotnet restore PersonManagement.sln
dotnet build PersonManagement.sln
dotnet run --project PersonManagement.API
```

The server listens on the configured gRPC HTTP/2 endpoint. Use a gRPC client such as `grpcurl` with TLS configured for the environment. For local development, reflection is not enabled; provide the `.proto` file explicitly.

## Contract and sample calls

```protobuf
service PersonManagementService {
  rpc CreatePerson (PersonData) returns (PersonReply);
  rpc GetPerson (PersonId) returns (PersonReply);
  rpc GetAllPersons (Empty) returns (PersonList);
  rpc UpdatePerson (UpdatePersonRequest) returns (PersonReply);
  rpc DeletePerson (PersonId) returns (Empty);
}
message PersonData { string first_name = 1; string last_name = 2; string national_code = 3; google.protobuf.Timestamp birth_date = 4; }
```

Example with `grpcurl` (adjust address/TLS and use an RFC3339 timestamp):

```powershell
grpcurl -plaintext -import-path PersonManagement.API/Protos -proto persons.proto -d '{"firstName":"Ava","lastName":"Stone","nationalCode":"1234567890","birthDate":"1990-01-01T00:00:00Z"}' localhost:5000 personmanagement.PersonManagementService/CreatePerson
grpcurl -plaintext -import-path PersonManagement.API/Protos -proto persons.proto -d '{"id":1}' localhost:5000 personmanagement.PersonManagementService/GetPerson
grpcurl -plaintext -import-path PersonManagement.API/Protos -proto persons.proto -d '{}' localhost:5000 personmanagement.PersonManagementService/GetAllPersons
```

## Errors and logging

Invalid input maps to `InvalidArgument`, missing people to `NotFound`, duplicate national codes to `AlreadyExists`, and unexpected errors to a generic `Internal` response. Cancellation is passed through. Serilog logs operation IDs and unexpected exceptions; national codes are never logged.

## Tests

```powershell
dotnet test PersonManagement.sln
```
