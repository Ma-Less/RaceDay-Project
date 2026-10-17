using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EventsController(RaceDayDbContext context)
        {
            _context = context;
        }

        private int? GetCurrentUserId() => HttpContext.Session.GetInt32("UserID");
        private string? GetCurrentUserRole() => HttpContext.Session.GetString("Role");

        // GET: api/events (Any logged in user)
        [HttpGet]
        public async Task<IActionResult> GetAllEvents()
        {
            if (GetCurrentUserId() == null) return Unauthorized(new { message = "Not logged in." });

            var events = await _context.Events
                .Include(e => e.Categories)
                .Where(e => e.IsActive == true)
                .Select(e => new EventResponseDto
                {
                    EventID = e.EventID,
                    Name = e.Name,
                    Description = e.Description,
                    Date = e.Date,
                    Location = e.Location,
                    Distance = e.Distance,
                    EventType = e.EventType,
                    CreatedByUserID = e.CreatedByUserID,
                    IsActive = e.IsActive,
                    Categories = e.Categories.Select(c => new CategoryResponseDto
                    {
                        CategoryID = c.CategoryID,
                        Name = c.Name,
                        Description = c.Description
                    }).ToList()
                })
                .ToListAsync();

            return Ok(events);
        }

        // GET: api/events/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEvent(int id)
        {
            if (GetCurrentUserId() == null) return Unauthorized(new { message = "Not logged in." });

            var e = await _context.Events
                .Include(e => e.Categories)
                .FirstOrDefaultAsync(e => e.EventID == id);

            if (e == null) return NotFound(new { message = "Event does not exist." });

            var response = new EventResponseDto
            {
                EventID = e.EventID,
                Name = e.Name,
                Description = e.Description,
                Date = e.Date,
                Location = e.Location,
                Distance = e.Distance,
                EventType = e.EventType,
                CreatedByUserID = e.CreatedByUserID,
                IsActive = e.IsActive,
                Categories = e.Categories.Select(c => new CategoryResponseDto
                {
                    CategoryID = c.CategoryID,
                    Name = c.Name,
                    Description = c.Description
                }).ToList()
            };

            return Ok(response);
        }

        // POST: api/events (Organiser only)
        [HttpPost]
        public async Task<IActionResult> CreateEvent([FromBody] CreateEventDto dto)
        {
            var userId = GetCurrentUserId();
            var role = GetCurrentUserRole();

            if (userId == null) return Unauthorized(new { message = "Not logged in." });
            if (role != "Organiser") return StatusCode(403, new { message = "User is not an Organiser." });

            if (dto.EventType != "Run" && dto.EventType != "Walk" && dto.EventType != "Cycle")
                return BadRequest(new { message = "EventType must be Run, Walk, or Cycle." });

            var newEvent = new Event
            {
                Name = dto.Name,
                Description = dto.Description,
                Date = dto.Date,
                Location = dto.Location,
                Distance = dto.Distance,
                EventType = dto.EventType,
                CreatedByUserID = userId.Value,
                IsActive = true
            };

            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();

            return StatusCode(201, new { message = "Event created successfully.", eventId = newEvent.EventID });
        }

        // PUT: api/events/{id} (Organiser only - must be creator)
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEvent(int id, [FromBody] UpdateEventDto dto)
        {
            var userId = GetCurrentUserId();
            var role = GetCurrentUserRole();

            if (userId == null) return Unauthorized(new { message = "Not logged in." });
            if (role != "Organiser") return StatusCode(403, new { message = "User is not an Organiser." });

            var existingEvent = await _context.Events.FindAsync(id);
            if (existingEvent == null) return NotFound(new { message = "Event does not exist." });

            if (existingEvent.CreatedByUserID != userId.Value)
                return StatusCode(403, new { message = "User is not the organiser of this event." });

            existingEvent.Name = dto.Name;
            existingEvent.Description = dto.Description;
            existingEvent.Date = dto.Date;
            existingEvent.Location = dto.Location;
            existingEvent.Distance = dto.Distance;
            existingEvent.EventType = dto.EventType;
            existingEvent.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Event updated successfully." });
        }

        // DELETE: api/events/{id} (Organiser only - must be creator)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var userId = GetCurrentUserId();
            var role = GetCurrentUserRole();

            if (userId == null) return Unauthorized(new { message = "Not logged in." });
            if (role != "Organiser") return StatusCode(403, new { message = "User is not an Organiser." });

            var existingEvent = await _context.Events.FindAsync(id);
            if (existingEvent == null) return NotFound(new { message = "Event does not exist." });

            if (existingEvent.CreatedByUserID != userId.Value)
                return StatusCode(403, new { message = "User is not the organiser of this event." });

            // Soft delete
            existingEvent.IsActive = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}