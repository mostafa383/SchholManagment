using Microsoft.AspNetCore.Identity;

namespace SmartSchool.Models
{
    public class Teacher : IdentityUser
    {
        public string Name { get; set; } = null!;
        public DateTime HireDate { get; set; }

        public int DepartmentId { get; set; }
        public Department Department { get; set; } = null!;
        
        public ICollection<Course> Courses { get; set; } = new List<Course>();
    }
}