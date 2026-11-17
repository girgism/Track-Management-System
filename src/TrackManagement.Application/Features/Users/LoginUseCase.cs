using FluentValidation;

using global::TrackManagement.Application.Common.Auth;

using TrackManagement.Application.Common.Exceptions;

namespace TrackManagement.Application.Features.Users;

public sealed class LoginUseCase(IUserRepository users,
                                 IPasswordHasher passwordHasher,
                                 ITokenService tokenService,
                                 IValidator<LoginRequest> validator)
{
    public async Task<LoginResponse> ExecuteAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var email = request.Email.Trim().ToLowerInvariant();
        var user = await users.GetByEmailAsync(email, cancellationToken);

        var passwordValid = passwordHasher.Verify(user?.PasswordHash, request.Password);

        if (user is null || !passwordValid)
            throw new AuthenticationFailedException();

        var token = tokenService.Generate(user);

        return new LoginResponse(token.Value, "Bearer", token.ExpiresAtUtc);
    }
}
