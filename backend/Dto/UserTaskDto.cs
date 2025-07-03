using backend.Models;

namespace backend.DTOs
{
    public class UserTaskDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public PriorityLevel Priority { get; set; }
        public DateTime TerminationDate { get; set; }
        public bool IsDone { get; set; }
    }
}
