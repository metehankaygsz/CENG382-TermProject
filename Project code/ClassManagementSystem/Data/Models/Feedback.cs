using System.ComponentModel.DataAnnotations;

namespace ClassManagementSystem.Data.Models 
{
    public class Feedback
    {
        public int Id { get; set; }
        [Display(Name = "Classroom ID")]
        public int? ClassroomId { get; set; }
        public Classroom? Classroom { get; set; }
        [Display(Name = "Instructor Name")]
        public string? InstructorName { get; set; }
        public int? Rating { get; set; }
        public string? Comments { get; set; }
        [Display(Name = "Submitted On")]
        public DateTime? SubmittedOn { get; set; }
    }
}
