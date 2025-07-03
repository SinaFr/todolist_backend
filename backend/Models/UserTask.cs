using System.Text.Json.Serialization;

namespace backend.Models
{

    public enum PriorityLevel
    {
        Low,
        Medium,
        High
    }

    public class UserTask
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public PriorityLevel Priority { get; set; }

        public DateTime TerminationDate { get; set; }

        public Boolean IsDone { get; set; }

        public int AccountId { get; set; }

        /* Navigation Property */
        public Account Account { get; set; } = null!;
    }
}
