namespace Library.Models
{
    public class Book
    {
        public int Id { get; set; }

        public string Title { get; set; } = null!;
        public string Author { get; set; } = null!;
        public string Isbn { get; set; } = null!;

        public int PublishedYear { get; set; }

        // Доступна ли книга для выдачи
        public bool IsAvailable { get; set; } = true;

        public string? Genre { get; set; }
        public int? Pages { get; set; }

        // Навигация
        public List<Loan> Loans { get; set; } = new();
    }
}
