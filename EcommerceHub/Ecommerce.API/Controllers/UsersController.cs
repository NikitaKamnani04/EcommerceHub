
using Ecommerce.API.DTOs;
using Ecommerce.API.Services;
using Ecommerce.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ECommerceDbContext _context;
        private readonly TokenService _tokenService;

        public UsersController(ECommerceDbContext context, TokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _context.Users.Include(u => u.Profile)
                .Select(u => new UserResponseDto
                {
                    UserID = u.UserId,
                    Username = u.Username,
                    FullName = u.Profile.FullName,
                    Email = u.Profile.Email,
                    ContactNumber = u.Profile.ContactNumber,
                    CreatedAt = u.CreatedAt
                }).ToListAsync();

            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _context.Users.Include(u => u.Profile).Where(u => u.UserId == id)
                .Select(u => new UserResponseDto
                {
                    UserID = u.UserId,
                    Username = u.Username,
                    FullName = u.Profile.FullName,
                    Email = u.Profile.Email,
                    ContactNumber = u.Profile.ContactNumber,
                    CreatedAt = u.CreatedAt
                }).FirstOrDefaultAsync();

            if (user == null)
                return NotFound();

            return Ok(user);

        }

       
        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterDto dto)
        {
            var emailExists = await _context.UserProfiles.AnyAsync(p=>p.Email == dto.Email);
            if (emailExists)
                return BadRequest("Email is Already registered");

            var usernameExists = await _context.Users.AnyAsync(u=>u.Username == dto.Username);
            if (usernameExists)
                return BadRequest("Username is already taken");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Step 1: Password ko hash karo - BCrypt.HashPassword ek "salted hash" banata hai
                // (matlab har baar same password se bhi alag hash banega - extra security)
                var passwordhash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
                var profile = new UserProfile
                {
                    FullName = dto.FullName,
                    Email = dto.Email,
                    PasswordHash = passwordhash,
                    ContactNumber = dto.ContactNumber
                };

                _context.UserProfiles.Add(profile);
                await _context.SaveChangesAsync();

                var user = new User
                {
                    ProfileId = profile.UserId,
                    Username = dto.Username,
                    RoleId = 2
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { message = "Registration successful", userId = user.UserId });
            }
            catch(Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDto dto)
        {
            var user = await _context.Users.Include(u=>u.Profile).Include(u=>u.Role)
                .FirstOrDefaultAsync(u=>u.Profile.Email == dto.Email);

            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

            if (user == null)
                return Unauthorized("Invalid email or password");

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.Profile.PasswordHash);

            if (!isPasswordValid)
            {
                var failedlog = new UserLoginHistory
                {
                    UserId = user.UserId,
                    LoginTime = DateTime.Now,
                    Ipaddress = ipAddress,
                    LoginStatus = "Failed"
                };

                _context.UserLoginHistories.Add(failedlog);
                await _context.SaveChangesAsync();
                return Unauthorized("Invalid email or Password");
            }

            string roleName = user.Role?.RoleName ?? "Customer";

            var token = _tokenService.GenerateToken(user.UserId, user.Username, roleName);

            var loginlog = new UserLoginHistory
            {
                UserId = user.UserId,
                LoginTime = DateTime.Now,
                Ipaddress = ipAddress,
                LoginStatus = "Success"
            };

            _context.UserLoginHistories.Add(loginlog);
            await _context.SaveChangesAsync();

            return Ok(new 
            { message = "Login Sucessful",
                token = token,
                userId = user.UserId, 
                Username = user.Username, role = roleName }
            );
        }

    }
}
