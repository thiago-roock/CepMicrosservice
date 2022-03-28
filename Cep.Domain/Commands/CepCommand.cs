using Cep.Domain.Models;
using MediatR;

namespace Cep.Domain.Commands
{
    public class CepCommand : IRequest<CepModel>
    {
        public string Cep { get; set; }
    }
}
