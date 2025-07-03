namespace backend.Dto
{
    public class AccountDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public required string Username { get; set; }
        public string Email { get; set; } = string.Empty;
        public required string Password { get; set; }
    }
}
