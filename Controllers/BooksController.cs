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
        public async Task<IActionResult> GetAll()
        {
            var books = await _context.Books.ToListAsync();

            var books_res = await _context.Books
                .Select(b => new BookShortDto
                {
                    Id = b.Id,
                    Title = b.Title,
                    Author = b.Author
                })
                .ToListAsync();

            return Ok(books_res);
        }

        // GET /api/v1/books/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var book = await _context.Books.FindAsync(id);

            if (book == null)
                return NotFound();

            return Ok(book);
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
    }
}
