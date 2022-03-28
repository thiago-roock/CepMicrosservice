using System.Threading.Tasks;
using Cep.Domain.Infrastructure.ExternalServices.Models;
using Refit;

namespace Cep.Domain.Infrastructure.ExternalServices
{
    public interface ICepExternalService
    {
        [Get("/ws/{cep}/json/")]
        Task<CepExternalServiceModel> GetCep(string cep);
    }
}
