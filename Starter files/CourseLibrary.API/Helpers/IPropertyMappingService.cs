namespace CourseLibrary.API.Helpers;

public interface IPropertyMappingService
{
  public Dictionary<string, PropertyMappingValue> GetPropertyMapping
    <TSource, TDestination>();
}
