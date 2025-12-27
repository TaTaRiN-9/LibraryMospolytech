namespace Library.DTOs
{
    public class BookCreateV2Dto
    {
        public string Title { get; set; } = null!;
        public string Author { get; set; } = null!;
        public string Isbn { get; set; } = null!;
        public int PublishedYear { get; set; }


        public string? Genre { get; set; }
        public int? Pages { get; set; }
    }
}
