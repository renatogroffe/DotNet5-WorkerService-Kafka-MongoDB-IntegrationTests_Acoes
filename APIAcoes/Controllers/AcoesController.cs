using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Confluent.Kafka;
using MongoDB.Driver;
using APIAcoes.Models;
using APIAcoes.Documents;
using APIAcoes.Extensions;

namespace APIAcoes.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AcoesController : ControllerBase
    {
        private readonly ILogger<AcoesController> _logger;
        private readonly IConfiguration _configuration;

        public AcoesController(ILogger<AcoesController> logger,
            [FromServices]IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [HttpPost]
        [ProducesResponseType(typeof(Resultado), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<Resultado> Post(Acao acao)
        {
            CotacaoAcao cotacaoAcao = new ()
            {
                Codigo = acao.Codigo,
                Valor = acao.Valor,
                CodCorretora = _configuration["Corretora:Codigo"],
                NomeCorretora = _configuration["Corretora:Nome"]
            };
            var conteudoAcao = JsonSerializer.Serialize(cotacaoAcao);
            _logger.LogInformation($"Dados: {conteudoAcao}");

            string topic = _configuration["ApacheKafka:Topic"];

            using (var producer = KafkaExtensions.CreateProducer(_configuration))
            {
                var result = await producer.ProduceAsync(
                    topic,
                    new Message<Null, string>
                    { Value = conteudoAcao });

                _logger.LogInformation(
                    $"Apache Kafka - Envio para o tópico {topic} concluído | " +
                    $"{conteudoAcao} | Status: { result.Status.ToString()}");
            }

            return new ()
            {
                Mensagem = "Informações de ação enviadas com sucesso!"
            };
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<AcaoDocument>), (int)HttpStatusCode.OK)]
        public List<AcaoDocument> ListAll()
        {
            return new MongoClient(_configuration["MongoDBConnection"])
                .GetDatabase(_configuration["MongoDatabase"])
                .GetCollection<AcaoDocument>(_configuration["MongoCollection"])
                .Find(all => true).ToList();
        }
    }
}