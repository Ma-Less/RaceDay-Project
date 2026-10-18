using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Controllers;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using Xunit;

namespace RaceDay.Tests
{
    // A simple implementation of ISession to avoid complex Moq setups
    public class TestSession : ISession
    {
        private readonly Dictionary<string, byte[]> _store = new();
        public bool IsAvailable => true;
        public string Id => "TestSession";
        public IEnumerable<string> Keys => _store.Keys;
        public void Clear() => _store.Clear();
        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Remove(string key) => _store.Remove(key);
        public void Set(string key, byte[] value) => _store[key] = value;
        public bool TryGetValue(string key, out byte[] value) => _store.TryGetValue(key, out value);
    }

    public class EventsControllerTests
    {
        private RaceDayDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new RaceDayDbContext(options);
        }

        private EventsController GetControllerWithSession(RaceDayDbContext context, int userId, string role)
        {
            var session = new TestSession();
            session.SetInt32("UserID", userId);
            session.SetString("Role", role);

            var mockHttpContext = new DefaultHttpContext();
            mockHttpContext.Session = session;

            var controller = new EventsController(context);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = mockHttpContext
            };

            return controller;
        }

        [Fact]
        public async Task CreateEvent_AsOrganiser_Returns201Created()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = GetControllerWithSession(context, 1, "Organiser");

            var dto = new CreateEventDto
            {
                Name = "Test Event",
                Date = DateTime.Now.AddDays(10),
                Location = "Test Location",
                Distance = "10km",
                EventType = "Run"
            };

            // Act
            var result = await controller.CreateEvent(dto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(201, objectResult.StatusCode);
        }

        [Fact]
        public async Task CreateEvent_AsParticipant_Returns403Forbidden()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = GetControllerWithSession(context, 1, "Participant"); // Wrong Role!

            var dto = new CreateEventDto
            {
                Name = "Test Event",
                Date = DateTime.Now.AddDays(10),
                Location = "Test Location",
                Distance = "10km",
                EventType = "Run"
            };

            // Act
            var result = await controller.CreateEvent(dto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(403, objectResult.StatusCode); // 403 Forbidden
        }
    }
}