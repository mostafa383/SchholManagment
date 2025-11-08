using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartSchool.Data;

namespace SmartSchool.Controllers
{
    [ApiController, Route("api/[controller]")]
    public class CoursesController(SchoolContext context) : ControllerBase
    {
        [HttpGet("GetCourses")]
        public async Task<IActionResult> GetCourses()
        {
            return Ok(await context.Courses.
                Select(e => new
                {
                    e.Id,
                    e.Title,
                    e.Credits,
                    TeacherName = e.Teacher.Name,
                    StudentCount = e.StudentCourses.Count,
                }).ToListAsync());
        }

        [HttpGet("GetStudentCourses/{studentId}")]
        public async Task<IActionResult> GetStudentCourses(int studentId)
        {
            // var courses= await context.Courses
            //     .Where(c => c.StudentCourses.Any(sc => sc.StudentId == studentId))
            //     .Select(e => new
            //     {
            //         e.Id,
            //         e.Title,
            //         e.Credits,
            //         TeacherName = e.Teacher.Name,
            //     }).ToListAsync();
            var courses = await context.StudentCourses
                .Where(sc => sc.StudentId == studentId)
                .Select(sc => new
                {
                    sc.Course.Id,
                    sc.Course.Title,
                    sc.Course.Credits,
                    TeacherName = sc.Course.Teacher.Name,
                }).ToListAsync();
            return Ok(courses);
        }

        [HttpGet("GetTotalStudentCount")]
        public async Task<IActionResult> GetTotalStudentCount(DateTime? date)
        {
            if (date.HasValue)
            {
                var countByDate = await context.Students
                    .Where(s => s.RegistrationDate.Date == date.Value.Date)
                    .CountAsync();
                return Ok(countByDate);
            }
            else
            {
                var count = await context.Students.CountAsync();
                return Ok(count);
            }
        }

        [HttpPut("UpdateCourseTeacher/{courseId}/{teacherId}")]
        public async Task<IActionResult> UpdateCourseTeacher(int courseId, int teacherId)
        {
            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                var course = await context.Courses.FindAsync(courseId);
                if (course == null) return NotFound();

                var teacher = await context.Teachers.FindAsync(teacherId);
                if (teacher == null) return NotFound();

                course.TeacherId = teacherId;
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
                return NoContent();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "An error occurred while updating the course teacher.");
            }
        }

        [HttpDelete("DeleteStudentCourse/{studentId}/{courseId}")]
        public async Task<IActionResult> DeleteStudentCourse(int studentId, int courseId)
        {
            var studentCourse = await context.StudentCourses
                .FirstOrDefaultAsync(sc => sc.StudentId == studentId && sc.CourseId == courseId);
            if (studentCourse == null) return NotFound();
            context.StudentCourses.Remove(studentCourse);
            await context.SaveChangesAsync();
            return NoContent();
        }

    


    }
}