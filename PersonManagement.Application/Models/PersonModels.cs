namespace PersonManagement.Application.Models;
public sealed record PersonData(string FirstName, string LastName, string NationalCode, DateTime BirthDate);
public sealed record PersonResult(int Id, string FirstName, string LastName, string NationalCode, DateTime BirthDate);
