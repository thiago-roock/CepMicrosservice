using System;
using System.Net;
using System.Threading.Tasks;
using Cep.Domain.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Cep.Domain.Models;


namespace Cep.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CepController : ControllerBase
    {
        private readonly IMediator _mediator;

        private readonly ILogger<CepController> _logger;

        public CepController(ILogger<CepController> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        /// <summary>
        /// Busca por cep
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpGet("/search/{cep}")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(CepModel))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> Get([FromRoute] CepCommand command)
        {
            try
            {
                _logger.LogInformation($"Iniciando busca por cep: {command?.Cep}");

                var result = await _mediator.Send(command);

                if (result is not null)
                {
                    _logger.LogInformation($"Deu tudo certo na busca do cep: {command?.Cep}, resultado: {JsonSerializer.Serialize(result)}");
                    return Ok(result);
                }
                else
                {
                    _logger.LogInformation($"Nào encontramos nenhuma informação sobre o cep: {command?.Cep}");
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error ao buscar cep: {command?.Cep}", ex);
                return BadRequest();
            }
            
        }
    }
}
