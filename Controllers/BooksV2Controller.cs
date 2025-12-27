using Library.Data;
using Library.DTOs;
using Library.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Library.Controllers
{
    [ApiController]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/books")]
    public class BooksV2Controller : ControllerBase
    {
        private readonly AppDbContext _context;

        public BooksV2Controller(AppDbContext context)
        {
            _context = context;
        }

        // GET /api/v2/books
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var books = await _context.Books.ToListAsync();
            return Ok(books);
        }

        // POST /api/v2/books
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create(BookCreateV2Dto dto)
        {
            if (await _context.Books.AnyAsync(b => b.Isbn == dto.Isbn))
                return BadRequest("Книга с таким книжным номером существует");

            var book = new Book
            {
                Title = dto.Title,
                Author = dto.Author,
                Isbn = dto.Isbn,
                PublishedYear = dto.PublishedYear,
                Genre = dto.Genre,
                Pages = dto.Pages,
                IsAvailable = true
            };

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAll), new { }, book);
        }

        // PUT /api/v2/books/{id}
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, BookUpdateV2Dto dto)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
                return NotFound();

            book.Title = dto.Title;
            book.Author = dto.Author;
            book.PublishedYear = dto.PublishedYear;
            book.IsAvailable = dto.IsAvailable;
            book.Genre = dto.Genre;
            book.Pages = dto.Pages;

            await _context.SaveChangesAsync();
            return Ok(book);
        }
    }
}
