using MassTransit;
using NotificationService.Infrastructure.Consumers;
using NotificationService.Infrastructure.Stores.Interfaces;
using NotificationService.Infrastructure.Stores;
using Serilog;
using NotificationService.API.Middlewares;


namespace NotificationService;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddSingleton<INotificationStore, NotificationStore>();

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var rabbitHost = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? builder.Configuration["RabbitMq:Host"];
        var rabbitUsername = Environment.GetEnvironmentVariable("RABBITMQ_USERNAME") ?? builder.Configuration["RabbitMq:Username"];
        var rabbitPassword = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? builder.Configuration["RabbitMq:Password"];
        var rabbitVHost = Environment.GetEnvironmentVariable("RABBITMQ_VIRTUALHOST") ?? builder.Configuration["RabbitMq:VirtualHost"];
        var notificationQueueName = builder.Configuration["RabbitMq:NotificationQueueName"] ?? "notification-service";

        builder.Services.AddMassTransit(x =>
        {
            x.AddConsumer<NotificationEventConsumer>();

            x.UsingRabbitMq((ctx, cfg) =>
            {
                cfg.Host(rabbitHost, rabbitVHost, h =>
                {
                    h.Username(rabbitUsername);
                    h.Password(rabbitPassword);
                });

                cfg.ReceiveEndpoint(notificationQueueName, e =>
                {
                    e.ConfigureConsumer<NotificationEventConsumer>(ctx);
                });

                cfg.UseMessageRetry(r =>
                {
                    r.Interval(3, TimeSpan.FromSeconds(5));
                });
            });
        });

        builder.Host.UseSerilog((context, config) =>
        {
            config.ReadFrom.Configuration(context.Configuration);
        });

        var app = builder.Build();

        app.UseMiddleware<ExceptionMiddleware>();

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
