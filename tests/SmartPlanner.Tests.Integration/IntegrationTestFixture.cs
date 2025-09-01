using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SmartPlanner.Infrastructure.Data;

namespace SmartPlanner.Tests.Integration
{
    public class IntegrationTestFixture : WebApplicationFactory<Program>
    {
        private readonly string _databaseName = "TestDatabase_" + Guid.NewGuid();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove the existing DbContext registration
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<SmartPlannerDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }
                
                // Replace with in-memory database for testing isolation
                services.AddDbContext<SmartPlannerDbContext>(options =>
                {
                    options.UseInMemoryDatabase(_databaseName);
                });

                // Ensure database is created for each test
                var serviceProvider = services.BuildServiceProvider();
                using var scope = serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<SmartPlannerDbContext>();
                context.Database.EnsureCreated();
            });

            // Use test environment settings
            builder.UseEnvironment("Testing");
        }
    }
}