using FluentValidation;

namespace Auth.Application.UseCases.Auth.Commands.RefreshToken
{
    internal sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
    {
        public RefreshTokenCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("El UserId es requerido");

            RuleFor(x => x.RefreshToken)
                .NotEmpty().WithMessage("El RefreshToken es requerido");
        }
    }
}
