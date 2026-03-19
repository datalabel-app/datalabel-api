using BCrypt.Net;
using DataLabeling.API.DTOs;
using DataLabeling.BLL;
using DataLabeling.DAL.Data;
using DataLabeling.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OfficeOpenXml;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace DataLabeling.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly EmailService _emailService;

        public AuthController(
            ApplicationDbContext context,
            IConfiguration configuration, EmailService emailService)
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
        }

        [Authorize(Roles = nameof(UserRole.Admin))]
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            var emailExists = await _context.Users
                .AnyAsync(x => x.Email == request.Email);

            if (emailExists)
                return BadRequest("Email already exists");

            string plainPassword = GenerateRandomPassword();
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(plainPassword);

            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                Password = hashedPassword,
                Role = request.Role ?? UserRole.Annotator,
                Status = "Active",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _ = System.Threading.Tasks.Task.Run(async () =>
            {
                await _emailService.SendAccountCreationEmailAsync(
                    user.Email,
                    user.FullName,
                    plainPassword
                );
            });

            return Ok(new
            {
                message = "Register success",
                userId = user.UserId,
                email = user.Email,
                role = user.Role
            });
        }

        [Authorize(Roles = nameof(UserRole.Admin))]
        [HttpGet("export-template")]
        public IActionResult ExportTemplate()
        {
            ExcelPackage.License.SetNonCommercialPersonal("Admin");

            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("Users");

            sheet.Cells[1, 1].Value = "FullName";
            sheet.Cells[1, 2].Value = "Email";
            sheet.Cells[1, 3].Value = "Role";

            sheet.Cells[2, 1].Value = "Nguyen Van A";
            sheet.Cells[2, 2].Value = "a@gmail.com";
            sheet.Cells[2, 3].Value = "Annotator";

            sheet.Cells[3, 1].Value = "Tran Thi B";
            sheet.Cells[3, 2].Value = "b@gmail.com";
            sheet.Cells[3, 3].Value = "Manager";

            var stream = new MemoryStream(package.GetAsByteArray());

            return File(stream,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "UserTemplate.xlsx");
        }

        [Authorize(Roles = nameof(UserRole.Admin))]
        [HttpPost("import-users")]
        public async Task<IActionResult> ImportUsers(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is empty");

            var usersCreated = new List<object>();
            var errors = new List<string>();

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);

            using var package = new OfficeOpenXml.ExcelPackage(stream);
            var sheet = package.Workbook.Worksheets[0];

            int rowCount = sheet.Dimension.Rows;

            for (int row = 2; row <= rowCount; row++)
            {
                try
                {
                    var fullName = sheet.Cells[row, 1].Text.Trim();
                    var email = sheet.Cells[row, 2].Text.Trim();
                    var roleText = sheet.Cells[row, 3].Text.Trim();

                    if (string.IsNullOrEmpty(email))
                    {
                        errors.Add($"Row {row}: Email is required");
                        continue;
                    }

                    var emailExists = await _context.Users
                        .AnyAsync(x => x.Email == email);

                    if (emailExists)
                    {
                        errors.Add($"Row {row}: Email already exists");
                        continue;
                    }

                    UserRole role = UserRole.Annotator;
                    if (!string.IsNullOrEmpty(roleText) &&
                        Enum.TryParse(roleText, out UserRole parsedRole))
                    {
                        role = parsedRole;
                    }

                    string plainPassword = GenerateRandomPassword();
                    string hashedPassword = BCrypt.Net.BCrypt.HashPassword(plainPassword);

                    var user = new User
                    {
                        FullName = fullName,
                        Email = email,
                        Password = hashedPassword,
                        Role = role,
                        Status = "Active",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();

                    _ = System.Threading.Tasks.Task.Run(async () =>
                    {
                        await _emailService.SendAccountCreationEmailAsync(
                            user.Email,
                            user.FullName,
                            plainPassword
                        );
                    });

                    usersCreated.Add(new
                    {
                        email = user.Email,
                        role = user.Role
                    });
                }
                catch (Exception ex)
                {
                    errors.Add($"Row {row}: {ex.Message}");
                }
            }

            return Ok(new
            {
                message = "Import completed",
                successCount = usersCreated.Count,
                errorCount = errors.Count,
                usersCreated,
                errors
            });
        }

        [AllowAnonymous]
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Email == request.Email);
            if (user!.Status != "Active") return BadRequest("Your account banned");

            if (user == null)
                return BadRequest("Email does not exist");

            var existingTokens = await _context.Token
                .Where(x => x.UserId == user.UserId 
                    && x.TokenType == "OTP" 
                    && !x.IsUsed 
                    && x.Expired > DateTime.UtcNow)
                .ToListAsync();

            foreach (var token in existingTokens)
            {
                token.IsUsed = true;
                token.UpdatedAt = DateTime.UtcNow;
            }

            string otp = GenerateOtp();

            var newToken = new Token
            {
                UserId = user.UserId,
                TokenType = "OTP",
                TokenValue = otp,
                IsUsed = false,
                Expired = DateTime.UtcNow.AddMinutes(5),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Token.Add(newToken);
            await _context.SaveChangesAsync();

            _ = System.Threading.Tasks.Task.Run(async () =>
            {
                await _emailService.SendForgotPasswordOtpEmailAsync(
                    user.Email,
                    user.FullName,
                    otp
                );
            });

            return Ok(new
            {
                message = "OTP has been sent to your email"
            });
        }

        [AllowAnonymous]
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp(VerifyOtpRequest request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Email == request.Email);

            if (user == null)
                return BadRequest("Email does not exist");

            var token = await _context.Token
                .Where(x => x.UserId == user.UserId
                    && x.TokenType == "OTP"
                    && x.TokenValue == request.Otp
                    && !x.IsUsed
                    && x.Expired > DateTime.UtcNow)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            if (token == null)
                return BadRequest("Invalid or expired OTP");

            return Ok(new
            {
                message = "OTP verified successfully"
            });
        }

        [AllowAnonymous]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Email == request.Email);

            if (user == null)
                return BadRequest("Email does not exist");

            var token = await _context.Token
                .Where(x => x.UserId == user.UserId
                    && x.TokenType == "OTP"
                    && x.TokenValue == request.Otp
                    && !x.IsUsed
                    && x.Expired > DateTime.UtcNow)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            if (token == null)
                return BadRequest("Invalid or expired OTP");

            user.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;
            user.IsChangePassword = true;

            token.IsUsed = true;
            token.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Password has been reset successfully"
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Email == request.Email);

            if (user == null)
                return Unauthorized("Invalid email or password");


            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(
                request.Password,
                user.Password
            );

            if (!isPasswordValid)
                return Unauthorized("Invalid email or password");
            if (user!.Status != "Active") return BadRequest("Your account banned");
            var token = GenerateJwtToken(user);

            return Ok(new
            {
                token,
                email = user.Email,
                role = user.Role,
                isChangePassword = user.IsChangePassword,
            });
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["Key"])
            );

            var creds = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256
            );

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    double.Parse(jwtSettings["ExpireMinutes"])
                ),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static string GenerateRandomPassword(int length = 6)
        {
            const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*";
            char[] password = new char[length];
            byte[] randomBytes = new byte[length];
            using var rdb = RandomNumberGenerator.Create();
            rdb.GetBytes(randomBytes);
            for (int i = 0; i < length; i++)
            {
                password[i] = validChars[randomBytes[i] % validChars.Length];
            }
            return new string(password);
        }

        private string GenerateOtp()
        {
            const string digits = "0123456789";
            var random = new Random();
            var otp = new StringBuilder(6);
            for (int i = 0; i < 6; i++)
            {
                otp.Append(digits[random.Next(digits.Length)]);
            }
            return otp.ToString();
        }
    }
}