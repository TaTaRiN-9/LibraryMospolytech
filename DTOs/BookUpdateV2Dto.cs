namespace Library.DTOs
{
    public class BookUpdateV2Dto
    {
        public string Title { get; set; } = null!;
        public string Author { get; set; } = null!;
        public int PublishedYear { get; set; }
        public bool IsAvailable { get; set; }


        public string? Genre { get; set; }
        public int? Pages { get; set; }
    }
}
