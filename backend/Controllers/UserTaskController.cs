using backend.DTOs;
using backend.Models;
using backend.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace backend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("")]
    public class TaskController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TaskController(ApplicationDbContext context)
        {
            _context = context;
        }

        /* Mapping Entity to DTO */
        private UserTaskDto ToDto(UserTask task) => new UserTaskDto
        {
            Id = task.Id,
            Name = task.Name,
            Description = task.Description,
            Priority = task.Priority,
            TerminationDate = task.TerminationDate,
            IsDone = task.IsDone
        };

        /* Mapping DTO to Entity */
        private void UpdateEntityFromDto(UserTask task, UserTaskDto dto)
        {
            task.Name = dto.Name;
            task.Description = dto.Description;
            task.Priority = dto.Priority;
            task.TerminationDate = dto.TerminationDate;
            task.IsDone = dto.IsDone;
        }

        [HttpGet("accounts/{accountId}/tasks")]
        public async Task<IActionResult> GetTasksForAccount(int accountId)
        {
            var account = await _context.Accounts.FindAsync(accountId);
            if (account == null)
                return NotFound("Account not found.");

            var tasks = await _context.UserTasks
                .Where(t => t.AccountId == accountId)
                .ToListAsync();

            var dtos = tasks.Select(ToDto);
            return Ok(dtos);
        }

        [HttpGet("tasks/{id}")]
        public async Task<IActionResult> GetTaskById(int id)
        {
            var task = await _context.UserTasks.FindAsync(id);
            if (task == null)
                return NotFound();

            return Ok(ToDto(task));
        }

        [HttpPost("tasks")]
        public async Task<IActionResult> CreateTask([FromBody] UserTaskDto taskDto)
        {
            var username = User.FindFirstValue(ClaimTypes.Name);
            if (username == null)
                return Unauthorized();

            var account = await _context.Accounts.SingleOrDefaultAsync(a => a.Username == username);
            if (account == null)
                return Unauthorized();

            var task = new UserTask();
            UpdateEntityFromDto(task, taskDto);

            task.AccountId = account.Id;

            _context.UserTasks.Add(task);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTaskById), new { id = task.Id }, ToDto(task));
        }

        [HttpPut("tasks/{id}")]
        public async Task<IActionResult> UpdateTask(int id, [FromBody] UserTaskDto taskDto)
        {
            var task = await _context.UserTasks.FindAsync(id);
            if (task == null)
                return NotFound();

            UpdateEntityFromDto(task, taskDto);

            await _context.SaveChangesAsync();
            return Ok(ToDto(task));
        }

        [HttpDelete("tasks/{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var task = await _context.UserTasks.FindAsync(id);
            if (task == null)
                return NotFound();

            _context.UserTasks.Remove(task);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
