using Ecommerce.ProductService.Data;
using Ecommerce.ProductService.Kafka.Consumer;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ProductDbContext>(
    opt => opt.UseNpgsql(builder.Configuration.GetConnectionString("ProductConnection")));

builder.Services.AddHostedService<KafkaConsumer>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
