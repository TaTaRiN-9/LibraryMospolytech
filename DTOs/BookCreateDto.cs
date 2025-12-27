namespace Library.DTOs
{
    public class BookCreateDto
    {
        public string Title { get; set; } = null!;
        public string Author { get; set; } = null!;
        public string Isbn { get; set; } = null!;
        public int PublishedYear { get; set; }
    }
}
