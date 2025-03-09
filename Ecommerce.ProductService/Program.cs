using Confluent.Kafka;
using Ecommerce.ProductService.Data;
using Ecommerce.ProductService.Kafka.Consumer;
using HealthChecks.Kafka;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);


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
    options.Configuration = redisConnectionString; // Убедитесь, что строка подключения правильная
    options.InstanceName = "EcommerceProductService:";  // Можно добавить имя инстанса для лучшей идентификации
});

builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("ProductConnection"))
    .AddRedis(builder.Configuration.GetValue<string>("Redis:ConnectionString"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//using (var scope = app.Services.CreateScope())
//{
//    var dbContext = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
//    dbContext.Database.Migrate();
//}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
