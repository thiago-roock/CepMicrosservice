using System.Threading;
using System.Threading.Tasks;
using Cep.Domain.Commands;
using Cep.Domain.Infrastructure.ExternalServices;
using Cep.Domain.Infrastructure.ExternalServices.Models;
using Cep.Domain.Infrastructure.Repository;
using Cep.Domain.Models;
using MediatR;
using Microsoft.Extensions.Logging;


namespace Cep.Domain.Handlers
{
    public class CepHandler : IRequestHandler<CepCommand, CepModel>
    {
        private readonly ICepExternalService _cepExternalServices;
        private readonly ILogger<CepHandler> _logger;
        private readonly ICepDistributedCache _cepDistributedCache;

        public CepHandler(ICepExternalService cepExternalService, ILogger<CepHandler> logger, ICepDistributedCache cepDistributedCache)
        {
            _cepExternalServices = cepExternalService;
            _logger = logger;
            _cepDistributedCache = cepDistributedCache;
        }

        public async Task<CepModel> Handle(CepCommand request, CancellationToken cancellationToken)
        {
            CepExternalServiceModel cepExternalServiceModel = await GetCepOrderFallback(request.Cep);

            if (cepExternalServiceModel is not null)
            {
                await _cepDistributedCache.InsertItemCache(request.Cep, cepExternalServiceModel);

                    return new()
                    {
                        Cep = cepExternalServiceModel.Cep,
                        Logradouro = cepExternalServiceModel.Logradouro,
                        Complemento = cepExternalServiceModel.Complemento,
                        Bairro = cepExternalServiceModel.Bairro,
                        Localidade = cepExternalServiceModel.Localidade,
                        Uf = cepExternalServiceModel.Uf,
                        Ibge = cepExternalServiceModel.Ibge,
                        Gia = cepExternalServiceModel.Gia,
                        Ddd = cepExternalServiceModel.Ddd,
                        Siafi = cepExternalServiceModel.Siafi
                    };
            }

            return null;
        }

        private async Task<CepExternalServiceModel> GetCepOrderFallback(string cep)
        {
            CepExternalServiceModel cepExternalServiceModel = await SearchCaching(cep);

            if(cepExternalServiceModel is null)
                cepExternalServiceModel = await _cepExternalServices.GetCep(cep);

            return cepExternalServiceModel;
        }

        private async Task<CepExternalServiceModel> SearchCaching(string cep)=>
            await _cepDistributedCache.GetItemCache(cep);
    }
}
