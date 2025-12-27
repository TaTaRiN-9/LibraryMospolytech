namespace Library.DTOs
{
    public class BookUpdateDto
    {
        public string Title { get; set; } = null!;
        public string Author { get; set; } = null!;
        public int PublishedYear { get; set; }
        public bool IsAvailable { get; set; }
    }
}
