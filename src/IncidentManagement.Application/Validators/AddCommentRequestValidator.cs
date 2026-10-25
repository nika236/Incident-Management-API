using FluentValidation;
using IncidentManagement.Application.DTOs;

namespace IncidentManagement.Application.Validators;

public class AddCommentRequestValidator : AbstractValidator<AddCommentRequest>
{
    public AddCommentRequestValidator()
    {
        RuleFor(x => x.Body).NotEmpty().MaximumLength(4000);
    }
}
