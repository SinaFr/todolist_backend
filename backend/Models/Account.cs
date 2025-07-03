namespace backend.Models
{
    public class Account
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public required string Username { get; set; }

        public string Email { get; set; } = string.Empty;

        public required string Password { get; set; }

        public List<UserTask> Tasks { get; set; } = new List<UserTask>();
    }
}
