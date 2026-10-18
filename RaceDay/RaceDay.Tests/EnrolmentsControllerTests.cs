using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Controllers;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using Xunit;

namespace RaceDay.Tests
{
    public class EnrolmentsControllerTests
    {
        private RaceDayDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new RaceDayDbContext(options);
        }

        private EnrolmentsController GetControllerWithSession(RaceDayDbContext context, int userId, string role)
        {
            var session = new TestSession(); // Reusing the class from EventsControllerTests
            session.SetInt32("UserID", userId);
            session.SetString("Role", role);

            var mockHttpContext = new DefaultHttpContext();
            mockHttpContext.Session = session;

            var controller = new EnrolmentsController(context);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = mockHttpContext
            };

            return controller;
        }

        [Fact]
        public async Task EnrolInEvent_AsParticipant_Returns201Created_AndSavesToDb()
        {
            // Arrange
            var context = GetInMemoryDbContext();

            // Seed a test Event and Category
            var testEvent = new Event { Name = "Test Event", Date = DateTime.Now, Location = "Loc", Distance = "10km", EventType = "Run", CreatedByUserID = 99 };
            context.Events.Add(testEvent);
            await context.SaveChangesAsync();

            var testCategory = new Category { Name = "Open", EventID = testEvent.EventID };
            context.Categories.Add(testCategory);
            await context.SaveChangesAsync();

            // Create controller with Participant Role
            var controller = GetControllerWithSession(context, 2, "Participant");
            var dto = new EnrolEventDto { CategoryID = testCategory.CategoryID };

            // Act
            var result = await controller.EnrolInEvent(testEvent.EventID, dto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(201, objectResult.StatusCode);

            // Verify the enrolment was actually saved to the database
            var enrolmentInDb = await context.Enrolments.FirstOrDefaultAsync();
            Assert.NotNull(enrolmentInDb);
            Assert.Equal(2, enrolmentInDb.ParticipantUserID);
            Assert.Equal(testEvent.EventID, enrolmentInDb.EventID);
            Assert.Equal("Pending", enrolmentInDb.Status);
        }
    }
}