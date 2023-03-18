using CourseLibrary.API.Services;

namespace CourseLibrary.API.Helpers;

public class PropertyMapping<TSource, TDestination> : IPropertyMapping
{
  public Dictionary<string, PropertyMappingValue> MappingDictionary { get; private set; }

  public PropertyMapping(Dictionary<string, PropertyMappingValue> mappingDict)
  {
    MappingDictionary = mappingDict ?? throw new ArgumentNullException(nameof(mappingDict));
  }

}
