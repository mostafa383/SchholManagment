using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SmartSchool.Data;
using SmartSchool.Enums;
using SmartSchool.Models;
namespace SmartSchool.Controllers
{

    [Route("api/[controller]"),ApiController]
    public class AuthController(UserManager<Teacher> userManager,
                                IConfiguration configuration,
                                SchoolContext context) : ControllerBase
    {
        // استخدم هذا الـ DTO لطلب تسجيل الدخول
        public class LoginModel
        {
            public required string Username { get; set; }
            public required string Password { get; set; }
        }
        
        public class RegisterDto
        {
            public required string Username { get; set; }
            public required string Password { get; set; }
            public required int DepartmentId { get; set; }
            public Roles Role { get; set; }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            // 1. البحث عن المستخدم
            var user = await userManager.FindByNameAsync(model.Username);
            if (user is null)
                return NotFound("This user isn't found");
            if (!await userManager.CheckPasswordAsync(user, model.Password))
                return BadRequest("This password is wrong");
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim("Email", model.Username) // unique token id
            };
            var secretKey = configuration["JWT:SecretKey"]; // use a strong key in real apps
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            // Serialize the token to a string
            var roles = await userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            var token = new JwtSecurityToken(
                issuer: configuration["JWT:Issuer"],              // who issued the token
                audience: configuration["JWT:Audience"],       // who can use the token
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1), // token expiration
                signingCredentials: creds
            );
            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            return Ok(new { Token = tokenString });
        }

       [HttpPost("Register")]
public async Task<IActionResult> Register([FromBody] RegisterDto model)
{
    // 1. بدء المعاملة (Transaction)
    using var transaction = await context.Database.BeginTransactionAsync();

    try
    {
        var user = new Teacher 
        {
            UserName = model.Username,
            Email = model.Username,
            Name = model.Username,
            DepartmentId = model.DepartmentId 
        };

        // 2. محاولة إنشاء المستخدم
        var createResult = await userManager.CreateAsync(user, model.Password);
        if (!createResult.Succeeded)
        {
            // إذا فشل الإنشاء: يتم رمي استثناء للخروج من Try والوصول إلى Rollback
            throw new Exception("User creation failed: " + string.Join(", ", createResult.Errors.Select(e => e.Description)));
        }

        // 3. محاولة إضافة الدور
        var roleResult = await userManager.AddToRoleAsync(user, model.Role.ToString());
        if (!roleResult.Succeeded)
        {
            // إذا فشل إضافة الدور: يتم رمي استثناء للخروج
            throw new Exception("Adding role failed: " + string.Join(", ", roleResult.Errors.Select(e => e.Description)));
        }

        // 4. إذا نجحت كلتا العمليتين: يتم حفظ التغييرات نهائيًا (Commit)
        await transaction.CommitAsync();
        return Ok("User registered successfully");
    }
    catch (Exception ex)
    {
        // 5. في حالة أي فشل (حتى لو كان من AddToRole): يتم التراجع (Rollback)
        await transaction.RollbackAsync();
        
        // **الآن يجب أن نعالج رسائل الخطأ**
        
        // إذا كان هناك خطأ بسبب فشل CreateAsync أو AddToRoleAsync، أعد الـ BadRequest
        if (ex.Message.Contains("creation failed") || ex.Message.Contains("role failed"))
        {
             // استخراج الأخطاء من الرسالة المرمية
             var errorMessage = ex.Message.Split(':').Last().Trim();
             // نحتاج إلى تمرير قائمة الأخطاء بشكل أنظف، لكن للسرعة:
             return BadRequest(new { Error = errorMessage });
        }
        
        // خطأ غير متوقع
        return StatusCode(500, "An unexpected error occurred during registration.");
    }
}

    }
}   