using System.Threading.Tasks;
using Cep.Domain.Infrastructure.ExternalServices.Models;

namespace Cep.Domain.Infrastructure.Repository
{
    public interface ICepDistributedCache
    {
        Task InsertItemCache(string keyIdentify, CepExternalServiceModel cache);

        Task<CepExternalServiceModel> GetItemCache(string keyIdentify);
    }
}
