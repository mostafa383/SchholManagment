using Microsoft.AspNetCore.Mvc;
using SmartSchool.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using SmartSchool.Enums;
namespace SmartSchool.Controllers
{
    [Route("api/[controller]"), ApiController]
    public class TeacherController(UserManager<Teacher> userManager,
                                   RoleManager<IdentityRole> roleManager) : ControllerBase
    {
        // Example action to get all teachers
        [HttpGet("GetTeacher"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetTeacher()
        {
            var teacherId = User.Claims.FirstOrDefault(e => ClaimTypes.NameIdentifier == e.Type)?.Value;
            return Ok(await userManager.Users.FirstOrDefaultAsync(e => e.Id == teacherId));
        }

        [HttpPost("AddRoles")]
        public async Task<IActionResult> AddRoles()
        {
            foreach (var role in Roles.GetNames<Roles>())
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
            return Ok(roleManager.Roles.Select(e=> new
            {
                e.Id,
                e.Name
            }));
        }
    }
}