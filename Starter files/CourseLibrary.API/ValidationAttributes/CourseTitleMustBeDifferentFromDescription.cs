using System.ComponentModel.DataAnnotations;
using CourseLibrary.API.Models;

namespace CourseLibrary.API.ValidationAttributes;

class CourseTitleMustBeDifferentFromDescritption : ValidationAttribute
{
   protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
   {
     CourseForManipulationDto? course = null;
     List<CourseForCreationDto>? courses = null;

     bool isCourse = validationContext.ObjectInstance is CourseForManipulationDto;
     bool isCourseCollection = validationContext.ObjectInstance is List<CourseForCreationDto>;

     if (!isCourse && !isCourseCollection)
     {
       throw new Exception($"Attribute {nameof(CourseTitleMustBeDifferentFromDescritption)} must" +
           $"be applied to a {nameof(CourseForManipulationDto)} or derived type.");
     }
     if (isCourse) course = (CourseForManipulationDto) validationContext.ObjectInstance;
     else if (isCourseCollection) courses = (List<CourseForCreationDto>) validationContext.ObjectInstance;


     if (course != null && course.Title == course.Description)
     {
       return new ValidationResult("The provided description should be different from the title.",
                                          new[] { nameof(CourseForManipulationDto) });
     }

     if (courses != null && courses.Any(courseManipDto => courseManipDto.Title == courseManipDto.Description))
     {
       return new ValidationResult("The provided description in the collection should be different from the title.",
                                          new[] { nameof(CourseForManipulationDto) });
     }

     return ValidationResult.Success;
   }
}
