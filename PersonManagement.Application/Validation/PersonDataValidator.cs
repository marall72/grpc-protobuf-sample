using FluentValidation;
using PersonManagement.Application.Models;
namespace PersonManagement.Application.Validation;
public sealed class PersonDataValidator : AbstractValidator<PersonData>
{
    public PersonDataValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.NationalCode).Matches("^[0-9]{10}$").WithMessage("NationalCode must contain exactly 10 digits.");
        RuleFor(x => x.BirthDate).Must(x => x != default).WithMessage("BirthDate is required.").Must(x => x.Date <= DateTime.UtcNow.Date).WithMessage("BirthDate cannot be in the future.");
    }
}
