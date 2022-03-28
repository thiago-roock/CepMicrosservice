using System;
using System.Text.Json;
using System.Threading.Tasks;
using Cep.Domain.Infrastructure.ExternalServices.Models;
using Cep.Domain.Infrastructure.Repository;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Cep.Infrastructure.Repository
{
    public class CepDistributedCache : ICepDistributedCache
    {
        private readonly ILogger<CepDistributedCache> _logger;
        private readonly IDistributedCache _cache;
        private readonly IConfiguration _configuration;
        private TimeSpan timeExpiredCached;

        public CepDistributedCache(IDistributedCache cache, IConfiguration configuration, ILogger<CepDistributedCache> logger)
        {
            _cache = cache;
            _configuration = configuration; 
            _logger = logger;

            _logger.LogInformation("Carregando configurações do redis...");

            timeExpiredCached = TimeSpan.FromSeconds(int.Parse(_configuration["CACHE_TIME_EXPIRED_CACHED"]));

            _logger.LogInformation("Configurações carregadas com sucesso:");
            _logger.LogInformation($"Nome da instância: {_configuration["CACHE_INSTANCE_NAME"]}");
            _logger.LogInformation($"Servidor: {_configuration["CACHE_CONFIGURATION_URL"]}");
            _logger.LogInformation($"Tempo de expiração: {timeExpiredCached}");
        }

        public async Task<CepExternalServiceModel> GetItemCache(string keyIdentify)
        {
            CepExternalServiceModel FromCache = null;
            try
            {
                var ObjCache = await _cache.GetStringAsync($"{keyIdentify}");
                if (ObjCache is not null)
                    FromCache = JsonSerializer.Deserialize<CepExternalServiceModel>(ObjCache);                           

                _logger.LogInformation($"Get {keyIdentify}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exceção: {ex.GetType().FullName} | " +
                            $"Mensagem: {ex.Message}");

                throw new ArgumentException($"Exceção: {ex.GetType().FullName} | " +
                               $"Mensagem: {ex.Message}");
            }
            return FromCache;
        }

        public async Task InsertItemCache(string keyIdentify, CepExternalServiceModel cache)
        {
            try
            {
                var value = JsonSerializer.Serialize(cache);
                _logger.LogInformation($"inserindo {keyIdentify}: {value}");
                await _cache.SetStringAsync($"{keyIdentify}", value);
                _logger.LogInformation("inserção realizada com sucesso no Redis!");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exceção: {ex.GetType().FullName} | " +
                            $"Mensagem: {ex.Message}");

                throw new ArgumentException($"Exceção: {ex.GetType().FullName} | " +
                               $"Mensagem: {ex.Message}");
            }
        }
    }
}
