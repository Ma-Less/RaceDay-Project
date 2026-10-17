using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers
{
    [Route("api/events/{eventId}/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public CategoriesController(RaceDayDbContext context)
        {
            _context = context;
        }

        private int? GetCurrentUserId() => HttpContext.Session.GetInt32("UserID");
        private string? GetCurrentUserRole() => HttpContext.Session.GetString("Role");

        // GET: api/events/{eventId}/categories
        [HttpGet]
        public async Task<IActionResult> GetCategories(int eventId)
        {
            if (GetCurrentUserId() == null) return Unauthorized(new { message = "Not logged in." });

            var eventExists = await _context.Events.AnyAsync(e => e.EventID == eventId);
            if (!eventExists) return NotFound(new { message = "Event not found." });

            var categories = await _context.Categories
                .Where(c => c.EventID == eventId)
                .Select(c => new CategoryResponseDto
                {
                    CategoryID = c.CategoryID,
                    Name = c.Name,
                    Description = c.Description
                })
                .ToListAsync();

            return Ok(categories);
        }

        // POST: api/events/{eventId}/categories (Organiser only)
        [HttpPost]
        public async Task<IActionResult> AddCategory(int eventId, [FromBody] CreateCategoryDto dto)
        {
            var userId = GetCurrentUserId();
            var role = GetCurrentUserRole();

            if (userId == null) return Unauthorized(new { message = "Not logged in." });
            if (role != "Organiser") return StatusCode(403, new { message = "User is not an Organiser." });

            var eventEntity = await _context.Events.FindAsync(eventId);
            if (eventEntity == null) return NotFound(new { message = "Event not found." });

            if (eventEntity.CreatedByUserID != userId.Value)
                return StatusCode(403, new { message = "User is not the organiser of this event." });

            var category = new Category
            {
                EventID = eventId,
                Name = dto.Name,
                Description = dto.Description
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return StatusCode(201, new { message = "Category created successfully.", categoryId = category.CategoryID });
        }

        // PUT: api/events/{eventId}/categories/{categoryId} (Organiser only)
        [HttpPut("{categoryId}")]
        public async Task<IActionResult> UpdateCategory(int eventId, int categoryId, [FromBody] UpdateCategoryDto dto)
        {
            var userId = GetCurrentUserId();
            var role = GetCurrentUserRole();

            if (userId == null) return Unauthorized(new { message = "Not logged in." });
            if (role != "Organiser") return StatusCode(403, new { message = "User is not an Organiser." });

            var category = await _context.Categories.Include(c => c.Event).FirstOrDefaultAsync(c => c.CategoryID == categoryId && c.EventID == eventId);
            if (category == null) return NotFound(new { message = "Category not found." });

            if (category.Event!.CreatedByUserID != userId.Value)
                return StatusCode(403, new { message = "User is not the organiser of this event." });

            category.Name = dto.Name;
            category.Description = dto.Description;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Category updated successfully." });
        }

        // DELETE: api/events/{eventId}/categories/{categoryId} (Organiser only)
        [HttpDelete("{categoryId}")]
        public async Task<IActionResult> DeleteCategory(int eventId, int categoryId)
        {
            var userId = GetCurrentUserId();
            var role = GetCurrentUserRole();

            if (userId == null) return Unauthorized(new { message = "Not logged in." });
            if (role != "Organiser") return StatusCode(403, new { message = "User is not an Organiser." });

            var category = await _context.Categories.Include(c => c.Event).FirstOrDefaultAsync(c => c.CategoryID == categoryId && c.EventID == eventId);
            if (category == null) return NotFound(new { message = "Category not found." });

            if (category.Event!.CreatedByUserID != userId.Value)
                return StatusCode(403, new { message = "User is not the organiser of this event." });

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}