using System.ComponentModel.DataAnnotations;
using CourseLibrary.API.ValidationAttributes;

namespace CourseLibrary.API.Models;

[CourseTitleMustBeDifferentFromDescritption]
public class CourseForManipulationDto //: IValidatableObject
{
  [Required(ErrorMessage = "You should fill out a title")]
  [MaxLength(100)]
  public string Title { get; set; } = string.Empty;
  [Required]
  [MaxLength(1500)]
  public virtual string Description { get; set; } = string.Empty;

  /*
  public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
  {
    if (Title == Description)
    {
      yield return new ValidationResult("Title can't be same as the Description", new[] {"Course"});
    }
  }
  */
}
