# SmartPlanner Performance & Security Testing Plan (AI Agent Instructions)

## **Phase Context**
- **Project**: SmartPlanner ASP.NET Core web application
- **Current State**: Unit and integration tests completed
- **Target**: Validate performance benchmarks and security controls

## **Performance Testing Requirements**

### **1. Load Testing**
```bash
# Create: tests/SmartPlanner.Tests.Performance/
dotnet add package NBomber --version 5.0.0
```

**Key Tests:**
- Dashboard endpoint under 50 concurrent users
- Verify 95th percentile response time < 2 seconds
- Monitor error rates and throughput

**Skeleton:**
```csharp
// DashboardLoadTests.cs
public class DashboardLoadTests : LoadTestBase
{
    [Fact]
    public async Task Dashboard_UnderLoad_MeetsPerformanceTarget()
    {
        // AI Agent: Setup NBomber scenario targeting /api/dashboard
        // Simulate 50 users for 60 seconds
        // Assert response time < 2s, error rate < 1%
    }
}
```

### **2. Database Performance**
```bash
dotnet add package BenchmarkDotNet --version 0.13.8
```

**Key Tests:**
- Profile task filtering queries
- Measure GetTodayTasks and GetUpcomingTasks performance
- Identify optimization opportunities

**Skeleton:**
```csharp
// DatabasePerformanceTests.cs
[MemoryDiagnoser]
public class DatabasePerformanceTests
{
    [Benchmark]
    public async Task GetStudentTasks_FilterByDeadline()
    {
        // AI Agent: Benchmark critical dashboard queries
        // Setup 1000 test tasks, measure query performance
    }
}
```

## **Security Testing Requirements**

### **1. JWT Security**
```bash
# Create: tests/SmartPlanner.Tests.Security/
dotnet add package System.IdentityModel.Tokens.Jwt
```

**Key Tests:**
- Valid token acceptance
- Invalid/tampered token rejection
- Expired token handling

**Skeleton:**
```csharp
// JwtSecurityTests.cs
public class JwtSecurityTests
{
    [Fact]
    public async Task ProtectedEndpoint_WithTamperedToken_ShouldReturn401()
    {
        // AI Agent: Get valid token, tamper with signature, verify rejection
    }
    
    [Fact]
    public async Task ExpiredToken_ShouldBeRejected()
    {
        // AI Agent: Test expired token scenarios
    }
}
```

### **2. Password Security**
```bash
dotnet add package BCrypt.Net-Next --version 4.0.3
```

**Key Tests:**
- BCrypt hashing verification
- Weak password rejection
- Brute force protection

**Skeleton:**
```csharp
// PasswordSecurityTests.cs
public class PasswordSecurityTests
{
    [Fact] 
    public async Task PasswordHashing_ShouldUseBCrypt()
    {
        // AI Agent: Verify BCrypt usage and proper verification
    }
    
    [Theory]
    [InlineData("123"), InlineData("password")]
    public async Task WeakPasswords_ShouldBeRejected(string weakPassword)
    {
        // AI Agent: Test password strength validation
    }
}
```

### **3. Input Validation**

**Key Tests:**
- SQL injection prevention
- XSS payload sanitization
- Input length and format validation

**Skeleton:**
```csharp
// InputValidationTests.cs
public class InputValidationTests
{
    [Theory]
    [InlineData("'; DROP TABLE Tasks; --")]
    [InlineData("<script>alert('XSS')</script>")]
    public async Task MaliciousInput_ShouldBeSanitized(string payload)
    {
        // AI Agent: Test task creation with malicious payloads
        // Verify rejection or proper sanitization
    }
}
```

### **4. HTTPS Enforcement**

**Skeleton:**
```csharp
// HttpsEnforcementTests.cs 
public class HttpsEnforcementTests
{
    [Fact]
    public async Task HttpRequest_ShouldRedirectToHttps()
    {
        // AI Agent: Test HTTP to HTTPS redirection
        // Verify security headers presence
    }
}
```

## **CI Integration**

**File:** `.github/workflows/performance-security.yml`

```yaml
name: Performance & Security Tests
on:
  schedule:
    - cron: '0 2 * * *'  # Nightly runs

jobs:
  performance-tests:
    # AI Agent: Run NBomber load tests
    # Upload results as artifacts
    
  security-tests:
    # AI Agent: Run security test suite
    # Optional: Integrate OWASP ZAP scan
```

## **Execution Commands**

```bash
# Run performance tests
dotnet test tests/SmartPlanner.Tests.Performance/

# Run security tests  
dotnet test tests/SmartPlanner.Tests.Security/

# Run benchmarks
dotnet run --project tests/SmartPlanner.Tests.Performance/ --configuration Release

# All tests with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## **Success Criteria**

**Performance:**
- Dashboard loads < 2s under 50 concurrent users
- Database queries optimized and profiled
- Memory usage stable, no leaks detected

**Security:**
- JWT validation working correctly
- BCrypt password hashing confirmed
- Input validation prevents injection attacks
- HTTPS properly enforced

## **AI Agent Implementation Notes**

- Use `WebApplicationFactory<Program>` for test setup
- Create test helper methods for authentication
- Focus on critical user paths (dashboard, task CRUD)
- Keep tests fast and reliable
- Generate reports for performance metrics
- Integrate with existing CI pipeline
- Follow same patterns as unit/integration tests