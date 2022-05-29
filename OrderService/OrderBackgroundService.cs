using Confluent.Kafka;
using System.Text.Json;

namespace OrderService
{
    public class OrderBackgroundService : BackgroundService
    {
        private readonly ILogger<OrderBackgroundService> _logger;

        public OrderBackgroundService(ILogger<OrderBackgroundService> logger)
        {
            _logger = logger;
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Order service running...");

            // Config producer
            var producer = new ProducerBuilder<string, string>(new ProducerConfig
            {
                BootstrapServers = "kafka:9092",
            }).Build();

            // Config consumer
            var consumer = new ConsumerBuilder<Ignore, string>(new ConsumerConfig
            {
                BootstrapServers = "kafka:9092",
                GroupId = "test",
                AutoOffsetReset = AutoOffsetReset.Earliest
            }).Build();
            consumer.Subscribe("orders_request");

            while (!stoppingToken.IsCancellationRequested)
            {
                var consumeResult = consumer.Consume(stoppingToken);

                // handle consumed message.
                _logger.LogInformation($"Handling order request message {consumeResult.Message.Value}");

                var orderRequest = JsonSerializer.Deserialize<OrderRequest>(consumeResult.Message.Value);

                orderRequest.Status = "COMPLETED";

                // Publish message to ready_to_ship topic
                var deliveryResult = await producer.ProduceAsync(topic: "ready_to_ship", new Message<string, string>()
                {
                    Value = JsonSerializer.Serialize(orderRequest)
                });

                Console.WriteLine($"KAFKA => Delivered '{deliveryResult.Value}' to '{deliveryResult.TopicPartitionOffset}'");
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Order service is stopping.");

            await base.StopAsync(stoppingToken);
        }
    }

    internal record OrderRequest()
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Status { get; set; } = "IN_PROGRESS";
    }
}
