using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WorkerAcoes.Data;
using WorkerAcoes.Extensions;

namespace WorkerAcoes
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    // Assume o uso do Apache Kafka e desta aplicação em modo testes
                    // quando não houver um password de acesso definido
                    if (KafkaExtensions.ExecutingTests(hostContext.Configuration))
                        KafkaExtensions.CheckTopicForTests(hostContext.Configuration);
                    
                    services.AddSingleton<AcoesRepository>();
                    services.AddHostedService<Worker>();
                });
    }
}