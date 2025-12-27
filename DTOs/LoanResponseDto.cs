namespace Library.DTOs
{
    public class LoanResponseDto
    {
        public int Id { get; set; }
        public DateTime LoanDate { get; set; }
        public DateTime? ReturnDate { get; set; }

        public BookShortDto Book { get; set; } = null!;
    }

    public class BookShortDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Author { get; set; } = null!;
    }
}
