using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartSchool.Models;
namespace SmartSchool.Controllers
{

    [Route("api/[controller]"),ApiController]
    public class AuthController(UserManager<Teacher> userManager) : ControllerBase
    {
        // استخدم هذا الـ DTO لطلب تسجيل الدخول
    public class LoginModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
        // 1. البحث عن المستخدم
        var user= await userManager.FindByNameAsync(model.Username);
        if(user is null)
        {
            return NotFound("This user isn't found");
        }
        
        // 2. التحقق من كلمة المرور (باستخدام userManager.CheckPasswordAsync)
        // 3. (التحدي) توليد قائمة Claims (مطالبات) للمستخدم
        // 4. (التحدي الأكبر) توليد وتوقيع الـ JWT Token
        // 5. إرجاع التوكن في الـ Response
        }
    }
}   