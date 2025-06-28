using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using NotificationService.Infrastructure.Consumers;
using Testcontainers.RabbitMq;

namespace NotificationService.Tests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        public readonly RabbitMqContainer rabbitMQContainter;

        public CustomWebApplicationFactory()
        {
            rabbitMQContainter = new RabbitMqBuilder()
                .WithUsername("guest")
                .WithPassword("guest")
                .Build();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove default MassTransit registration
                var massTransitServices = services
                    .Where(s => s.ServiceType.Namespace?.Contains("MassTransit") == true)
                    .ToList();

                foreach (var serviceDescriptor in massTransitServices)
                {
                    services.Remove(serviceDescriptor);
                }

                services.AddMassTransitTestHarness(x =>
                {
                    x.AddConsumer<NotificationEventConsumer>();
                    x.UsingRabbitMq((ctx, cfg) =>
                    {
                        cfg.Host(
                            rabbitMQContainter.Hostname,
                            rabbitMQContainter.GetMappedPublicPort(5672),
                            "/",
                            h =>
                            {
                                h.Username("guest");
                                h.Password("guest");
                            });

                        cfg.ConfigureEndpoints(ctx);
                    });
                });
            });
        }

        public void ConfigureRabbitMqHost(IRabbitMqBusFactoryConfigurator configurator)
        {
            configurator.Host("localhost", "/", h =>
            {
                h.Username("guest");
                h.Password("guest");
            });
        }

        public async Task InitializeAsync()
        {
            await rabbitMQContainter.StartAsync();
        }

        public async Task DisposeAsync()
        {
            await rabbitMQContainter.DisposeAsync();
        }
    }
}
