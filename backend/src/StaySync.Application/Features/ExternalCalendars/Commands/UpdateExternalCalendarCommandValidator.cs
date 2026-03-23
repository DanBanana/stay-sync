using FluentValidation;

namespace StaySync.Application.Features.ExternalCalendars.Commands;

public class UpdateExternalCalendarCommandValidator : AbstractValidator<UpdateExternalCalendarCommand>
{
    public UpdateExternalCalendarCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Platform).NotEmpty().MaximumLength(100);
        RuleFor(x => x.IcsUrl).NotEmpty().MaximumLength(2048).Must(BeAValidUrl).WithMessage("ICS URL must be a valid URL.");
    }

    private static bool BeAValidUrl(string url) =>
        Uri.TryCreate(url, UriKind.Absolute, out var result) &&
        (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
}
