
namespace CourseLibrary.API.Helpers;
public static class DateTimeOffsetExtensions
{
  public static int GetCurrentAge(this DateTimeOffset dateTimeOffset,
      DateTimeOffset? dateOfDeath)
  {
    var currentDate = DateTime.UtcNow;

    int age = dateOfDeath == null ? (currentDate.Year - dateTimeOffset.Year):
                                    (dateOfDeath.Value.Year - dateTimeOffset.Year);

    if (currentDate < dateTimeOffset.AddYears(age))
    {
      age--;
    }

    return age;
  }
}

