using Confluent.Kafka;
using Ecommerce.OrderService.Data;
using Ecommerce.OrderService.Kafka.Producer;
using Ecommerce.OrderService.OutBox;
using HealthChecks.Kafka;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHostedService<OutBoxProcessor>();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IKafkaProducer, KafkaProducer>();
builder.Services.AddDbContext<OrderDbContext>(
    opt => opt.UseNpgsql(builder.Configuration.GetConnectionString("OrderConnection")));

builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("OrderConnection"))
    .AddKafka(new KafkaHealthCheckOptions
    {
        Configuration = new ProducerConfig
        {
            BootstrapServers = builder.Configuration.GetValue<string>("Kafka:BootstrapServers")
        },
        Topic = "healthchecks-topic"
    });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//using (var scope = app.Services.CreateScope())
//{
//    var dbContext = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
//    dbContext.Database.Migrate();
//}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
