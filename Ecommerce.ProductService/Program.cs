using Confluent.Kafka;
using Ecommerce.ProductService.Data;
using Ecommerce.ProductService.DI;
using Ecommerce.ProductService.Kafka.Consumer;
using HealthChecks.Kafka;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);
SerilogProductLogging.ConfigureLogging();
builder.ConfigureOpenTelemetry();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

builder.Services.AddDbContext<ProductDbContext>(
    opt => opt.UseNpgsql(builder.Configuration.GetConnectionString("ProductConnection")));

builder.Services.AddHostedService<HostedKafkaConsumer>();
builder.Services.AddSingleton<KafkaConsumer>();
builder.Services.AddStackExchangeRedisCache(options =>
{
    var redisConnectionString = builder.Configuration.GetValue<string>("Redis:ConnectionString");
    options.Configuration = redisConnectionString; 
    options.InstanceName = "EcommerceProductService:";  
});

builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("ProductConnection"))
    .AddRedis(builder.Configuration.GetValue<string>("Redis:ConnectionString"));

builder.Host.UseSerilog();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
    dbContext.Database.Migrate();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
