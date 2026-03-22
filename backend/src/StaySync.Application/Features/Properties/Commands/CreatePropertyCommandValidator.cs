using FluentValidation;

namespace StaySync.Application.Features.Properties.Commands;

public class CreatePropertyCommandValidator : AbstractValidator<CreatePropertyCommand>
{
    public CreatePropertyCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(255);
    }
}
