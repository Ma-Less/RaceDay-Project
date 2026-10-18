using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using RaceDay.API.Controllers;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using Xunit;

namespace RaceDay.Tests
{
    public class AuthControllerTests
    {
        private RaceDayDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new RaceDayDbContext(options);
        }

        // Helper to mock the session for the Login test
        private AuthController GetControllerWithMockSession(RaceDayDbContext context)
        {
            var mockSession = new Mock<ISession>();
            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(c => c.Session).Returns(mockSession.Object);

            var controller = new AuthController(context);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = mockHttpContext.Object
            };

            return controller;
        }

        [Fact]
        public async Task Register_WithNewEmail_Returns201Created()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = new AuthController(context);
            var dto = new RegisterDto
            {
                FirstName = "Test",
                LastName = "User",
                Email = "newuser@test.com",
                Password = "Password123!",
                Role = "Participant"
            };

            // Act
            var result = await controller.Register(dto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(201, createdResult.StatusCode);
        }

        [Fact]
        public async Task Register_WithExistingEmail_Returns409Conflict()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = new AuthController(context);
            var dto = new RegisterDto
            {
                FirstName = "Test",
                LastName = "User",
                Email = "existing@test.com",
                Password = "Password123!",
                Role = "Participant"
            };

            await controller.Register(dto); // Register once

            // Act - Try to register again with the same email
            var result = await controller.Register(dto);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            Assert.Equal(409, conflictResult.StatusCode);
        }

        [Fact]
        public async Task Login_WithValidCredentials_Returns200Ok()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var registerController = new AuthController(context);
            var registerDto = new RegisterDto
            {
                FirstName = "Sipho",
                LastName = "Runner",
                Email = "login@test.com",
                Password = "Password123!",
                Role = "Participant"
            };
            await registerController.Register(registerDto);

            var loginController = GetControllerWithMockSession(context);
            var loginDto = new LoginDto { Email = "login@test.com", Password = "Password123!" };

            // Act
            var result = await loginController.Login(loginDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }
    }
}