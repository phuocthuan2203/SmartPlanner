using Moq;
using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SmartPlanner.Application.Services;
using SmartPlanner.Application.Services.Interfaces;
using SmartPlanner.Application.DTOs;
using SmartPlanner.Domain.Entities;
using SmartPlanner.Domain.ValueObjects;
using SmartPlanner.Infrastructure.Data;
using Task = System.Threading.Tasks.Task;

namespace SmartPlanner.Tests.Unit.Services
{
    public class AuthenticationServiceTests : TestBase
    {
        private readonly SmartPlannerDbContext _context;
        private readonly Mock<ISecurityService> _mockSecurityService;
        private readonly AuthenticationService _authService;

        public AuthenticationServiceTests()
        {
            var options = new DbContextOptionsBuilder<SmartPlannerDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            
            _context = new SmartPlannerDbContext(options);
            _mockSecurityService = MockRepository.Create<ISecurityService>();
            
            _authService = new AuthenticationService(
                _context,
                _mockSecurityService.Object
            );
        }

        public override void Dispose()
        {
            _context.Dispose();
            base.Dispose();
        }

        [Fact]
        public async Task RegisterAsync_WithValidData_ShouldReturnSuccessResponse()
        {
            // Arrange
            var registerDto = TestDataFactory.CreateValidRegisterDTO();
                
            _mockSecurityService
                .Setup(x => x.HashPassword(registerDto.Password))
                .Returns("hashed_password");
                
            _mockSecurityService
                .Setup(x => x.GenerateAuthToken(It.IsAny<Guid>(), registerDto.Email, registerDto.FullName))
                .Returns("mock_jwt_token");

            // Act
            var result = await _authService.RegisterAsync(registerDto);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Token.Should().Be("mock_jwt_token");
            result.ErrorMessage.Should().BeNull();
        }

        [Fact]
        public async Task RegisterAsync_WithDuplicateEmail_ShouldReturnFailureResponse()
        {
            // Arrange
            var registerDto = TestDataFactory.CreateValidRegisterDTO();
            var existingStudent = TestDataFactory.CreateValidStudentAccount();
            existingStudent.Email = registerDto.Email;
            
            _context.StudentAccounts.Add(existingStudent);
            await _context.SaveChangesAsync();

            // Act
            var result = await _authService.RegisterAsync(registerDto);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("already exists");
        }

        [Fact]
        public async Task RegisterAsync_WithInvalidEmail_ShouldReturnFailureResponse()
        {
            // Arrange
            var registerDto = TestDataFactory.CreateValidRegisterDTO();
            registerDto.Email = "invalid-email";

            // Act
            var result = await _authService.RegisterAsync(registerDto);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Email format is invalid");
        }

        [Fact]
        public async Task RegisterAsync_WithMismatchedPasswords_ShouldReturnFailureResponse()
        {
            // Arrange
            var registerDto = TestDataFactory.CreateValidRegisterDTO();
            registerDto.ConfirmPassword = "different_password";

            // Act
            var result = await _authService.RegisterAsync(registerDto);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Passwords do not match");
        }

        [Fact]
        public async Task LoginAsync_WithValidCredentials_ShouldReturnSuccessResponse()
        {
            // Arrange
            var loginDto = TestDataFactory.CreateValidLoginDTO();
            var existingStudent = TestDataFactory.CreateValidStudentAccount();
            existingStudent.Email = loginDto.Email;
            
            _context.StudentAccounts.Add(existingStudent);
            await _context.SaveChangesAsync();
                
            _mockSecurityService
                .Setup(x => x.VerifyPassword(loginDto.Password, existingStudent.PasswordHash))
                .Returns(true);
                
            _mockSecurityService
                .Setup(x => x.GenerateAuthToken(existingStudent.Id, existingStudent.Email, existingStudent.FullName))
                .Returns("mock_jwt_token");

            // Act
            var result = await _authService.LoginAsync(loginDto);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Token.Should().Be("mock_jwt_token");
            result.StudentId.Should().Be(existingStudent.Id);
            result.StudentName.Should().Be(existingStudent.FullName);
        }

        [Fact]
        public async Task LoginAsync_WithInvalidCredentials_ShouldReturnFailureResponse()
        {
            // Arrange
            var loginDto = TestDataFactory.CreateValidLoginDTO();
            var existingStudent = TestDataFactory.CreateValidStudentAccount();
            existingStudent.Email = loginDto.Email;
            
            _context.StudentAccounts.Add(existingStudent);
            await _context.SaveChangesAsync();
                
            _mockSecurityService
                .Setup(x => x.VerifyPassword(loginDto.Password, existingStudent.PasswordHash))
                .Returns(false);

            // Act
            var result = await _authService.LoginAsync(loginDto);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Invalid email or password");
        }

        [Fact]
        public async Task LoginAsync_WithNonExistentUser_ShouldReturnFailureResponse()
        {
            // Arrange
            var loginDto = TestDataFactory.CreateValidLoginDTO();

            // Act
            var result = await _authService.LoginAsync(loginDto);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Invalid email or password");
        }
    }
}