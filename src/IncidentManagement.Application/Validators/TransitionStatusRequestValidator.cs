using FluentValidation;
using IncidentManagement.Application.DTOs;

namespace IncidentManagement.Application.Validators;

public class TransitionStatusRequestValidator : AbstractValidator<TransitionStatusRequest>
{
    public TransitionStatusRequestValidator()
    {
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.Note).MaximumLength(2000);
    }
}
