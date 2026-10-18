using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers
{
    [Route("api")]
    [ApiController]
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ResultsController(RaceDayDbContext context)
        {
            _context = context;
        }

        private int? GetCurrentUserId() => HttpContext.Session.GetInt32("UserID");
        private string? GetCurrentUserRole() => HttpContext.Session.GetString("Role");

        // POST: api/events/{eventId}/results (Organiser only)
        [HttpPost("events/{eventId}/results")]
        public async Task<IActionResult> CaptureResults(int eventId, [FromBody] List<CaptureResultDto> dtos)
        {
            var userId = GetCurrentUserId();
            var role = GetCurrentUserRole();

            if (userId == null) return Unauthorized(new { message = "Not logged in." });
            if (role != "Organiser") return StatusCode(403, new { message = "User is not an Organiser." });

            var eventEntity = await _context.Events.FindAsync(eventId);
            if (eventEntity == null) return NotFound(new { message = "Event not found." });
            if (eventEntity.CreatedByUserID != userId.Value) return StatusCode(403, new { message = "Not the organiser of this event." });

            int createdCount = 0;

            foreach (var dto in dtos)
            {
                var enrolment = await _context.Enrolments
                    .FirstOrDefaultAsync(e => e.EnrolmentID == dto.EnrolmentID && e.EventID == eventId);

                if (enrolment == null) continue; // Skip if enrolment doesn't belong to this event

                var existingResult = await _context.Results.FirstOrDefaultAsync(r => r.EnrolmentID == dto.EnrolmentID);

                if (existingResult != null)
                {
                    // Update existing result
                    existingResult.FinishTime = dto.FinishTime;
                    existingResult.FinishPosition = dto.FinishPosition;
                    existingResult.DateCaptured = DateTime.Now;
                }
                else
                {
                    // Create new result
                    var result = new Result
                    {
                        EnrolmentID = dto.EnrolmentID,
                        FinishTime = dto.FinishTime,
                        FinishPosition = dto.FinishPosition
                    };
                    _context.Results.Add(result);
                }
                createdCount++;
            }

            await _context.SaveChangesAsync();
            return StatusCode(201, new { message = "Results captured successfully.", createdCount });
        }

        // GET: api/users/results (Participant only)
        [HttpGet("users/results")]
        public async Task<IActionResult> GetMyResults()
        {
            var userId = GetCurrentUserId();
            var role = GetCurrentUserRole();

            if (userId == null) return Unauthorized(new { message = "Not logged in." });
            if (role != "Participant") return StatusCode(403, new { message = "User is not a Participant." });

            var results = await _context.Results
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e.Event)
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e.Category)
                .Where(r => r.Enrolment!.ParticipantUserID == userId.Value)
                .Select(r => new ResultResponseDto
                {
                    ResultID = r.ResultID,
                    EventName = r.Enrolment!.Event!.Name,
                    EventDate = r.Enrolment.Event.Date,
                    CategoryName = r.Enrolment.Category!.Name,
                    FinishTime = r.FinishTime,
                    FinishPosition = r.FinishPosition
                })
                .ToListAsync();

            return Ok(results);
        }
    }
}