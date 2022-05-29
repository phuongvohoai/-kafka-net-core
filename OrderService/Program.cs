using OrderService;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHostedService<OrderBackgroundService>();

var app = builder.Build();

app.Run();
