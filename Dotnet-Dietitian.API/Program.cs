using System.Text;
using Dotnet_Dietitian.Application.Tools;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Dotnet_Dietitian.API.Extensions;
using Microsoft.OpenApi.Models;
using Dotnet_Dietitian.Persistence.Data;
using Dotnet_Dietitian.API.Middlewares;
using MassTransit;
using Dotnet_Dietitian.Infrastructure.Consumers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(opt =>
{
    opt.RequireHttpsMetadata = false;
    opt.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = JwtTokenDefaults.ValidIssuer,
        ValidAudience = JwtTokenDefaults.ValidAudience,
        ClockSkew = TimeSpan.Zero,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtTokenDefaults.Key)),
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
    };
});
// Add services to the container
builder.Services.AddControllers();

builder.Services.AddApplicationServices(builder.Configuration);

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Dotnet-Dietitian API", Version = "v1" });
});

// SignalR ekleyin
builder.Services.AddSignalR();

// MassTransit yapılandırma işlemleri
builder.Services.AddMassTransit(config =>
{
    // Consumer'ları ekle
    config.AddConsumer<MesajGonderildiConsumer>();
    config.AddConsumer<MesajOkunduConsumer>();
    
    // RabbitMQ yapılandırması
    config.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });
        
        cfg.ConfigureEndpoints(context);
    });
});





var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Dotnet-Dietitian API v1"));
}

//exception handler
app.UseMiddleware<ExceptionMiddleware>();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// SignalR hub endpoint'inizi ekleyin
app.MapHub<MesajlasmaChatHub>("/mesajlasmahub");

// Seed the database
if (app.Environment.IsDevelopment())
{
    await SeedData.SeedAsync(app.Services);
}

app.Run();