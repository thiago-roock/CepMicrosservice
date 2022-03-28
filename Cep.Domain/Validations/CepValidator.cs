using Cep.Domain.Commands;
using FluentValidation;

namespace Cep.Domain.Validations
{
    public class CepValidator : AbstractValidator<CepCommand>
    {
        public CepValidator()
        {
            RuleFor(x => x.Cep).NotEmpty().WithMessage("Porfavor especifique o cep");
            RuleFor(x => x.Cep).Length(8, 9);
        }
    }
}
