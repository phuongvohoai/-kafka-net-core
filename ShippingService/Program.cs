using ShippingService;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHostedService<ShippingBackgroundService>();

var app = builder.Build();

app.Run();
