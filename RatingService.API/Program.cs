using Common.Messaging.Interfaces;
using Common.Messaging;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using RatingService.Application.Interfaces;
using RatingService.Domain.Interfaces.Repositories;
using RatingService.Infrastructure.Persistence.DbContexts;
using RatingService.Infrastructure.Persistence.Repositories;
using Serilog;
using RatingService.API.Middlewares;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var rabbitHost = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? builder.Configuration["RabbitMq:Host"];
        var rabbitUsername = Environment.GetEnvironmentVariable("RABBITMQ_USERNAME") ?? builder.Configuration["RabbitMq:Username"];
        var rabbitPassword = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? builder.Configuration["RabbitMq:Password"];
        var rabbitVHost = Environment.GetEnvironmentVariable("RABBITMQ_VIRTUALHOST") ?? builder.Configuration["RabbitMq:VirtualHost"];

        builder.Services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((ctx, cfg) =>
            {
                cfg.Host(rabbitHost, rabbitVHost, h =>
                {
                    h.Username(rabbitUsername);
                    h.Password(rabbitPassword);
                });

                cfg.UseMessageRetry(r =>
                {
                    r.Interval(3, TimeSpan.FromSeconds(5));
                });
            });
        });

        builder.Services.AddScoped<INotificationEventPublisher, MassTransitNotificationPublisher>();

        builder.Services.AddDbContext<RatingDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

        builder.Services.AddScoped<IRatingRepository, RatingRepository>();
        builder.Services.AddScoped<IRatingService, RatingService.Application.Services.RatingService>();

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Host.UseSerilog((context, config) =>
        {
            config.ReadFrom.Configuration(context.Configuration);
        });

        var app = builder.Build();

        app.UseMiddleware<ExceptionMiddleware>();

        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<RatingDbContext>();
            db.Database.EnsureCreated();
        }

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }
}
