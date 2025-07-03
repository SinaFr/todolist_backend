using Microsoft.AspNetCore.Mvc;
using backend.Data;
using backend.Models;
using backend.Dto;
using Microsoft.EntityFrameworkCore;
using backend.Security;
using Microsoft.AspNetCore.Authorization;

namespace backend.Controllers
{
    [ApiController]
    [Route("")]
    public class AccountController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly Jwt _jwt;
        private readonly IConfiguration _configuration;

        public AccountController(ApplicationDbContext context, Jwt jwt, IConfiguration configuration)
        {
            _context = context;
            _jwt = jwt;
            _configuration = configuration;
        }

        /* Mapping Entity to DTO */
        private static AccountDto ToDto(Account account) => new AccountDto
        {
            Id = account.Id,
            Name = account.Name,
            Username = account.Username,
            Email = account.Email,
            Password = string.Empty
        };

        /* Mapping DTO to Entity */
        private static void UpdateEntityFromDto(Account account, AccountDto dto)
        {
            account.Name = dto.Name;
            account.Username = dto.Username;
            account.Email = dto.Email;
        }

        [Authorize]
        [HttpGet("accounts")]
        public async Task<IActionResult> GetAllAccounts()
        {
            var accounts = await _context.Accounts.ToListAsync();
            var dtos = accounts.Select(ToDto);
            return Ok(dtos);
        }

        [Authorize]
        [HttpGet("accounts/{id}")]
        public async Task<IActionResult> GetAccountById(int id)
        {
            var account = await _context.Accounts.FindAsync(id);
            if (account == null)
                return NotFound("Account not found");

            return Ok(ToDto(account));
        }

        [Authorize]
        [HttpPut("accounts/{id}")]
        public async Task<IActionResult> UpdateAccount(int id, [FromBody] AccountDto updatedDto)
        {
            var existing = await _context.Accounts.FindAsync(id);
            if (existing == null)
                return NotFound("Account not found");

            bool usernameChanged = existing.Username != updatedDto.Username;

            UpdateEntityFromDto(existing, updatedDto);

            /* change password only if it is set */
            if (!string.IsNullOrWhiteSpace(updatedDto.Password))
                existing.Password = BCrypt.Net.BCrypt.HashPassword(updatedDto.Password);

            await _context.SaveChangesAsync();

            /* if username has changed, set new token */
            if (usernameChanged)
            {
                var token = _jwt.GenerateJwtToken(existing, _configuration["JwtSettings:Secret"]);
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTimeOffset.UtcNow.AddHours(1),
                    Path = "/"
                };
                Response.Cookies.Append("jwt", token, cookieOptions);
            }

            return Ok(ToDto(existing));
        }

        [HttpPost("signup")]
        public async Task<IActionResult> Register([FromBody] AccountDto model)
        {
            if (await _context.Accounts.AnyAsync(u => u.Username == model.Username))
                return BadRequest("Username already exists.");

            var user = new Account
            {
                Username = model.Username,
                Password = BCrypt.Net.BCrypt.HashPassword(model.Password)
            };

            /* Update other properties using the helper method */
            UpdateEntityFromDto(user, model);

            _context.Accounts.Add(user);
            await _context.SaveChangesAsync();

            return Created("", ToDto(user));
        }

        /* Login (creates JWT and sets cookie) */
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AccountDto model)
        {
            var user = await _context.Accounts.FirstOrDefaultAsync(u => u.Username == model.Username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
                return StatusCode(400, new { message = "Login not successful. Please try again!" });

            var token = _jwt.GenerateJwtToken(user, _configuration["JwtSettings:Secret"]);
            Console.WriteLine("token: " + token);

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = false, /* set in Produktion to true for HTTPS */
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddHours(1),
                Path = "/"
            };
            Response.Cookies.Append("jwt", token, cookieOptions);

            return Ok(new { message = "Login successful" });
        }

        /* checks for a valid token and returns username */
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetUserInfo()
        {
            if (!Request.Cookies.TryGetValue("jwt", out var token))
                return Unauthorized("No login");

            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized("User is not authenticated");
            }

            var username = _jwt.GetUsernameFromJwtToken(token);
            var user = await _context.Accounts.SingleOrDefaultAsync(a => a.Username == username);
            if (user == null)
                return Unauthorized();

            return Ok(ToDto(user));
        }

        /* used to get all tasks from a specific account */
        [HttpGet("api/account")]
        public async Task<IActionResult> GetAccount()
        {
            var username = User.Identity.Name;
            Console.WriteLine("Username" + username);

            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized(new { error = "Unauthorized" });
            }

            var account = await _context.Accounts.SingleOrDefaultAsync(a => a.Username == username);

            if (account != null)
            {
                return Ok(ToDto(account));
            }
            else
            {
                return NotFound(new { error = "Account not found" });
            }
        }

        /* used to check if username is available */
        [HttpGet("username-check/{username}")]
        public async Task<IActionResult> CheckUsernameAvailability(string username)
        {
            bool available = !await _context.Accounts.AnyAsync(a => a.Username == username);
            return Ok(new { available });
        }

        [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("jwt", new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Lax,
                Path = "/"
            });

            return Ok("Logout successful");
        }

        [Authorize]
        [HttpDelete("accounts/{id}")]
        public async Task<IActionResult> DeleteAccount(int id)
        {
            var account = await _context.Accounts.FindAsync(id);
            if (account == null)
                return NotFound("Account not found");

            _context.Accounts.Remove(account);
            await _context.SaveChangesAsync();

            /* if current user deltes itself, delete cookie */
            var currentUsername = User?.Identity?.Name;
            if (currentUsername != null && currentUsername == account.Username)
            {
                Response.Cookies.Delete("jwt");
            }

            return Ok("Account deleted");
        }
    }
}
