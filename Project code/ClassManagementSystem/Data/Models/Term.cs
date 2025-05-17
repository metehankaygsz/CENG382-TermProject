using System.ComponentModel.DataAnnotations;

namespace ClassManagementSystem.Data.Models;
public class Term {
  public int      Id        { get; set; }
  public string   Name      { get; set; } = default!;
  [Display(Name = "Start Date")]
  public DateTime StartDate { get; set; }
  [Display(Name = "End Date")]
  public DateTime EndDate { get; set; }
  [Display(Name = "Active")]
  public bool IsActive { get; set; } 
}