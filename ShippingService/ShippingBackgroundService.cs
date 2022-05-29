using Confluent.Kafka;

namespace ShippingService
{
    public class ShippingBackgroundService : BackgroundService
    {
        private readonly ILogger<ShippingBackgroundService> _logger;

        public ShippingBackgroundService(ILogger<ShippingBackgroundService> logger)
        {
            _logger = logger;
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Shipping service running...");

            // Config consumer
            var consumer = new ConsumerBuilder<Ignore, string>(new ConsumerConfig
            {
                BootstrapServers = "kafka:9092",
                GroupId = "test",
                AutoOffsetReset = AutoOffsetReset.Earliest
            }).Build();
            consumer.Subscribe("ready_to_ship");

            while (!stoppingToken.IsCancellationRequested)
            {
                var consumeResult = consumer.Consume(stoppingToken);

                // handle consumed message.
                _logger.LogInformation($"Handling ready to ship message {consumeResult.Message.Value}");
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Shipping service is stopping.");

            await base.StopAsync(stoppingToken);
        }
    }
}
