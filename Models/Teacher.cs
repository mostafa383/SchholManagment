namespace SmartSchool.Models
{
    public class Teacher
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public DateTime HireDate { get; set; }

        public ICollection<Course> Courses { get; set; } = new List<Course>();
    }
}