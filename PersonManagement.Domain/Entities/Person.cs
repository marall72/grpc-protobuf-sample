namespace PersonManagement.Domain.Entities;

public sealed class Person
{
    private Person() { }
    public Person(string firstName, string lastName, string nationalCode, DateTime birthDate)
    { FirstName = firstName; LastName = lastName; NationalCode = nationalCode; BirthDate = birthDate; }
    public int Id { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string NationalCode { get; private set; } = string.Empty;
    public DateTime BirthDate { get; private set; }
    public void Update(string firstName, string lastName, string nationalCode, DateTime birthDate)
    { FirstName = firstName; LastName = lastName; NationalCode = nationalCode; BirthDate = birthDate; }
}
