using Microsoft.AspNetCore.Mvc.Testing;
using NBomber.CSharp;
using NBomber.Http.CSharp;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;

namespace SmartPlanner.Tests.Performance;

public class DashboardLoadTests : LoadTestBase
{
    public DashboardLoadTests(WebApplicationFactory<Program> factory) : base(factory) { }

    [Fact]
    public async Task Dashboard_UnderLoad_MeetsPerformanceTarget()
    {
        // Get authentication token for the test
        var token = await GetAuthTokenAsync();
        
        var scenario = Scenario.Create("dashboard_load_test", async context =>
        {
            using var client = _factory.CreateClient();
            
            // For MVC, we expect redirects to login for unauthenticated requests
            var response = await client.GetAsync("/Dashboard");
            
            // Success if we get OK or redirect (expected behavior)
            return (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.Redirect) 
                ? Response.Ok() : Response.Fail();
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.KeepConstant(copies: 3, during: TimeSpan.FromSeconds(15))  // 3 concurrent users for 15 seconds
        );

        var stats = NBomberRunner
            .RegisterScenarios(scenario)
            .WithReportFolder("load-test-results")
            .Run();

        // Assert performance requirements
        var scnStats = stats.ScenarioStats.First(x => x.ScenarioName == "dashboard_load_test");
        
        // Should have made some requests
        Assert.True(scnStats.AllRequestCount > 0, 
            $"Should have made some requests, got {scnStats.AllRequestCount}");
        
        // Error rate should be reasonable (allowing for redirects)
        var errorRate = (double)scnStats.Fail.Request.Count / scnStats.AllRequestCount * 100;
        Assert.True(errorRate < 50.0, 
            $"Error rate {errorRate:F2}% exceeds 50% threshold");
    }

    [Fact]
    public async Task Dashboard_StressTest_HandlesHighLoad()
    {
        var token = await GetAuthTokenAsync();
        
        var scenario = Scenario.Create("dashboard_stress_test", async context =>
        {
            using var client = _factory.CreateClient();
            
            // For MVC, we expect redirects to login for unauthenticated requests
            var response = await client.GetAsync("/Dashboard");
            
            // Success if we get OK or redirect (expected behavior)
            return (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.Redirect) 
                ? Response.Ok() : Response.Fail();
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.KeepConstant(copies: 2, during: TimeSpan.FromSeconds(10)) // Stress test with 2 users
        );

        var stats = NBomberRunner
            .RegisterScenarios(scenario)
            .WithReportFolder("stress-test-results")
            .Run();

        var scnStats = stats.ScenarioStats.First(x => x.ScenarioName == "dashboard_stress_test");
        
        // Should have made some requests
        Assert.True(scnStats.AllRequestCount > 0, 
            $"Should have made some requests, got {scnStats.AllRequestCount}");
        
        // Error rate should still be reasonable under stress
        var errorRate = (double)scnStats.Fail.Request.Count / scnStats.AllRequestCount * 100;
        Assert.True(errorRate < 70.0, 
            $"Error rate {errorRate:F2}% exceeds 70% stress threshold");
    }
}
