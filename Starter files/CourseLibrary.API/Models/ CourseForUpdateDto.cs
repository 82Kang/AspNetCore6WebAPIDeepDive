using System.ComponentModel.DataAnnotations;

namespace CourseLibrary.API.Models;

public class CourseForUpdateDto : CourseForManipulationDto
{
  [Required(ErrorMessage = "You should fill out a description")]
  [MaxLength(1500)]
  public override string Description { get; set; } = string.Empty;
}
