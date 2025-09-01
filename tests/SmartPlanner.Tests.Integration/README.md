# SmartPlanner Integration Tests

This directory contains integration tests for the SmartPlanner application.

## Current Status

✅ **Completed:**
- Integration test project setup with proper dependencies
- Test infrastructure (IntegrationTestFixture, IntegrationTestBase)
- Basic test structure for Authentication, Task, and Dashboard controllers
- HTML parsing support with AngleSharp for MVC form testing
- In-memory database configuration for test isolation
- Working authentication integration test (RegisterUser_Debug)
- Proper handling of MVC redirects in test client

⚠️ **Known Issues:**
- Some tests expect API-style responses but the application uses MVC patterns
- Need to update remaining tests to match MVC behavior (form submissions, redirects)
- Authentication tests need adjustment for session-based authentication vs JWT tokens

## Test Structure

### Base Classes
- `IntegrationTestFixture`: WebApplicationFactory setup with in-memory database
- `IntegrationTestBase`: Common test utilities for form handling and database access

### Test Classes
- `AuthenticationIntegrationTests`: User registration, login, logout flows
- `TaskIntegrationTests`: Task CRUD operations and authentication checks
- `DashboardIntegrationTests`: Dashboard display and task management
- `SimpleAuthenticationTest`: Debug tests for troubleshooting

## Running Tests

```bash
# Run all integration tests
dotnet test tests/SmartPlanner.Tests.Integration/

# Run specific test class
dotnet test tests/SmartPlanner.Tests.Integration/ --filter "SimpleAuthenticationTest"

# Run with verbose output
dotnet test tests/SmartPlanner.Tests.Integration/ --verbosity normal
```

## Next Steps

1. Fix database context isolation issue
2. Adjust test expectations to match MVC behavior (redirects vs status codes)
3. Implement proper session handling for authenticated requests
4. Add more comprehensive error scenario testing
5. Add API endpoint tests if API controllers are added later

## Dependencies

- Microsoft.AspNetCore.Mvc.Testing: For integration testing framework
- Microsoft.EntityFrameworkCore.InMemory: For in-memory database testing
- FluentAssertions: For readable test assertions
- AngleSharp: For HTML parsing and form handling
- xUnit: Test framework