using Confluent.Kafka;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/orders", async ([FromBody] OrderRequest orderRequest) =>
{
    // Config producer
    var config = new ProducerConfig
    {
        BootstrapServers = "kafka:9092",
    };

    var producer = new ProducerBuilder<string, string>(config).Build();

    // Publish message to orders_request topic
    var deliveryResult = await producer.ProduceAsync(topic: "orders_request", new Message<string, string>()
    {
        Value = JsonSerializer.Serialize(orderRequest)
    });

    Console.WriteLine($"KAFKA => Delivered '{deliveryResult.Value}' to '{deliveryResult.TopicPartitionOffset}'");

    // Return response to client
    return Results.Created("TransactionId", "Your order is in progress");
});

app.Run();

internal record OrderRequest()
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Status { get; set; } = "IN_PROGRESS";
}