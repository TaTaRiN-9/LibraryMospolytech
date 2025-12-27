using Library.Data;
using Library.DTOs;
using Library.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace Library.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/books")]
    public class BooksController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BooksController(AppDbContext context)
        {
            _context = context;
        }

        // GET /api/v1/books
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetBooks(
        int page = 1,
        int pageSize = 10,
        [FromQuery] string? include = null)
        {
            var totalBooks = await _context.Books.CountAsync();
            var books = await _context.Books
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var fields = include?.Split(',') ?? Array.Empty<string>();

            var result = books.Select(b =>
            {
                var obj = new Dictionary<string, object>();
                if (!fields.Any() || fields.Contains("Id")) obj["Id"] = b.Id;
                if (!fields.Any() || fields.Contains("Title")) obj["Title"] = b.Title;
                if (!fields.Any() || fields.Contains("Author")) obj["Author"] = b.Author;
                if (!fields.Any() || fields.Contains("PublishedYear")) obj["PublishedYear"] = b.PublishedYear;
                return obj;
            });

            Response.Headers.Append("X-Limit-Remaining", "5");
            Response.Headers.Append("Retry-After", "60");

            return Ok(new
            {
                Page = page,
                PageSize = pageSize,
                Total = totalBooks,
                Data = result
            });
        }

        // GET /api/v1/books/{id}
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id, [FromQuery] string? include = null)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();

            var fields = include?.Split(',') ?? Array.Empty<string>();
            var result = new Dictionary<string, object>();

            if (!fields.Any() || fields.Contains("Author")) result["Author"] = book.Author;
            if (!fields.Any() || fields.Contains("PublishedYear")) result["PublishedYear"] = book.PublishedYear;

            return Ok(result);
        }

        // POST /api/v1/books
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create(BookCreateDto dto)
        {
            // Проверка ISBN на уникальность
            if (await _context.Books.AnyAsync(b => b.Isbn == dto.Isbn))
                return BadRequest("Book with this ISBN already exists");

            var book = new Book
            {
                Title = dto.Title,
                Author = dto.Author,
                Isbn = dto.Isbn,
                PublishedYear = dto.PublishedYear,
                IsAvailable = true
            };

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = book.Id }, book);
        }

        // PUT /api/v1/books/{id}
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, BookUpdateDto dto)
        {
            var book = await _context.Books.FindAsync(id);

            if (book == null)
                return NotFound();

            book.Title = dto.Title;
            book.Author = dto.Author;
            book.PublishedYear = dto.PublishedYear;
            book.IsAvailable = dto.IsAvailable;

            await _context.SaveChangesAsync();
            return Ok(book);
        }

        // DELETE /api/v1/books/{id}
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var book = await _context.Books.FindAsync(id);

            if (book == null)
                return NotFound();

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("/internal/stats/books-count")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> GetBooksCount()
        {
            var count = await _context.Books.CountAsync();
            return Ok(new { Count = count });
        }
    }
}
