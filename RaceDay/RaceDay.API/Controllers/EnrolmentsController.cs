using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers
{
    [Route("api")]
    [ApiController]
    public class EnrolmentsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EnrolmentsController(RaceDayDbContext context)
        {
            _context = context;
        }

        private int? GetCurrentUserId() => HttpContext.Session.GetInt32("UserID");
        private string? GetCurrentUserRole() => HttpContext.Session.GetString("Role");

        // POST: api/events/{eventId}/enrol (Participant only)
        [HttpPost("events/{eventId}/enrol")]
        public async Task<IActionResult> EnrolInEvent(int eventId, [FromBody] EnrolEventDto dto)
        {
            var userId = GetCurrentUserId();
            var role = GetCurrentUserRole();

            if (userId == null) return Unauthorized(new { message = "Not logged in." });
            if (role != "Participant") return StatusCode(403, new { message = "User is not a Participant." });

            var eventEntity = await _context.Events.FindAsync(eventId);
            if (eventEntity == null) return NotFound(new { message = "Event not found." });

            var category = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryID == dto.CategoryID && c.EventID == eventId);
            if (category == null) return BadRequest(new { message = "Category not found for this event." });

            var alreadyEnrolled = await _context.Enrolments.AnyAsync(e => e.ParticipantUserID == userId.Value && e.EventID == eventId);
            if (alreadyEnrolled) return Conflict(new { message = "Already enrolled in this event." });

            var enrolment = new Enrolment
            {
                ParticipantUserID = userId.Value,
                EventID = eventId,
                CategoryID = dto.CategoryID,
                Status = "Pending" // Default status
            };

            _context.Enrolments.Add(enrolment);
            await _context.SaveChangesAsync();

            return StatusCode(201, new { message = "Enrolled successfully.", enrolmentId = enrolment.EnrolmentID });
        }

        // GET: api/users/enrolments (Participant only)
        [HttpGet("users/enrolments")]
        public async Task<IActionResult> GetMyEnrolments()
        {
            var userId = GetCurrentUserId();
            var role = GetCurrentUserRole();

            if (userId == null) return Unauthorized(new { message = "Not logged in." });
            if (role != "Participant") return StatusCode(403, new { message = "User is not a Participant." });

            var enrolments = await _context.Enrolments
                .Include(e => e.Event)
                .Include(e => e.Category)
                .Where(e => e.ParticipantUserID == userId.Value)
                .Select(e => new EnrolmentResponseDto
                {
                    EnrolmentID = e.EnrolmentID,
                    ParticipantName = HttpContext.Session.GetString("FirstName") ?? "User",
                    EventName = e.Event!.Name,
                    EventDate = e.Event.Date,
                    CategoryName = e.Category!.Name,
                    Status = e.Status
                })
                .ToListAsync();

            return Ok(enrolments);
        }

        // GET: api/events/{eventId}/enrolments (Organiser only)
        [HttpGet("events/{eventId}/enrolments")]
        public async Task<IActionResult> GetEventEnrolments(int eventId)
        {
            var userId = GetCurrentUserId();
            var role = GetCurrentUserRole();

            if (userId == null) return Unauthorized(new { message = "Not logged in." });
            if (role != "Organiser") return StatusCode(403, new { message = "User is not an Organiser." });

            var eventEntity = await _context.Events.FindAsync(eventId);
            if (eventEntity == null) return NotFound(new { message = "Event not found." });
            if (eventEntity.CreatedByUserID != userId.Value) return StatusCode(403, new { message = "Not the organiser of this event." });

            var enrolments = await _context.Enrolments
                .Include(e => e.ParticipantUser)
                .Include(e => e.Category)
                .Where(e => e.EventID == eventId)
                .Select(e => new EnrolmentResponseDto
                {
                    EnrolmentID = e.EnrolmentID,
                    ParticipantName = e.ParticipantUser!.FirstName + " " + e.ParticipantUser.LastName,
                    EventName = eventEntity.Name,
                    EventDate = eventEntity.Date,
                    CategoryName = e.Category!.Name,
                    Status = e.Status
                })
                .ToListAsync();

            return Ok(enrolments);
        }

        // PUT: api/enrolments/{enrolmentId}/status (Organiser only)
        [HttpPut("enrolments/{enrolmentId}/status")]
        public async Task<IActionResult> UpdateEnrolmentStatus(int enrolmentId, [FromBody] UpdateEnrolmentStatusDto dto)
        {
            var userId = GetCurrentUserId();
            var role = GetCurrentUserRole();

            if (userId == null) return Unauthorized(new { message = "Not logged in." });
            if (role != "Organiser") return StatusCode(403, new { message = "User is not an Organiser." });

            if (dto.Status != "Pending" && dto.Status != "Confirmed" && dto.Status != "Cancelled")
                return BadRequest(new { message = "Invalid status value." });

            var enrolment = await _context.Enrolments
                .Include(e => e.Event)
                .FirstOrDefaultAsync(e => e.EnrolmentID == enrolmentId);

            if (enrolment == null) return NotFound(new { message = "Enrolment not found." });
            if (enrolment.Event!.CreatedByUserID != userId.Value) return StatusCode(403, new { message = "Not the organiser of this event." });

            enrolment.Status = dto.Status;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Status updated successfully." });
        }
    }
}