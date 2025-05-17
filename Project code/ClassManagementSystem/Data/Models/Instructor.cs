using System.ComponentModel.DataAnnotations;

namespace ClassManagementSystem.Data.Models
{
    public class Instructor
    {
        public int Id { get; set; }
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
    }
}
