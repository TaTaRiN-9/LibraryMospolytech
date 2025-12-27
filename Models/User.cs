namespace Library.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public List<Loan> Loans { get; set; } = new();
    }
}
