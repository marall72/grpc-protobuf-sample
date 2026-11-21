# Person Management gRPC API

A layered CRUD service built with ASP.NET Core gRPC, Protocol Buffers, EF Core, SQL Server, FluentValidation, Serilog. No REST CRUD endpoints are exposed.

## Projects

- `PersonManagement.Domain`: Person entity.
- `PersonManagement.Application`: service contracts, validation, DTOs, and use cases.
- `PersonManagement.Infrastructure`: EF Core context, SQL Server mapping, and repository.
- `PersonManagement.API`: gRPC contract and endpoint host.

## Requirements and configuration

Install the .NET 10 SDK and SQL Server. Configure `ConnectionStrings:PersonDatabase` in `PersonManagement.API/appsettings.json` or override it with `ConnectionStrings__PersonDatabase`. Set a local SQL Server connection string in `PersonManagement.API/appsettings.json` or use user secrets/environment variables for credentials. Do not commit real credentials.

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
dotnet restore grpc-protobuf-sample.slnx
dotnet build grpc-protobuf-sample.slnx
dotnet run --project PersonManagement.API
```

The server listens on the configured gRPC HTTP/2 endpoint. Use a gRPC client such as `grpcurl` with TLS configured for the environment. For local development, reflection is not enabled; provide the `.proto` file explicitly.

## gRPC contract and example calls

The service contract is in `PersonManagement.API/Protos/persons.proto`. Server reflection is not enabled, so pass this file to `grpcurl`. The examples below use the local HTTPS profile (`localhost:7443`) and `-insecure` to accept the development certificate. Remove `-insecure` when using a trusted certificate in another environment.

Run these commands from the repository root while the API is running:

```powershell
# Create
'{"firstName":"Sara","lastName":"Ahmadi","nationalCode":"1234567890","birthDate":"1990-05-20T00:00:00Z"}' | grpcurl -insecure -import-path .\PersonManagement.API\Protos -proto persons.proto -d '@' localhost:7443 personmanagement.PersonManagementService/CreatePerson

# Get by ID
'{"id":1}' | grpcurl -insecure -import-path .\PersonManagement.API\Protos -proto persons.proto -d '@' localhost:7443 personmanagement.PersonManagementService/GetPerson

# Get all
'{}' | grpcurl -insecure -import-path .\PersonManagement.API\Protos -proto persons.proto -d '@' localhost:7443 personmanagement.PersonManagementService/GetAllPersons

# Update by ID
'{"id":1,"firstName":"Sara","lastName":"Ahmadi","nationalCode":"1234567890","birthDate":"1990-05-20T00:00:00Z"}' | grpcurl -insecure -import-path .\PersonManagement.API\Protos -proto persons.proto -d '@' localhost:7443 personmanagement.PersonManagementService/UpdatePerson

# Delete by ID
'{"id":1}' | grpcurl -insecure -import-path .\PersonManagement.API\Protos -proto persons.proto -d '@' localhost:7443 personmanagement.PersonManagementService/DeletePerson
```

The `birthDate` values use RFC3339 format and are converted to `google.protobuf.Timestamp` by the Proto JSON parser. Replace ID `1` with the ID returned by `CreatePerson`.

```protobuf
service PersonManagementService {
  rpc CreatePerson (PersonData) returns (PersonReply);
  rpc GetPerson (PersonId) returns (PersonReply);
  rpc GetAllPersons (Empty) returns (PersonList);
  rpc UpdatePerson (UpdatePersonRequest) returns (PersonReply);
  rpc DeletePerson (PersonId) returns (Empty);
}
message PersonData {
  string first_name = 1;
  string last_name = 2;
  string national_code = 3;
  google.protobuf.Timestamp birth_date = 4;
}
```
## Errors and logging

Invalid input maps to `InvalidArgument`, missing people to `NotFound`, duplicate national codes to `AlreadyExists`, and unexpected errors to a generic `Internal` response. Cancellation is passed through. Serilog logs operation IDs and unexpected exceptions; national codes are never logged.




