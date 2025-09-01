using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using SmartPlanner.Infrastructure.Data;
using SmartPlanner.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using TaskEntity = SmartPlanner.Domain.Entities.Task;

namespace SmartPlanner.Tests.Performance;

[MemoryDiagnoser]
[SimpleJob(BenchmarkDotNet.Jobs.RuntimeMoniker.Net80)]
public class DatabasePerformanceTests
{
    private SmartPlannerDbContext _context = null!;
    private WebApplicationFactory<Program> _factory = null!;

    [GlobalSetup]
    public async System.Threading.Tasks.Task Setup()
    {
        _factory = new WebApplicationFactory<Program>();
        var scope = _factory.Services.CreateScope();
        _context = scope.ServiceProvider.GetRequiredService<SmartPlannerDbContext>();
        
        // Seed test data
        await SeedTestDataAsync();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _context?.Dispose();
        _factory?.Dispose();
    }

    private async System.Threading.Tasks.Task SeedTestDataAsync()
    {
        // Clear existing data
        _context.Tasks.RemoveRange(_context.Tasks);
        await _context.SaveChangesAsync();

        // Create 1000 test tasks with various deadlines
        var tasks = new List<TaskEntity>();
        var random = new Random(42); // Fixed seed for consistent results

        for (int i = 0; i < 1000; i++)
        {
            tasks.Add(new TaskEntity
            {
                Id = Guid.NewGuid(),
                Title = $"Test Task {i}",
                Description = $"Description for test task {i}",
                Deadline = DateTime.Now.AddDays(random.Next(-30, 60)), // Tasks from 30 days ago to 60 days from now
                IsDone = (i % 4) != 0, // Most tasks not done
                StudentId = Guid.NewGuid(),
                CreatedAt = DateTime.Now.AddDays(-random.Next(0, 30)),
                UpdatedAt = DateTime.Now
            });
        }

        _context.Tasks.AddRange(tasks);
        await _context.SaveChangesAsync();
    }

    [Benchmark]
    public async System.Threading.Tasks.Task<List<TaskEntity>> GetTodayTasks_FilterByDeadline()
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);

        return await _context.Tasks
            .Where(t => t.Deadline >= today && t.Deadline < tomorrow)
            .Where(t => t.StudentId.ToString() == "test-user-id")
            .OrderBy(t => t.Title)
            .ToListAsync();
    }

    [Benchmark]
    public async System.Threading.Tasks.Task<List<TaskEntity>> GetUpcomingTasks_Next7Days()
    {
        var today = DateTime.Today;
        var nextWeek = today.AddDays(7);

        return await _context.Tasks
            .Where(t => t.Deadline >= today && t.Deadline <= nextWeek)
            .Where(t => t.StudentId.ToString() == "test-user-id")
            .Where(t => !t.IsDone)
            .OrderBy(t => t.Deadline)
            .ThenBy(t => t.Title)
            .ToListAsync();
    }

    [Benchmark]
    public async System.Threading.Tasks.Task<List<TaskEntity>> GetOverdueTasks()
    {
        var today = DateTime.Today;

        return await _context.Tasks
            .Where(t => t.Deadline < today)
            .Where(t => t.StudentId.ToString() == "test-user-id")
            .Where(t => !t.IsDone)
            .OrderBy(t => t.Deadline)
            .ToListAsync();
    }

    [Benchmark]
    public async System.Threading.Tasks.Task<int> GetTaskCountByStatus()
    {
        return await _context.Tasks
            .Where(t => t.StudentId.ToString() == "test-user-id")
            .Where(t => !t.IsDone)
            .CountAsync();
    }

    [Benchmark]
    public async System.Threading.Tasks.Task<List<TaskEntity>> SearchTasksByTitle()
    {
        return await _context.Tasks
            .Where(t => t.StudentId.ToString() == "test-user-id")
            .Where(t => t.Title.Contains("Test"))
            .OrderBy(t => t.CreatedAt)
            .Take(50)
            .ToListAsync();
    }

    // Method to run benchmarks programmatically
    public static void RunBenchmarks()
    {
        BenchmarkRunner.Run<DatabasePerformanceTests>();
    }
}
