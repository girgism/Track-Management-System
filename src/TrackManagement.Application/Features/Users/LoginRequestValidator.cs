using FluentValidation;

using global::TrackManagement.Domain.Entities.Users;


namespace TrackManagement.Application.Features.Users;

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().MaximumLength(User.EmailMaxLength);
        RuleFor(x => x.Password).NotEmpty().MaximumLength(256);
    }
}
