namespace CourseLibrary.API.Services;

public interface IPropertyCheckerService
{
  public bool TypeHasProperties<T>(string? fields);
}
