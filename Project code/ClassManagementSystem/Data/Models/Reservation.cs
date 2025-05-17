using System.ComponentModel.DataAnnotations;

namespace ClassManagementSystem.Data.Models;
public class Reservation {
  public int?       Id             { get; set; }
  [Display(Name = "Term ID")]
  public int? TermId { get; set; }
  public Term?      Term           { get; set; }
  [Display(Name = "Instructor Name")]
  public string? InstructorName { get; set; }
  [Display(Name = "Classroom ID")]
  public int? ClassroomId { get; set; }
  public Classroom? Classroom       { get; set; }
  public DateTime?  Date           { get; set; }
  [Display(Name = "Start Time")]
  public TimeSpan? StartTime { get; set; }
  [Display(Name = "End Time")]
  public TimeSpan? EndTime { get; set; }
  [Display(Name = "Is Approved")]
  public bool? IsApproved { get; set; }  
  [Display(Name = "Is Recurring")]
  public bool IsRecurring { get; set; }
  [Display(Name = "Recurs On")]
  public DayOfWeek? RecurringDayOfWeek { get; set; } 
  public DateTime? RecurrenceStartDate { get; set; }
  public DateTime? RecurrenceEndDate { get; set; }
}