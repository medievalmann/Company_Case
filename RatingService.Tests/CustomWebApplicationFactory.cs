using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RatingService.Infrastructure.Persistence.DbContexts;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace RatingService.Tests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        public readonly PostgreSqlContainer postgresContainer;
        public readonly RabbitMqContainer rabbitMQContainter;

        public CustomWebApplicationFactory()
        {
            postgresContainer = new PostgreSqlBuilder()
                .WithDatabase("testdb")
                .WithUsername("testuser")
                .WithPassword("testpassword")
                .Build();

            rabbitMQContainter = new RabbitMqBuilder()
                .WithUsername("guest")
                .WithPassword("guest")
                .Build();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove default DbContext registration
                var dbContextDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<RatingDbContext>));

                if (dbContextDescriptor != null)
                {
                    services.Remove(dbContextDescriptor);
                }

                services.AddDbContext<RatingDbContext>(options =>
                {
                    options.UseNpgsql(postgresContainer.GetConnectionString());
                });

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
            await postgresContainer.StartAsync();
            await rabbitMQContainter.StartAsync();
        }

        public async Task DisposeAsync()
        {
            await postgresContainer.DisposeAsync();
            await rabbitMQContainter.DisposeAsync();
        }
    }
}
