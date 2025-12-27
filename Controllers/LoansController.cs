using Library.Data;
using Library.DTOs;
using Library.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Library.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/loans")]
    [Authorize]
    public class LoansController : ControllerBase
    {
        private readonly AppDbContext _context;

        private static readonly Dictionary<string, int> IdempotencyStore = new();

        public LoansController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Создать выдачу книги
        /// </summary>
        /// <param name="dto">Данные выдачи</param>
        /// <param name="idempotencyKey">Idempotency-Key для предотвращения дублирования</param>
        [HttpPost]
        public async Task<IActionResult> CreateLoan(
            LoanCreateDto dto,
            [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey)
        {
            var userId = int.Parse(User.Claims
                .First(c => c.Type == ClaimTypes.NameIdentifier).Value);

            // Проверка идемпотентности
            if (!string.IsNullOrEmpty(idempotencyKey) &&
                IdempotencyStore.TryGetValue(idempotencyKey, out var existingLoanId))
            {
                var existingLoan = await _context.Loans
                    .Include(l => l.Book)
                    .FirstOrDefaultAsync(l => l.Id == existingLoanId);

                if (existingLoan == null)
                    return NotFound();

                return Ok(new LoanResponseDto
                {
                    Id = existingLoan.Id,
                    LoanDate = existingLoan.LoanDate,
                    ReturnDate = existingLoan.ReturnDate,
                    Book = new BookShortDto
                    {
                        Id = existingLoan.Book!.Id,
                        Title = existingLoan.Book.Title,
                        Author = existingLoan.Book.Author
                    }
                });
            }

            var book = await _context.Books.FindAsync(dto.BookId);
            if (book == null)
                return NotFound("Книга не найдена");

            if (!book.IsAvailable)
                return BadRequest("Данной книги нет в наличии");

            // Создаём выдачу
            var loan = new Loan
            {
                BookId = book.Id,
                UserId = userId
            };

            book.IsAvailable = false;

            _context.Loans.Add(loan);
            await _context.SaveChangesAsync();

            // Сохраняем идемпотентный ключ
            if (!string.IsNullOrEmpty(idempotencyKey))
            {
                IdempotencyStore[idempotencyKey] = loan.Id;
            }

            var response = new LoanResponseDto
            {
                Id = loan.Id,
                LoanDate = loan.LoanDate,
                ReturnDate = loan.ReturnDate,
                Book = new BookShortDto
                {
                    Id = book.Id,
                    Title = book.Title,
                    Author = book.Author
                }
            };

            return CreatedAtAction(nameof(GetMyLoans), new { }, response);
        }

        // POST /api/v1/loans/{id}/return
        // Вернуть книгу
        [HttpPost("{id}/return")]
        public async Task<IActionResult> ReturnBook(int id)
        {
            var userId = int.Parse(User.Claims
                .First(c => c.Type == ClaimTypes.NameIdentifier).Value);

            var loan = await _context.Loans
                .Include(l => l.Book)
                .FirstOrDefaultAsync(l => l.Id == id && l.UserId == userId);

            if (loan == null)
                return NotFound();

            if (loan.ReturnDate != null)
                return BadRequest("Книга уже возвращена");

            loan.ReturnDate = DateTime.UtcNow;
            loan.Book!.IsAvailable = true;

            await _context.SaveChangesAsync();

            return Ok(new LoanResponseDto
            {
                Id = loan.Id,
                LoanDate = loan.LoanDate,
                ReturnDate = loan.ReturnDate,
                Book = new BookShortDto
                {
                    Id = loan.Book!.Id,
                    Title = loan.Book.Title,
                    Author = loan.Book.Author
                }
            });
        }

        // GET /api/v1/loans/my
        // Мои выдачи
        [HttpGet("my")]
        public async Task<IActionResult> GetMyLoans()
        {
            var userId = int.Parse(User.Claims
                .First(c => c.Type == ClaimTypes.NameIdentifier).Value);

            var loans = await _context.Loans
                .AsNoTracking()
                .Include(l => l.Book)
                .Where(l => l.UserId == userId)
                .Select(l => new LoanResponseDto
                {
                    Id = l.Id,
                    LoanDate = l.LoanDate,
                    ReturnDate = l.ReturnDate,
                    Book = new BookShortDto
                    {
                        Id = l.Book!.Id,
                        Title = l.Book.Title,
                        Author = l.Book.Author
                    }
                })
                .ToListAsync();

            return Ok(loans);
        }
    }
}
